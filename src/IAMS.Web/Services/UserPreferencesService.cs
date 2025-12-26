using Microsoft.JSInterop;

namespace IAMS.Web.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private const string ListViewModeKey = "iams_list_view_mode";
    private readonly IJSRuntime _jsRuntime;

    public UserPreferencesService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<ListViewMode> GetListViewModeAsync()
    {
        try
        {
            var stored = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", ListViewModeKey);

            if (string.IsNullOrEmpty(stored))
            {
                // Default to Simple view
                return ListViewMode.Simple;
            }

            return Enum.TryParse<ListViewMode>(stored, out var mode)
                ? mode
                : ListViewMode.Simple;
        }
        catch
        {
            return ListViewMode.Simple;
        }
    }

    public async Task SetListViewModeAsync(ListViewMode mode)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ListViewModeKey, mode.ToString());
        }
        catch
        {
            // Silently fail if localStorage is not available
        }
    }
}
