namespace IAMS.Web.Services;

public interface IUserPreferencesService
{
    Task<ListViewMode> GetListViewModeAsync();
    Task SetListViewModeAsync(ListViewMode mode);
}

public enum ListViewMode
{
    Simple,
    Detailed
}
