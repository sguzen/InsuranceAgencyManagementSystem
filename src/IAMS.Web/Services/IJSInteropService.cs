using Microsoft.JSInterop;

namespace IAMS.Web.Services;

public interface IJSInteropService
{
    Task<bool> ValidateTurkishNationalIdAsync(string nationalId);
    Task<string> FormatCurrencyAsync(decimal amount);
    Task<string> FormatDateAsync(DateTime date);
    Task DownloadFileAsync(string filename, string content, string mimeType = "text/plain");
    Task PrintElementAsync(string elementId);
    Task ShowBrowserNotificationAsync(string title, string message, string? icon = null);
    Task SetLocalStorageAsync(string key, string value);
    Task<string?> GetLocalStorageAsync(string key);
    Task RemoveLocalStorageAsync(string key);
}

public class JSInteropService : IJSInteropService
{
    private readonly IJSRuntime _jsRuntime;

    public JSInteropService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> ValidateTurkishNationalIdAsync(string nationalId)
    {
        return await _jsRuntime.InvokeAsync<bool>("BlazorInterop.validateTurkishNationalId", nationalId);
    }

    public async Task<string> FormatCurrencyAsync(decimal amount)
    {
        return await _jsRuntime.InvokeAsync<string>("BlazorInterop.formatTurkishCurrency", amount);
    }

    public async Task<string> FormatDateAsync(DateTime date)
    {
        return await _jsRuntime.InvokeAsync<string>("BlazorInterop.formatTurkishDate", date);
    }

    public async Task DownloadFileAsync(string filename, string content, string mimeType = "text/plain")
    {
        await _jsRuntime.InvokeVoidAsync("BlazorInterop.downloadFile", filename, content, mimeType);
    }

    public async Task PrintElementAsync(string elementId)
    {
        await _jsRuntime.InvokeVoidAsync("BlazorInterop.printElement", elementId);
    }

    public async Task ShowBrowserNotificationAsync(string title, string message, string? icon = null)
    {
        await _jsRuntime.InvokeVoidAsync("BlazorInterop.showBrowserNotification", title, message, icon);
    }

    public async Task SetLocalStorageAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("BlazorInterop.setLocalStorage", key, value);
    }

    public async Task<string?> GetLocalStorageAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>("BlazorInterop.getLocalStorage", key);
    }

    public async Task RemoveLocalStorageAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("BlazorInterop.removeLocalStorage", key);
    }
}