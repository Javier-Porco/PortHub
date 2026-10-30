using System.Net.Http.Headers;
using System.Text.Json;
using PortHub.API.Domain.DTOs;

namespace PortHub.API.Services;

public class AeroLinkClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AeroLinkClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> ValidarTicketAsync(string codigoTicket)
    {
        try
        {
            var baseUrl = _configuration.GetValue<string>("AeroLinkApi:BaseUrl") ?? "http://localhost:5000";
            var apiKey = _configuration.GetValue<string>("AeroLinkApi:ApiKey") ?? "AeroLinkSecretKey2026";

            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/tickets/{codigoTicket}/validacion");
            request.Headers.Add("Authorization", $"ApiKey {apiKey}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ValidacionTicketAeroLinkDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.EsValido ?? false;
        }
        catch
        {
            // Si AeroLink está caído o no responde, por seguridad el pasajero no aborda
            return false;
        }
    }
}