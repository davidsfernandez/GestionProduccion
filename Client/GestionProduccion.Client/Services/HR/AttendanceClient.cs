using GestionProduccion.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestionProduccion.Client.Services.HR;

public interface IAttendanceClient
{
    Task<ApiResponse<AttendanceStatusDto>> GetStatusAsync();
    Task<ApiResponse<AttendanceDto>> ClockInAsync(string? note = null);
    Task<ApiResponse<AttendanceDto>> ClockOutAsync(string? note = null);
}

public class AttendanceClient : IAttendanceClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _options;

    public AttendanceClient(HttpClient http, JsonSerializerOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<ApiResponse<AttendanceStatusDto>> GetStatusAsync()
    {
        return await _http.GetFromJsonAsync<ApiResponse<AttendanceStatusDto>>("api/Attendance/status", _options)
            ?? ApiResponse<AttendanceStatusDto>.FailureResult("Erro ao consultar status.");
    }

    public async Task<ApiResponse<AttendanceDto>> ClockInAsync(string? note = null)
    {
        var response = await _http.PostAsync($"api/Attendance/clock-in?note={Uri.EscapeDataString(note ?? "")}", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse<AttendanceDto>>(_options)
            ?? ApiResponse<AttendanceDto>.FailureResult("Erro ao registrar entrada.");
    }

    public async Task<ApiResponse<AttendanceDto>> ClockOutAsync(string? note = null)
    {
        var response = await _http.PostAsync($"api/Attendance/clock-out?note={Uri.EscapeDataString(note ?? "")}", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse<AttendanceDto>>(_options)
            ?? ApiResponse<AttendanceDto>.FailureResult("Erro ao registrar saída.");
    }
}