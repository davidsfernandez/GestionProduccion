using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Hubs;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestionProduccion.Services;

public class ProductionOrderService : IProductionOrderService
{
    private readonly IProductionOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IFinancialCalculatorService _financialCalculator;
    private readonly IHubContext<ProductionHub> _hubContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductionOrderService(
        IProductionOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        IFinancialCalculatorService financialCalculator,
        IHubContext<ProductionHub> hubContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _financialCalculator = financialCalculator;
        _hubContext = hubContext;
        _httpContextAccessor = httpContextAccessor;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return 1;
    }

    public async Task<ProductionOrderDto> CreateProductionOrderAsync(CreateProductionOrderRequest request, int createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(request.UniqueCode))
            throw new InvalidOperationException("Production order unique code cannot be empty.");

        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than 0.");

        if (request.EstimatedDeliveryDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Estimated delivery date must be in the future.");

        if (request.ProductId.HasValue)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId.Value);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");
            
            if (string.IsNullOrWhiteSpace(request.ProductDescription))
                request.ProductDescription = $"{product.Name} ({product.FabricType})";

            if (product.AverageProductionTimeMinutes > 0)
            {
                // Auto-calculation logic could refine EstimatedDeliveryDate here if needed
            }
        }
        else if (string.IsNullOrWhiteSpace(request.ProductDescription))
        {
             throw new InvalidOperationException("Product description cannot be empty if no Product ID is provided.");
        }

        var existingOrder = await _orderRepository.GetByUniqueCodeAsync(request.UniqueCode);
        if (existingOrder != null)
            throw new InvalidOperationException($"A production order with code '{request.UniqueCode}' already exists.");

        var order = new ProductionOrder
        {
            UniqueCode = request.UniqueCode,
            ProductDescription = request.ProductDescription,
            Quantity = request.Quantity,
            EstimatedDeliveryDate = request.EstimatedDeliveryDate,
            ClientName = request.ClientName,
            Size = request.Size,
            CurrentStage = ProductionStage.Cutting,
            CurrentStatus = ProductionStatus.InProduction,
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow,
            UserId = request.UserId,
            ProductId = request.ProductId
        };

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        var historyNote = "Criação da ordem de produção";
        if (request.UserId.HasValue)
        {
            var assignedUser = await _userRepository.GetByIdAsync(request.UserId.Value);
            if (assignedUser != null) historyNote += $" e atribuído a {assignedUser.Name}";
        }

        await AddHistory(order.Id, null, order.CurrentStage, null, order.CurrentStatus, createdByUserId, historyNote);
        await _orderRepository.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        var createdOrder = await _orderRepository.GetByIdAsync(order.Id);

        return MapToDto(createdOrder!);
    }

    public async Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order == null ? null : MapToDto(order);
    }

    public async Task<List<ProductionOrderDto>> ListProductionOrdersAsync(FilterProductionOrderDto? filter)
    {
        var query = await _orderRepository.GetQueryableAsync();
        var currentUserId = GetCurrentUserId();
        var currentUser = await _userRepository.GetByIdAsync(currentUserId);

        if (currentUser != null && (currentUser.Role == UserRole.Operator || currentUser.Role == UserRole.Workshop))
        {
            query = query.Where(po => po.UserId == currentUserId);
        }

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.ProductDescription))
                query = query.Where(po => po.ProductDescription.Contains(filter.ProductDescription));
            
            if (!string.IsNullOrWhiteSpace(filter.CurrentStage) && Enum.TryParse<ProductionStage>(filter.CurrentStage, true, out var stage))
                query = query.Where(po => po.CurrentStage == stage);

            if (!string.IsNullOrWhiteSpace(filter.CurrentStatus) && Enum.TryParse<ProductionStatus>(filter.CurrentStatus, true, out var status))
                query = query.Where(po => po.CurrentStatus == status);

            if (filter.UserId.HasValue && filter.UserId.Value > 0)
                query = query.Where(po => po.UserId == filter.UserId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(po => po.CreationDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(po => po.CreationDate <= filter.EndDate.Value);
            
            if (!string.IsNullOrWhiteSpace(filter.ClientName))
                query = query.Where(po => po.ClientName != null && po.ClientName.Contains(filter.ClientName));

            if (!string.IsNullOrWhiteSpace(filter.Size))
                query = query.Where(po => po.Size != null && po.Size.Contains(filter.Size));
        }

        var ordersList = query.ToList();
        return ordersList.Select(MapToDto).ToList();
    }

    public async Task<bool> AssignTaskAsync(int orderId, int userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || (user.Role != UserRole.Operator && user.Role != UserRole.Workshop)) return false;

        order.UserId = userId;
        await _orderRepository.UpdateAsync(order);

        var currentUserId = GetCurrentUserId();
        await AddHistory(order.Id, order.CurrentStage, order.CurrentStage, order.CurrentStatus, order.CurrentStatus, currentUserId, $"Atribuído a {user.Name}");
        await _orderRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        return true;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, ProductionStatus newStatus, string note, int modifiedByUserId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        var user = await _userRepository.GetByIdAsync(modifiedByUserId);
        if (user != null && (user.Role == UserRole.Operator || user.Role == UserRole.Workshop))
        {
            if (order.UserId != modifiedByUserId) throw new UnauthorizedAccessException("Você só pode atualizar o status de ordens atribuídas a você.");
        }

        if (newStatus == ProductionStatus.Completed && order.CurrentStage != ProductionStage.Packaging) return false;
        if (order.CurrentStatus == ProductionStatus.Completed && newStatus != ProductionStatus.Completed) return false;

        var previousStatus = order.CurrentStatus;
        order.CurrentStatus = newStatus;

        // Timestamp Logic
        if (newStatus == ProductionStatus.InProduction && previousStatus != ProductionStatus.InProduction && order.ActualStartDate == null)
        {
            order.ActualStartDate = DateTime.UtcNow;
        }
        else if (newStatus == ProductionStatus.Completed && previousStatus != ProductionStatus.Completed)
        {
            order.ActualEndDate = DateTime.UtcNow;
        }

        await _orderRepository.UpdateAsync(order);

        var completeNote = string.IsNullOrWhiteSpace(note) ? $"Status alterado de {previousStatus} para {newStatus}" : note;
        await AddHistory(order.Id, order.CurrentStage, order.CurrentStage, previousStatus, newStatus, modifiedByUserId, completeNote);

        if (newStatus == ProductionStatus.Completed && previousStatus != ProductionStatus.Completed)
        {
            // Financial Calculation
            await _financialCalculator.CalculateFinalOrderCostAsync(order);

            if (order.ProductId.HasValue)
            {
                await RecalculateProductAverageTimeAsync(order.ProductId.Value, order.CreationDate, DateTime.UtcNow);
            }
        }

        await _orderRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        return true;
    }

    public async Task<BulkUpdateResult> BulkUpdateStatusAsync(List<int> orderIds, ProductionStatus newStatus, string note, int modifiedByUserId)
    {
        var result = new BulkUpdateResult();
        foreach (var id in orderIds)
        {
            try
            {
                if (await UpdateStatusAsync(id, newStatus, note, modifiedByUserId)) result.SuccessCount++;
                else { result.FailureCount++; result.Errors.Add($"Order {id}: Update failed."); }
            }
            catch (Exception ex) { result.FailureCount++; result.Errors.Add($"Order {id}: {ex.Message}"); }
        }
        return result;
    }

    public async Task<bool> AdvanceStageAsync(int orderId, int modifiedByUserId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        var user = await _userRepository.GetByIdAsync(modifiedByUserId);
        if (user != null && (user.Role == UserRole.Operator || user.Role == UserRole.Workshop))
        {
            if (order.UserId != modifiedByUserId) throw new UnauthorizedAccessException("Você só pode avançar a etapa de ordens atribuídas a você.");
        }

        if (order.CurrentStatus == ProductionStatus.Completed) return false;

        var previousStage = order.CurrentStage;
        var newStage = previousStage switch
        {
            ProductionStage.Cutting => ProductionStage.Sewing,
            ProductionStage.Sewing => ProductionStage.Review,
            ProductionStage.Review => ProductionStage.Packaging,
            _ => throw new InvalidOperationException("Unknown production stage or already final.")
        };

        order.CurrentStage = newStage;
        order.CurrentStatus = ProductionStatus.InProduction;

        await _orderRepository.UpdateAsync(order);
        await AddHistory(order.Id, previousStage, newStage, order.CurrentStatus, order.CurrentStatus, modifiedByUserId, $"Avançou para {newStage}");
        await _orderRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        return true;
    }

    public async Task<bool> ChangeStageAsync(int orderId, ProductionStage newStage, string note, int modifiedByUserId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        if (newStage < order.CurrentStage && string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("É obrigatório fornecer um comentário ao retornar para uma etapa anterior.");

        var user = await _userRepository.GetByIdAsync(modifiedByUserId);
        if (user != null && (user.Role == UserRole.Operator || user.Role == UserRole.Workshop))
        {
            if (order.UserId != modifiedByUserId) throw new UnauthorizedAccessException("Você só pode alterar o estágio de ordens atribuídas a você.");
        }

        var previousStage = order.CurrentStage;
        order.CurrentStage = newStage;
        order.CurrentStatus = ProductionStatus.InProduction;

        await _orderRepository.UpdateAsync(order);
        var actionText = newStage < previousStage ? "Retornou para" : "Alterou para";
        var historyNote = string.IsNullOrWhiteSpace(note) ? $"{actionText} {newStage}" : $"{actionText} {newStage}: {note}";
        
        await AddHistory(order.Id, previousStage, newStage, order.CurrentStatus, order.CurrentStatus, modifiedByUserId, historyNote);
        await _orderRepository.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", order.Id, order.CurrentStage.ToString(), order.CurrentStatus.ToString());

        return true;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var query = await _orderRepository.GetQueryableAsync();
        
        // Eager load related entities for the report and UI
        var ordersWithRelations = query
            .Include(o => o.AssignedUser)
            .Include(o => o.AssignedTeam)
            .AsNoTracking();

        var totalActiveOrders = ordersWithRelations.Count(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled);
        var completedToday = ordersWithRelations.Count(o => o.CurrentStatus == ProductionStatus.Completed && o.ActualEndDate >= today);

        var activeOrdersList = ordersWithRelations
            .Where(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled && o.UserId.HasValue)
            .ToList();

        var workloadDistribution = activeOrdersList
            .GroupBy(o => o.UserId!.Value)
            .Select((g, index) => new WorkerStatsDto
            {
                Name = g.First().AssignedUser?.Name ?? "Unknown",
                AvatarUrl = g.First().AssignedUser?.AvatarUrl ?? "/img/avatars/avatar.jpg",
                ActiveCount = g.Count(),
                EfficiencyScore = 95.0,
                Color = GetColorByIndex(index)
            })
            .OrderByDescending(w => w.ActiveCount)
            .ToList();

        var historyLogs = await _orderRepository.GetRecentHistoryAsync(10);
        var recentActivities = historyLogs.Select(h => new RecentActivityDto
        {
            OrderId = h.ProductionOrderId,
            UniqueCode = h.ProductionOrder?.UniqueCode ?? "N/A",
            UserName = h.ResponsibleUser?.Name ?? "System",
            Action = h.Note ?? h.NewStatus.ToString(),
            Date = h.ModificationDate
        }).ToList();

        var ordersByStage = query.Where(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled)
            .ToList() // Client evaluation for Enum grouping if EF fails
            .GroupBy(o => o.CurrentStage)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var totalAll = query.Count();
        var totalComp = ordersWithRelations.Count(o => o.CurrentStatus == ProductionStatus.Completed);
        var rate = totalAll > 0 ? (decimal)totalComp / totalAll * 100 : 0;

        var todaysOrdersList = ordersWithRelations.Where(o => o.CreationDate >= today || (o.ActualEndDate != null && o.ActualEndDate >= today)).OrderByDescending(o => o.CreationDate).ToList();
        var todaysOrdersDtos = todaysOrdersList.Select(MapToDto).ToList();

        return new DashboardDto
        {
            TotalActiveOrders = totalActiveOrders,
            CompletedToday = completedToday,
            AverageLeadTimeHours = 0,
            WeeklyVolumeData = new List<int> { 0, 0, 0, 0, 0, 0, 0 },
            WorkloadDistribution = workloadDistribution,
            OrdersByStage = ordersByStage,
            RecentActivities = recentActivities,
            TodaysOrders = todaysOrdersDtos,
            CompletionRate = rate,
            LastUpdated = DateTime.Now
        };
    }

    public async Task<List<ProductionHistoryDto>> GetHistoryByProductionOrderIdAsync(int orderId)
    {
        var history = await _orderRepository.GetHistoryByOrderIdAsync(orderId);
        return history.Select(h => new ProductionHistoryDto
        {
            Id = h.Id,
            ProductionOrderId = h.ProductionOrderId,
            PreviousStage = h.PreviousStage?.ToString() ?? "",
            NewStage = h.NewStage.ToString(),
            PreviousStatus = h.PreviousStatus?.ToString() ?? "",
            NewStatus = h.NewStatus.ToString(),
            UserId = h.UserId,
            UserName = h.ResponsibleUser?.Name ?? "Unknown",
            ModificationDate = h.ModificationDate,
            Note = h.Note ?? ""
        }).ToList();
    }

    private async Task AddHistory(int productionOrderId, ProductionStage? previousStage, ProductionStage newStage, ProductionStatus? previousStatus, ProductionStatus newStatus, int userId, string note)
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
        await _orderRepository.AddHistoryAsync(history);
    }

    private async Task RecalculateProductAverageTimeAsync(int productId, DateTime startDate, DateTime endDate)
    {
        var durationMinutes = (endDate - startDate).TotalMinutes;
        if (durationMinutes < 0) durationMinutes = 0;

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return;

        var query = await _orderRepository.GetQueryableAsync();
        var completedOrders = await query
            .AsNoTracking()
            .Where(o => o.ProductId == productId && o.CurrentStatus == ProductionStatus.Completed)
            .Select(o => new { o.CreationDate, o.ModificationDate })
            .ToListAsync();

        double totalMinutes = durationMinutes;
        int count = 1;

        foreach (var order in completedOrders)
        {
            var d = (order.ModificationDate - order.CreationDate).TotalMinutes;
            if (d > 0) { totalMinutes += d; count++; }
        }

        if (count > 0)
        {
            product.AverageProductionTimeMinutes = totalMinutes / count;
            await _productRepository.UpdateAsync(product);
        }
    }

    private ProductionOrderDto MapToDto(ProductionOrder order)
    {
        return new ProductionOrderDto
        {
            Id = order.Id,
            UniqueCode = order.UniqueCode,
            ProductDescription = order.ProductDescription,
            Quantity = order.Quantity,
            ClientName = order.ClientName,
            Size = order.Size,
            CurrentStage = order.CurrentStage.ToString(),
            CurrentStatus = order.CurrentStatus.ToString(),
            CreationDate = order.CreationDate,
            EstimatedDeliveryDate = order.EstimatedDeliveryDate,
            UserId = order.UserId,
            AssignedUserName = order.AssignedUser?.Name,
            SewingTeamId = order.SewingTeamId,
            SewingTeamName = order.AssignedTeam?.Name,
            Product = order.Product != null ? new ProductDto
            {
                Id = order.Product.Id,
                Name = order.Product.Name,
                InternalCode = order.Product.InternalCode,
                FabricType = order.Product.FabricType,
                MainSku = order.Product.MainSku,
                AverageProductionTimeMinutes = order.Product.AverageProductionTimeMinutes
            } : null
        };
    }

    private string GetColorByIndex(int index)
    {
        var colors = new[] { "#00C899", "#3B7DDD", "#fcb92c", "#dc3545", "#151628", "#6f42c1", "#e83e8c" };
        return colors[index % colors.Length];
    }
}
