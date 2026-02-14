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

    public async Task<ProductionOrderDto> CreateProductionOrderAsync(CreateProductionOrderRequest request, int createdByUserId)
    {
        // Business validations
        if (string.IsNullOrWhiteSpace(request.UniqueCode))
        {
            throw new InvalidOperationException("Production order unique code cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.ProductDescription))
        {
            throw new InvalidOperationException("Product description cannot be empty.");
        }

        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than 0.");
        }

        if (request.EstimatedDeliveryDate <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Estimated delivery date must be in the future.");
        }

        // Check that unique code doesn't already exist
        var existingOrder = await _context.ProductionOrders
            .FirstOrDefaultAsync(x => x.UniqueCode == request.UniqueCode);
        
        if (existingOrder != null)
        {
            throw new InvalidOperationException($"A production order with code '{request.UniqueCode}' already exists.");
        }

        var order = new ProductionOrder
        {
            UniqueCode = request.UniqueCode,
            ProductDescription = request.ProductDescription,
            Quantity = request.Quantity,
            EstimatedDeliveryDate = request.EstimatedDeliveryDate,
            CurrentStage = ProductionStage.Cutting,
            CurrentStatus = ProductionStatus.InProduction,
            CreationDate = DateTime.UtcNow,
            UserId = createdByUserId // Set createdByUserId
        };
        
        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync(); // Save to get the order.Id

        AddHistory(order.Id, null, order.CurrentStage, null, order.CurrentStatus, createdByUserId, "Creation of production order");
        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        return new ProductionOrderDto
        {
            Id = order.Id,
            UniqueCode = order.UniqueCode,
            ProductDescription = order.ProductDescription,
            Quantity = order.Quantity,
            CurrentStage = order.CurrentStage.ToString(),
            CurrentStatus = order.CurrentStatus.ToString(),
            CreationDate = order.CreationDate,
            EstimatedDeliveryDate = order.EstimatedDeliveryDate,
            UserId = order.UserId,
            AssignedUserName = order.AssignedUser?.Name
        };
    }

    public async Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id)
    {
        var order = await _context.ProductionOrders
            .Include(po => po.AssignedUser)
            .Include(po => po.History)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (order == null)
        {
            return null;
        }

        return new ProductionOrderDto
        {
            Id = order.Id,
            UniqueCode = order.UniqueCode,
            ProductDescription = order.ProductDescription,
            Quantity = order.Quantity,
            CurrentStage = order.CurrentStage.ToString(),
            CurrentStatus = order.CurrentStatus.ToString(),
            CreationDate = order.CreationDate,
            EstimatedDeliveryDate = order.EstimatedDeliveryDate,
            UserId = order.UserId,
            AssignedUserName = order.AssignedUser?.Name
        };
    }

    public async Task<List<ProductionOrderDto>> ListProductionOrdersAsync(FilterProductionOrderDto? filter)
    {
        var query = _context.ProductionOrders.AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.ProductDescription))
            {
                query = query.Where(po => po.ProductDescription.Contains(filter.ProductDescription));
            }

            if (!string.IsNullOrWhiteSpace(filter.CurrentStage))
            {
                if (Enum.TryParse<ProductionStage>(filter.CurrentStage, true, out var stage))
                {
                    query = query.Where(po => po.CurrentStage == stage);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.CurrentStatus))
            {
                if (Enum.TryParse<ProductionStatus>(filter.CurrentStatus, true, out var status))
                {
                    query = query.Where(po => po.CurrentStatus == status);
                }
            }

            if (filter.UserId.HasValue && filter.UserId.Value > 0)
            {
                query = query.Where(po => po.UserId == filter.UserId.Value);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(po => po.CreationDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(po => po.CreationDate <= filter.EndDate.Value);
            }
        }

        var orders = await query
            .Include(po => po.AssignedUser)
            .Select(po => new ProductionOrderDto
            {
                Id = po.Id,
                UniqueCode = po.UniqueCode,
                ProductDescription = po.ProductDescription,
                Quantity = po.Quantity,
                CurrentStage = po.CurrentStage.ToString(),
                CurrentStatus = po.CurrentStatus.ToString(),
                CreationDate = po.CreationDate,
                EstimatedDeliveryDate = po.EstimatedDeliveryDate,
                UserId = po.UserId,
                AssignedUserName = po.AssignedUser != null ? po.AssignedUser.Name : null
            })
            .ToListAsync();

        return orders;
    }

    public async Task<bool> AssignTaskAsync(int orderId, int userId)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }
        
        // Assuming UserRole.Sewer is now UserRole.Operator based on DTOs for UI translation
        if (user.Role != UserRole.Operator && user.Role != UserRole.Workshop)
        {
            // The action plan states Operator or Workshop.
            return false; 
        }

        order.UserId = userId;
        _context.Entry(order).State = EntityState.Modified;
        
        var currentUserId = GetCurrentUserId();
        AddHistory(order.Id, order.CurrentStage, order.CurrentStage, order.CurrentStatus, order.CurrentStatus, 
            currentUserId, $"Assigned to {user.Name}");

        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        return true;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }

        // Validation: Only allow marking as Completed from Packaging stage
        if (newStatus == ProductionStatus.Completed && order.CurrentStage != ProductionStage.Packaging)
        {
            return false;
        }

        // Validation: Don't allow changes if already completed
        if (order.CurrentStatus == ProductionStatus.Completed && newStatus != ProductionStatus.Completed)
        {
            return false;
        }

        var previousStatus = order.CurrentStatus;
        order.CurrentStatus = newStatus;

        _context.Entry(order).State = EntityState.Modified;
        
        var completeNote = string.IsNullOrWhiteSpace(note) 
            ? $"Status changed from {previousStatus} to {newStatus}" 
            : note;
        
        AddHistory(order.Id, order.CurrentStage, order.CurrentStage, previousStatus, newStatus, modifiedByUserId, completeNote);

        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        return true;
    }

    public async Task<bool> AdvanceStageAsync(int orderId, int modifiedByUserId)
    {
        var order = await _context.ProductionOrders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }

        // Validation: Don't allow advancing stage if completed
        if (order.CurrentStatus == ProductionStatus.Completed)
        {
            return false;
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
        
        AddHistory(order.Id, previousStage, newStage, previousStatus, order.CurrentStatus, modifiedByUserId, 
            $"Advanced to {newStage}");

        await _context.SaveChangesAsync();
        
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        return true;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        // 1. Fetch ALL Active Orders (Status != Completed && Status != Cancelled)
        // We include AssignedUser to get Avatar and Name (JOIN)
        var activeOrders = await _context.ProductionOrders
            .Where(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled)
            .Include(o => o.AssignedUser)
            .ToListAsync();

        // 2. Calculate Workload Distribution (Group by User)
        var userWorkloads = activeOrders
            .Where(o => o.UserId.HasValue && o.AssignedUser != null)
            .GroupBy(o => o.UserId)
            .Select((g, index) => new UserWorkloadDto
            {
                UserId = g.Key ?? 0,
                UserName = g.First().AssignedUser!.Name,
                AvatarUrl = g.First().AssignedUser!.AvatarUrl,
                TaskCount = g.Count(),
                Color = GetColorByIndex(index) // Assign distinct color
            })
            .OrderByDescending(w => w.TaskCount)
            .ToList();

        // 3. Stopped Operations (for Alerts)
        var pausedOrders = activeOrders
            .Where(o => o.CurrentStatus == ProductionStatus.Stopped)
            .Select(order => new ProductionOrderDto
            {
                Id = order.Id,
                UniqueCode = order.UniqueCode,
                ProductDescription = order.ProductDescription,
                Quantity = order.Quantity,
                CurrentStage = order.CurrentStage.ToString(),
                CurrentStatus = order.CurrentStatus.ToString(),
                CreationDate = order.CreationDate,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                UserId = order.UserId,
                AssignedUserName = order.AssignedUser?.Name
            })
            .ToList();

        // 4. Operations by Stage (Chart Data)
        var operationsByStage = activeOrders
            .GroupBy(o => o.CurrentStage.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // 5. Total Produced Units (All time)
        var totalProducedUnits = await _context.ProductionOrders
            .Where(o => o.CurrentStatus == ProductionStatus.Completed)
            .SumAsync(o => o.Quantity);

        // 6. Weekly Volume (Last 7 days history)
        var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-6);
        var completedHistory = await _context.ProductionHistories
            .Where(h => h.NewStatus == ProductionStatus.Completed && h.ModificationDate >= sevenDaysAgo)
            .ToListAsync();

        var volumeHistory = new List<ChartPointDto>();
        for (int i = 0; i < 7; i++)
        {
            var date = sevenDaysAgo.AddDays(i);
            var count = completedHistory.Count(h => h.ModificationDate.Date == date);
            volumeHistory.Add(new ChartPointDto
            {
                Label = date.ToString("dd/MM"),
                Value = count
            });
        }

        // 7. Recent Activity
        var recentHistory = await _context.ProductionHistories
            .Include(h => h.ResponsibleUser)
            .Include(h => h.ProductionOrder)
            .OrderByDescending(h => h.ModificationDate)
            .Take(10)
            .ToListAsync();

        var recentActivities = recentHistory.Select(h => new RecentActivityDto
        {
            OrderId = h.ProductionOrderId,
            UniqueCode = h.ProductionOrder?.UniqueCode ?? "N/A",
            UserName = h.ResponsibleUser?.Name ?? "System",
            Action = h.Note,
            Date = h.ModificationDate
        }).ToList();

        // 8. Urgent Orders
        var threeDaysFromNow = DateTime.UtcNow.AddDays(3);
        var urgentOrders = activeOrders
            .Where(o => o.EstimatedDeliveryDate <= threeDaysFromNow)
            .OrderBy(o => o.EstimatedDeliveryDate)
            .Select(order => new ProductionOrderDto
            {
                Id = order.Id,
                UniqueCode = order.UniqueCode,
                ProductDescription = order.ProductDescription,
                Quantity = order.Quantity,
                CurrentStage = order.CurrentStage.ToString(),
                CurrentStatus = order.CurrentStatus.ToString(),
                CreationDate = order.CreationDate,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                UserId = order.UserId,
                AssignedUserName = order.AssignedUser?.Name
            })
            .Take(5)
            .ToList();

        // Calculate Completion Rate
        var totalAllTime = await _context.ProductionOrders.CountAsync();
        var totalCompleted = await _context.ProductionOrders.CountAsync(o => o.CurrentStatus == ProductionStatus.Completed);
        var completionRate = totalAllTime > 0 
            ? Math.Round(((decimal)totalCompleted / (decimal)totalAllTime) * 100, 1)
            : 0;

        return new DashboardDto
        {
            UserWorkloads = userWorkloads, // NEW
            OperationsByStage = operationsByStage,
            StoppedOperations = pausedOrders,
            CompletionRate = completionRate,
            TotalProducedUnits = totalProducedUnits,
            EfficiencyTrend = 2.5, // Calc logic could be improved, keeping constant for now
            ProductionVolumeHistory = volumeHistory,
            RecentActivities = recentActivities,
            UrgentOrders = urgentOrders,
            AverageStageTime = new Dictionary<string, double>() // Simplify for now or keep existing calc
        };
    }

    private string GetColorByIndex(int index)
    {
        var colors = new[] 
        { 
            "#00C899", // Green
            "#3B7DDD", // Blue
            "#fcb92c", // Yellow
            "#dc3545", // Red
            "#151628", // Dark
            "#6f42c1", // Purple
            "#e83e8c"  // Pink
        };
        return colors[index % colors.Length];
    }

    private Dictionary<string, double> CalculateAverageStageTime(List<ProductionOrder> orders)
    {
        var stageDurations = new Dictionary<string, List<double>>();

        foreach (var order in orders)
        {
            var history = order.History.OrderBy(h => h.ModificationDate).ToList();
            for (int i = 0; i < history.Count; i++)
            {
                var current = history[i];
                DateTime? nextDate = (i + 1 < history.Count) ? history[i+1].ModificationDate : (order.CurrentStatus == ProductionStatus.Completed ? (DateTime?)null : DateTime.UtcNow);
                
                if (nextDate.HasValue)
                {
                    var duration = (nextDate.Value - current.ModificationDate).TotalHours;
                    var stageName = current.NewStage.ToString();
                    
                    if (!stageDurations.ContainsKey(stageName))
                        stageDurations[stageName] = new List<double>();
                    
                    stageDurations[stageName].Add(duration);
                }
            }
        }

        return stageDurations.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Any() ? Math.Round(kvp.Value.Average(), 1) : 0
        );
    }
    
    public async Task<List<ProductionHistoryDto>> GetHistoryByProductionOrderIdAsync(int orderId)
    {
        var history = await _context.ProductionHistories
            .Where(h => h.ProductionOrderId == orderId)
            .Include(h => h.ResponsibleUser)
            .Select(h => new ProductionHistoryDto
            {
                Id = h.Id,
                ProductionOrderId = h.ProductionOrderId,
                PreviousStage = h.PreviousStage.ToString(),
                NewStage = h.NewStage.ToString(),
                PreviousStatus = h.PreviousStatus.ToString(),
                NewStatus = h.NewStatus.ToString(),
                UserId = h.UserId,
                UserName = h.ResponsibleUser != null ? h.ResponsibleUser.Name : "Unknown",
                ModificationDate = h.ModificationDate,
                Note = h.Note
            })
            .ToListAsync();

        return history;
    }
    
    /// Private method to add a record to the change history.
    /// Currently creates the record but does not persist it automatically.
    /// </summary>
    private void AddHistory(
        int productionOrderId, 
        ProductionStage? previousStage, 
        ProductionStage newStage, 
        ProductionStatus? previousStatus, 
        ProductionStatus newStatus, 
        int userId, 
        string note)
    {
        var history = new ProductionHistory
        {
            ProductionOrderId = productionOrderId,
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
