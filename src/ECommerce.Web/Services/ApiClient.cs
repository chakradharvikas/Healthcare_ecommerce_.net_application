using System.Net.Http.Headers;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;

namespace ECommerce.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthStateService _auth;

    public ApiClient(HttpClient http, AuthStateService auth)
    {
        _http = http;
        _auth = auth;
    }

    private void ApplyAuth()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrEmpty(_auth.Token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Token);
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync() =>
        await _http.GetFromJsonAsync<IReadOnlyList<ProductDto>>("api/products") ?? [];

    public async Task<ProductDto?> GetProductAsync(Guid id) =>
        await _http.GetFromJsonAsync<ProductDto>($"api/products/{id}");

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto) =>
        await PostAsync<AuthResponseDto>("api/auth/register", dto)
        ?? throw new InvalidOperationException("Registration failed.");

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto) =>
        await PostAsync<AuthResponseDto>("api/auth/login", dto)
        ?? throw new InvalidOperationException("Login failed.");

    public async Task<IReadOnlyList<CartItemDto>> GetCartAsync()
    {
        ApplyAuth();
        return await _http.GetFromJsonAsync<IReadOnlyList<CartItemDto>>("api/cart") ?? [];
    }

    public async Task AddToCartAsync(AddToCartDto dto)
    {
        ApplyAuth();
        var response = await _http.PostAsJsonAsync("api/cart", dto);
        await EnsureSuccessAsync(response);
    }

    public async Task UpdateCartItemAsync(Guid productId, UpdateCartItemDto dto)
    {
        ApplyAuth();
        var response = await _http.PutAsJsonAsync($"api/cart/{productId}", dto);
        await EnsureSuccessAsync(response);
    }

    public async Task RemoveFromCartAsync(Guid productId)
    {
        ApplyAuth();
        var response = await _http.DeleteAsync($"api/cart/{productId}");
        await EnsureSuccessAsync(response);
    }

    public async Task<OrderDto> CheckoutAsync(CreateOrderDto dto)
    {
        ApplyAuth();
        var response = await _http.PostAsJsonAsync("api/orders/checkout", dto);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<OrderDto>()
            ?? throw new InvalidOperationException("Checkout failed.");
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync()
    {
        ApplyAuth();
        return await _http.GetFromJsonAsync<IReadOnlyList<OrderDto>>("api/orders") ?? [];
    }

    private async Task<T?> PostAsync<T>(string url, object body)
    {
        var response = await _http.PostAsJsonAsync(url, body);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var error = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error);
    }
}
