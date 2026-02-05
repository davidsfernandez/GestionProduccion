using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Hubs;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestionProduccion.Services;

public class ProductionOrderService : IProductionOrderService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<ProductionHub> _hubContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductionOrderService(AppDbContext context, IHubContext<ProductionHub> hubContext, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _hubContext = hubContext;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the ID of the authenticated user from the current HTTP context.
    /// Returns 1 (Admin) as fallback if no user is authenticated.
    /// In production, consider throwing an exception instead of using fallback.
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        // Fallback for development/testing - In production should throw exception
        return 1;
    }

    public async Task<ProductionOrder> CreateProductionOrder(ProductionOrder order)
    {
        // Business validations
        if (string.IsNullOrWhiteSpace(order.UniqueCode))
        {
            throw new InvalidOperationException("Production order unique code cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(order.ProductDescription))
        {
            throw new InvalidOperationException("Product description cannot be empty.");
        }

        if (order.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than 0.");
        }

        if (order.EstimatedDeliveryDate <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Estimated delivery date must be in the future.");
        }

        // Check that unique code doesn't already exist
        var existingOrder = await _context.ProductionOrders
            .FirstOrDefaultAsync(x => x.UniqueCode == order.UniqueCode);
        
        if (existingOrder != null)
        {
            throw new InvalidOperationException($"A production order with code '{order.UniqueCode}' already exists.");
        }

        order.CurrentStage = ProductionStage.Cutting;
        order.CurrentStatus = ProductionStatus.InProduction;
        order.CreationDate = DateTime.UtcNow;

        _context.ProductionOrders.Add(order);

        var userId = GetCurrentUserId();
        await AddHistory(order, null, order.CurrentStage, null, order.CurrentStatus, userId, "Creation of production order");

        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<ProductionOrder?> GetProductionOrderById(int orderId)
    {
        return await _context.ProductionOrders
            .Include(po => po.AssignedUser)
            .Include(po => po.History)
            .FirstOrDefaultAsync(po => po.Id == orderId);
    }

    public async Task<ProductionOrder> AssignTask(int orderId, int userId)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException("Production order not found.");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        
        if (user.Role != UserRole.Sewer && user.Role != UserRole.Workshop)
        {
            throw new InvalidOperationException("Task can only be assigned to a 'Sewer' or 'Workshop'.");
        }

        order.UserId = userId;
        _context.Entry(order).State = EntityState.Modified;
        
        var currentUserId = GetCurrentUserId();
        await AddHistory(order, order.CurrentStage, order.CurrentStage, order.CurrentStatus, order.CurrentStatus, 
            currentUserId, $"Assigned to {user.Name}");

        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<ProductionOrder> UpdateStatus(int orderId, ProductionStatus newStatus, string note)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException("Production order not found.");
        }

        // Validation: Only allow marking as Completed from Packaging stage
        if (newStatus == ProductionStatus.Completed && order.CurrentStage != ProductionStage.Packaging)
        {
            throw new InvalidOperationException("A production order can only be marked as Completed when in Packaging stage.");
        }

        // Validation: Don't allow changes if already completed
        if (order.CurrentStatus == ProductionStatus.Completed && newStatus != ProductionStatus.Completed)
        {
            throw new InvalidOperationException("Cannot change the status of a production order that is already completed.");
        }

        var previousStatus = order.CurrentStatus;
        order.CurrentStatus = newStatus;

        _context.Entry(order).State = EntityState.Modified;
        
        var userId = GetCurrentUserId();
        var completeNote = string.IsNullOrWhiteSpace(note) 
            ? $"Status changed from {previousStatus} to {newStatus}" 
            : note;
        
        await AddHistory(order, order.CurrentStage, order.CurrentStage, previousStatus, newStatus, userId, completeNote);

        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        return order;
    }

    public async Task<ProductionOrder> AdvanceStage(int orderId)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException("Production order not found.");
        }

        // Validation: Don't allow advancing stage if completed
        if (order.CurrentStatus == ProductionStatus.Completed)
        {
            throw new InvalidOperationException("Cannot change the stage of a production order that is already completed.");
        }

        var previousStage = order.CurrentStage;
        var previousStatus = order.CurrentStatus;

        var newStage = previousStage switch
        {
            ProductionStage.Cutting => ProductionStage.Sewing,
            ProductionStage.Sewing => ProductionStage.Review,
            ProductionStage.Review => ProductionStage.Packaging,
            ProductionStage.Packaging => throw new InvalidOperationException("Production order is already in final stage. Use status update method to complete it."),
            _ => throw new InvalidOperationException("Unknown production stage.")
        };
        
        order.CurrentStage = newStage;
        order.CurrentStatus = ProductionStatus.InProduction; // Reset status

        _context.Entry(order).State = EntityState.Modified;
        
        var userId = GetCurrentUserId();
        await AddHistory(order, previousStage, newStage, previousStatus, order.CurrentStatus, userId, 
            $"Advanced to {newStage}");

        await _context.SaveChangesAsync();
        
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        return order;
    }

    public async Task<DashboardDto> GetDashboard()
    {
        // Get stopped operations
        var stoppedOperations = await _context.ProductionOrders
            .Where(po => po.CurrentStatus == ProductionStatus.Stopped)
            .ToListAsync();

        // Get operations by user
        var operationsByUserDic = await _context.ProductionOrders
            .Where(po => po.UserId.HasValue)
            .Include(po => po.AssignedUser)
            .GroupBy(po => po.AssignedUser!.Name)
            .ToListAsync();

        var dashboard = new DashboardDto
        {
            OperationsByStage = await _context.ProductionOrders
                .GroupBy(po => po.CurrentStage)
                .Select(g => new { Stage = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Stage, x => x.Count),

            StoppedOperations = stoppedOperations,

            OperationsByUser = operationsByUserDic
                .ToDictionary(g => g.Key, g => g.ToList())
        };

        return dashboard;
    }
    
    /// <summary>
    /// Private method to add a record to the change history.
    /// Currently creates the record but does not persist it automatically.
    /// </summary>
    private async Task AddHistory(
        ProductionOrder order, 
        ProductionStage? previousStage, 
        ProductionStage newStage, 
        ProductionStatus? previousStatus, 
        ProductionStatus newStatus, 
        int userId, 
        string note)
    {
        var history = new ProductionHistory
        {
            ProductionOrderId = order.Id,
            PreviousStage = previousStage,
            NewStage = newStage,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            UserId = userId,
            ModificationDate = DateTime.UtcNow,
            Note = note
        };
        _context.ProductionHistories.Add(history);
    }
}
