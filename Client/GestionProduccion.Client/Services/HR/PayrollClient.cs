using GestionProduccion.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestionProduccion.Client.Services.HR;

public interface IPayrollClient
{
    Task<ApiResponse<PayrollSlipDto>> GetMyCurrentSlipAsync(int year, int month);
    Task<ApiResponse<PayrollSlipDto>> GetEmployeeSlipAsync(int userId, int year, int month);
}

public class PayrollClient : IPayrollClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _options;

    public PayrollClient(HttpClient http, JsonSerializerOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<ApiResponse<PayrollSlipDto>> GetMyCurrentSlipAsync(int year, int month)
    {
        return await _http.GetFromJsonAsync<ApiResponse<PayrollSlipDto>>($"api/Payroll/my-slip?year={year}&month={month}", _options)
            ?? ApiResponse<PayrollSlipDto>.FailureResult("Erro ao consultar folha de pagamento.");
    }

    public async Task<ApiResponse<PayrollSlipDto>> GetEmployeeSlipAsync(int userId, int year, int month)
    {
        return await _http.GetFromJsonAsync<ApiResponse<PayrollSlipDto>>($"api/Payroll/employee/{userId}/slip?year={year}&month={month}", _options)
            ?? ApiResponse<PayrollSlipDto>.FailureResult("Erro ao consultar folha de colaborador.");
    }
}