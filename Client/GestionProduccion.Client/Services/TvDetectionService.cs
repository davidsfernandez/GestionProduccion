using Microsoft.JSInterop;

namespace GestionProduccion.Client.Services;

public class TvDetectionService
{
    private readonly IJSRuntime _js;

    public TvDetectionService(IJSRuntime js)
    {
        _js = js;
    }

    public virtual async Task<bool> IsTvDeviceAsync()
    {
        try
        {
            return await _js.InvokeAsync<bool>("tvDetection.isTvDevice");
        }
        catch
        {
            return false;
        }
    }

    public virtual async Task<ScreenResolution> GetScreenResolutionAsync()
    {
        try
        {
            return await _js.InvokeAsync<ScreenResolution>("tvDetection.getScreenResolution");
        }
        catch
        {
            return new ScreenResolution { Width = 0, Height = 0 };
        }
    }
}

public class ScreenResolution
{
    public int Width { get; set; }
    public int Height { get; set; }
}