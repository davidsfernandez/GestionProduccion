using GestionProduccion.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestionProduccion.Client.Services.HR;

public interface IHRClient
{
    // Leave Management
    Task<ApiResponse<EmployeeLeaveDto>> RequestLeaveAsync(CreateLeaveDto dto);
    Task<ApiResponse<EmployeeLeaveDto>> ProcessLeaveAsync(int id, bool approved, string? note);
    Task<ApiResponse<VacationBalanceDto>> GetVacationBalanceAsync(int userId);

    // Employee Profile Management
    Task<ApiResponse<List<EmployeeProfileDto>>> ListEmployeesAsync();
    Task<ApiResponse<EmployeeProfileDto>> GetProfileAsync(int userId);
    Task<ApiResponse<bool>> UpdateProfileAsync(EmployeeProfileDto dto);
}

public class HRClient : IHRClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _options;

    public HRClient(HttpClient http, JsonSerializerOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<ApiResponse<EmployeeLeaveDto>> RequestLeaveAsync(CreateLeaveDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/HR/leave-request", dto, _options);
        return await response.Content.ReadFromJsonAsync<ApiResponse<EmployeeLeaveDto>>(_options)
            ?? ApiResponse<EmployeeLeaveDto>.FailureResult("Erro ao solicitar ausência.");
    }

    public async Task<ApiResponse<EmployeeLeaveDto>> ProcessLeaveAsync(int id, bool approved, string? note)
    {
        var response = await _http.PostAsync($"api/HR/leave/{id}/process?approved={approved}&note={Uri.EscapeDataString(note ?? "")}", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse<EmployeeLeaveDto>>(_options)
            ?? ApiResponse<EmployeeLeaveDto>.FailureResult("Erro ao processar ausência.");
    }

    public async Task<ApiResponse<VacationBalanceDto>> GetVacationBalanceAsync(int userId)
    {
        return await _http.GetFromJsonAsync<ApiResponse<VacationBalanceDto>>($"api/HR/vacation-balance/{userId}", _options)
            ?? ApiResponse<VacationBalanceDto>.FailureResult("Erro ao obter saldo.");
    }

    public async Task<ApiResponse<List<EmployeeProfileDto>>> ListEmployeesAsync()
    {
        return await _http.GetFromJsonAsync<ApiResponse<List<EmployeeProfileDto>>>("api/HR/employees", _options)
            ?? ApiResponse<List<EmployeeProfileDto>>.FailureResult("Erro ao listar colaboradores.");
    }

    public async Task<ApiResponse<EmployeeProfileDto>> GetProfileAsync(int userId)
    {
        return await _http.GetFromJsonAsync<ApiResponse<EmployeeProfileDto>>($"api/HR/profile/{userId}", _options)
            ?? ApiResponse<EmployeeProfileDto>.FailureResult("Erro ao obter perfil.");
    }

    public async Task<ApiResponse<bool>> UpdateProfileAsync(EmployeeProfileDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/HR/profile/{dto.UserId}", dto, _options);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(_options)
            ?? ApiResponse<bool>.FailureResult("Erro ao atualizar perfil.");
    }
}