using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Security.Cryptography;
using System.Text.Json;

namespace InstitutoBlazor.Services;

/// <summary>Estado de sesión por circuito con notificación y persistencia protegida en la pestaña.</summary>
public class SessionService(ProtectedSessionStorage storage, ILogger<SessionService> logger)
{
    private const string StorageKey = "instituto.session.v2";
    private Task? _initTask;

    public bool IsReady { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public string? UserName { get; private set; }

    public event Action? OnChange;

    public Task InitializeAsync() => _initTask ??= InitializeCoreAsync();

    public async Task LoginAsync(string userName)
    {
        UserName = userName;
        IsAuthenticated = true;
        IsReady = true;
        _initTask ??= Task.CompletedTask;
        try { await storage.SetAsync(StorageKey, new SesionGuardada(userName, DateTimeOffset.UtcNow.AddHours(8))); }
        catch (JSException ex) { logger.LogDebug(ex, "Sesión activa sin persistencia en el navegador"); }
        OnChange?.Invoke();
    }

    public async Task LogoutAsync()
    {
        UserName = null;
        IsAuthenticated = false;
        IsReady = true;
        try { await storage.DeleteAsync(StorageKey); }
        catch (JSException ex) { logger.LogDebug(ex, "No se pudo borrar el almacenamiento de la sesión"); }
        OnChange?.Invoke();
    }

    private async Task InitializeCoreAsync()
    {
        try
        {
            var result = await storage.GetAsync<SesionGuardada>(StorageKey);
            if (result.Success && result.Value is { } saved && saved.Expira > DateTimeOffset.UtcNow)
            {
                UserName = saved.Nombre;
                IsAuthenticated = true;
            }
        }
        catch (Exception ex) when (ex is JSException or CryptographicException or JsonException)
        {
            logger.LogDebug(ex, "No se pudo recuperar la sesión; se solicitará iniciar sesión");
        }

        IsReady = true;
        OnChange?.Invoke();
    }

    public record SesionGuardada(string Nombre, DateTimeOffset Expira);
}
