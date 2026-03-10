using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;

namespace GestionProduccion.Application.Mapping;

public static class ManualMapper
{
    // --- PRODUCT MAPPING ---
    public static ProductDto ToDto(this Product entity)
    {
        if (entity == null) return null!;
        return new ProductDto
        {
            Id = entity.Id,
            Name = entity.Name,
            InternalCode = entity.InternalCode,
            FabricType = entity.FabricType,
            MainSku = entity.MainSku,
            AverageProductionTimeMinutes = entity.AverageProductionTimeMinutes,
            EstimatedSalePrice = entity.EstimatedSalePrice
        };
    }

    public static Product ToEntity(this ProductDto dto)
    {
        if (dto == null) return null!;
        return new Product
        {
            Id = dto.Id,
            Name = dto.Name,
            InternalCode = dto.InternalCode,
            FabricType = dto.FabricType,
            MainSku = dto.MainSku,
            AverageProductionTimeMinutes = dto.AverageProductionTimeMinutes,
            EstimatedSalePrice = dto.EstimatedSalePrice
        };
    }

    public static List<ProductDto> ToDtoList(this IEnumerable<Product> entities)
    {
        return entities?.Select(e => e.ToDto()).ToList() ?? new List<ProductDto>();
    }

    // --- USER MAPPING ---
    public static UserDto ToDto(this User entity)
    {
        if (entity == null) return null!;
        return new UserDto
        {
            Id = entity.Id,
            ExternalId = entity.ExternalId,
            FullName = entity.FullName,
            Email = entity.Email,
            AvatarUrl = entity.AvatarUrl,
            Role = entity.Role,
            IsActive = entity.IsActive,
            SewingTeamId = entity.SewingTeamId,
            SewingTeamName = entity.SewingTeam?.Name,
            AssignedOrdersCount = entity.AssignedOrders?.Count(o => o.CurrentStatus != ProductionStatus.Completed && o.CurrentStatus != ProductionStatus.Cancelled) ?? 0
        };
    }

    public static User ToEntity(this UserDto dto)
    {
        if (dto == null) return null!;
        return new User
        {
            Id = dto.Id,
            ExternalId = dto.ExternalId == Guid.Empty ? Guid.NewGuid() : dto.ExternalId,
            FullName = dto.FullName,
            Email = dto.Email,
            AvatarUrl = dto.AvatarUrl,
            Role = dto.Role,
            IsActive = dto.IsActive,
            SewingTeamId = dto.SewingTeamId
        };
    }

    public static List<UserDto> ToDtoList(this IEnumerable<User> entities)
    {
        return entities?.Select(e => e.ToDto()).ToList() ?? new List<UserDto>();
    }

    // --- BONUS RULE MAPPING ---
    public static BonusRuleDto ToDto(this BonusRule entity)
    {
        if (entity == null) return null!;
        return new BonusRuleDto
        {
            Id = entity.Id,
            ProductivityPercentage = (decimal)entity.ProductivityPercentage,
            DeadlineBonusPercentage = entity.DeadlineBonusPercentage,
            DefectLimitPercentage = entity.DefectLimitPercentage,
            DelayPenaltyPercentage = entity.DelayPenaltyPercentage,
            LastUpdate = entity.UpdatedAt
        };
    }

    // --- PRODUCTION ORDER SIZE MAPPING ---
    public static ProductionOrderSizeDto ToDto(this ProductionOrderSize entity)
    {
        if (entity == null) return null!;
        return new ProductionOrderSizeDto
        {
            Id = entity.Id,
            ProductionOrderId = entity.ProductionOrderId,
            Size = entity.Size,
            Quantity = entity.Quantity
        };
    }

    public static List<ProductionOrderSizeDto> ToDtoList(this IEnumerable<ProductionOrderSize> entities)
    {
        return entities?.Select(e => e.ToDto()).ToList() ?? new List<ProductionOrderSizeDto>();
    }

    // --- PRODUCTION ORDER MAPPING ---
    public static ProductionOrderDto ToDto(this ProductionOrder entity)
    {
        if (entity == null) return null!;
        var dto = new ProductionOrderDto
        {
            Id = entity.Id,
            LotCode = entity.LotCode,
            ProductName = entity.Product?.Name,
            ProductCode = entity.Product?.InternalCode,
            Quantity = entity.Quantity,
            ClientName = entity.ClientName,
            Size = entity.Size,
            CurrentStage = entity.CurrentStage.ToString(),
            CurrentStatus = entity.CurrentStatus.ToString(),
            CreatedAt = entity.CreatedAt,
            EstimatedCompletionAt = entity.EstimatedCompletionAt,
            UserId = entity.UserId,
            AssignedUserName = entity.AssignedUser?.FullName,
            AssignedUserAvatar = entity.AssignedUser?.AvatarUrl,
            SewingTeamId = entity.SewingTeamId,
            SewingTeamName = entity.AssignedTeam?.Name,
            TotalCost = entity.TotalCost,
            AverageCostPerPiece = entity.AverageCostPerPiece,
            ProfitMargin = entity.ProfitMargin,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            Product = entity.Product?.ToDto(),
            Sizes = entity.Sizes?.ToDtoList() ?? new List<ProductionOrderSizeDto>(),
            History = entity.History?.Select(h => new ProductionHistoryDto
            {
                Id = h.Id,
                ProductionOrderId = h.ProductionOrderId,
                PreviousStage = h.PreviousStage?.ToString() ?? "",
                NewStage = h.NewStage.ToString(),
                PreviousStatus = h.PreviousStatus?.ToString() ?? "",
                NewStatus = h.NewStatus.ToString(),
                UserId = h.UserId,
                UserName = h.ResponsibleUser?.FullName ?? "Unknown",
                ChangedAt = h.ChangedAt,
                Note = h.Note ?? ""
            }).ToList() ?? new List<ProductionHistoryDto>()
        };

        dto.EffectiveMinutes = entity.EffectiveMinutes;
        return dto;
    }

    public static List<ProductionOrderDto> ToDtoList(this IEnumerable<ProductionOrder> entities)
    {
        return entities?.Select(e => e.ToDto()).ToList() ?? new List<ProductionOrderDto>();
    }

    // --- QA DEFECT MAPPING ---
    public static QADefectDto ToDto(this QADefect entity)
    {
        if (entity == null) return null!;
        return new QADefectDto
        {
            Id = entity.Id,
            Reason = entity.Reason,
            Quantity = entity.Quantity,
            PhotoUrl = entity.PhotoUrl,
            ReportedAt = entity.ReportedAt,
            ReportedByUserId = entity.ReportedByUserId
        };
    }

    public static List<QADefectDto> ToDtoList(this IEnumerable<QADefect> entities)
    {
        return entities?.Select(e => e.ToDto()).ToList() ?? new List<QADefectDto>();
    }

    // --- OPERATIONAL TASK MAPPING ---
    public static TaskDto ToDto(this OperationalTask entity)
    {
        if (entity == null) return null!;
        return new TaskDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            AssignedUserName = entity.AssignedUser?.FullName ?? "N/A",
            Status = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            Deadline = entity.Deadline,
            ProgressPercentage = entity.CalculateProgress()
        };
    }

    private static double CalculateProgress(this OperationalTask t)
    {
        if (t.Status == OpTaskStatus.Completed) return 100;
        if (t.Deadline == null || t.Deadline <= t.CreatedAt) return 0;

        var total = (t.Deadline.Value - t.CreatedAt).TotalSeconds;
        var elapsed = (DateTime.UtcNow - t.CreatedAt).TotalSeconds;
        var progress = (elapsed / total) * 100;
        return Math.Max(0, Math.Round(progress, 1));
    }

    public static List<TaskDto> ToDtoList(this IEnumerable<OperationalTask> entities)
    {
        return entities?.Select(e => e.ToDto()).ToList() ?? new List<TaskDto>();
    }

    // --- SEWING TEAM MAPPING ---
    public static SewingTeamDto ToDto(this SewingTeam entity)
    {
        if (entity == null) return null!;
        return new SewingTeamDto
        {
            Id = entity.Id,
            Name = entity.Name,
            IsActive = entity.IsActive,
            MemberCount = entity.Members?.Count ?? 0,
            Members = entity.Members?.ToDtoList() ?? new List<UserDto>(),
            SelectedUserIds = entity.Members?.Select(m => m.Id).ToList() ?? new List<int>()
        };
    }

    public static List<SewingTeamDto> ToDtoList(this IEnumerable<SewingTeam> entities)
    {
        return entities?.Select(e => e.ToDto()).ToList() ?? new List<SewingTeamDto>();
    }

    // --- SYSTEM CONFIGURATION MAPPING ---
    public static SystemConfigurationDto ToDto(this SystemConfiguration entity)
    {
        if (entity == null) return null!;
        return new SystemConfigurationDto
        {
            CompanyName = entity.CompanyName,
            CompanyTaxId = entity.CompanyTaxId,
            LogoBase64 = entity.LogoBase64,
            DailyFixedCost = entity.DailyFixedCost,
            OperationalHourlyCost = entity.OperationalHourlyCost,
            ThemeName = entity.ThemeName,
            TvAnnouncement = entity.Value // Map Value to TvAnnouncement if used for that
        };
    }
}
