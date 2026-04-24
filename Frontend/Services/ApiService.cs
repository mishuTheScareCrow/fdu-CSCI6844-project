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

    public async Task<ProductDto?> CreateProductAsync(CreateProductDto request)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync("/api/products", request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<List<CustomerDto>> GetCustomersAsync()
    {
        try
        {
            var customers = await _http.GetFromJsonAsync<List<CustomerDto>>("/api/customers");
            return customers ?? new List<CustomerDto>();
        }
        catch (HttpRequestException)
        {
            return new List<CustomerDto>();
        }
        catch (NotSupportedException)
        {
            return new List<CustomerDto>();
        }
        catch (JsonException)
        {
            return new List<CustomerDto>();
        }
    }

    public async Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto request)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync("/api/customers", request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CustomerDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<List<OrderDto>> GetOrdersAsync()
    {
        try
        {
            var orders = await _http.GetFromJsonAsync<List<OrderDto>>("/api/orders");
            return orders ?? new List<OrderDto>();
        }
        catch (HttpRequestException)
        {
            return new List<OrderDto>();
        }
        catch (NotSupportedException)
        {
            return new List<OrderDto>();
        }
        catch (JsonException)
        {
            return new List<OrderDto>();
        }
    }

    public async Task<OrderDto?> CreateOrderAsync(CreateOrderDto request)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync("/api/orders", request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<OrderDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
