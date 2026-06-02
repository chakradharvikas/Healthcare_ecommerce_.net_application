using ECommerce.Application.DTOs;

namespace ECommerce.Web.Services;

public class AuthStateService
{
    public string? Token { get; private set; }
    public string? Email { get; private set; }
    public string? UserId { get; private set; }
    public string? FullName { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public event Action? OnChange;

    public void SetAuth(AuthResponseDto response)
    {
        Token = response.Token;
        Email = response.Email;
        UserId = response.UserId;
        FullName = response.FullName;
        NotifyStateChanged();
    }

    public void Logout()
    {
        Token = null;
        Email = null;
        UserId = null;
        FullName = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
