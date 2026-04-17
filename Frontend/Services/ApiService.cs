using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Models;

namespace Frontend.Services;

/// <summary>
/// Service layer that calls ONLY the API Gateway
/// </summary>
public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        try
        {
            var products = await _http.GetFromJsonAsync<List<ProductDto>>("/api/products");
            return products ?? new List<ProductDto>();
        }
        catch (HttpRequestException)
        {
            return new List<ProductDto>();
        }
        catch (NotSupportedException)
        {
            return new List<ProductDto>();
        }
        catch (JsonException)
        {
            return new List<ProductDto>();
        }
    }
}
