using GestionProduccion.Application.Mappers;
using GestionProduccion.Domain.Entities.CRM;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GestionProduccion.Services;

public class LeadService : ILeadService
{
    private readonly ILeadRepository _leadRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly MainMapper _mapper;
    private readonly ILogger<LeadService> _logger;

    public LeadService(
        ILeadRepository leadRepository,
        IEmailService emailService,
        INotificationService notificationService,
        IUserRepository userRepository,
        MainMapper mapper,
        ILogger<LeadService> logger)
    {
        _leadRepository = leadRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LeadDto> CreateLeadAsync(CreateLeadDto dto, CancellationToken ct = default)
    {
        var lead = new Lead
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Message = dto.Message,
            Status = LeadStatus.New,
            Source = LeadSource.Website,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _leadRepository.AddAsync(lead);
        await _leadRepository.SaveChangesAsync();

        // 1. Send confirmation email to client (PT-BR)
        try
        {
            var subject = "Recebemos sua solicitação - Serona Gestão";
            var body = $@"
                <h2>Olá {lead.Name},</h2>
                <p>Obrigado pelo seu interesse em nossos serviços de produção.</p>
                <p>Recebemos sua mensagem e entraremos em contato em breve.</p>
                <br/>
                <p>Atenciosamente,<br/>Equipe Serona</p>";
            
            await _emailService.SendEmailAsync(lead.Email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending confirmation email to lead {Email}", lead.Email);
        }

        // 2. Notify Admin (Igor) in real-time
        try
        {
            await _notificationService.NotifyNewLeadAsync(lead.Name, lead.Email, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying admins about new lead {Id}", lead.Id);
        }

        return _mapper.ToDto(lead);
        }

    public async Task<List<LeadDto>> GetLeadsAsync(CancellationToken ct = default)
    {
        var leads = await _leadRepository.GetAllAsync();
        return _mapper.ToDtoList(leads.OrderByDescending(l => l.CreatedAt));
    }

    public async Task<LeadDto> UpdateLeadStatusAsync(int leadId, LeadStatus newStatus, string? note = null, CancellationToken ct = default)
    {
        var lead = await _leadRepository.GetByIdAsync(leadId);
        if (lead == null) throw new KeyNotFoundException("Lead not found.");

        lead.Status = newStatus;
        lead.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(note))
        {
            lead.CommercialNotes = string.IsNullOrEmpty(lead.CommercialNotes) 
                ? $"[{DateTime.UtcNow:dd/MM/yyyy}] {note}" 
                : $"{lead.CommercialNotes}\n[{DateTime.UtcNow:dd/MM/yyyy}] {note}";
        }

        await _leadRepository.UpdateAsync(lead);
        await _leadRepository.SaveChangesAsync();

        return _mapper.ToDto(lead);
    }
}