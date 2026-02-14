using System;

namespace GestionProduccion.Client.Services;

public class UserStateService
{
    public event Action? OnChange;

    public string? AvatarUrl { get; private set; }
    public string? UserName { get; private set; }

    public void SetUser(string name, string? avatarUrl)
    {
        UserName = name;
        AvatarUrl = avatarUrl;
        NotifyStateChanged();
    }

    public void UpdateAvatar(string newUrl)
    {
        AvatarUrl = newUrl;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
