using System;

namespace GestionProduccion.Client.Services;

public class UserStateService
{
    public event Action? OnChange;

    private string? _avatarUrl;
    private string? _userName;

    public string? AvatarUrl
    {
        get => _avatarUrl;
        private set
        {
            _avatarUrl = value;
            NotifyStateChanged();
        }
    }

    public string? UserName
    {
        get => _userName;
        private set
        {
            _userName = value;
            NotifyStateChanged();
        }
    }

    public void SetUser(string name, string? avatarUrl)
    {
        _userName = name;
        _avatarUrl = avatarUrl;
        NotifyStateChanged();
    }

    public void UpdateAvatar(string? newUrl)
    {
        AvatarUrl = newUrl;
    }

    public void ClearState()
    {
        _userName = null;
        _avatarUrl = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
