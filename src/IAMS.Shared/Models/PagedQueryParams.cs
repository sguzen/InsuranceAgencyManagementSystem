namespace IAMS.Shared.Models
{
    public class PagedQueryParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
        public string? SearchTerm { get; set; }

        public PagedQueryParams()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;
            if (PageSize > 100) PageSize = 100;
        }
    }
}
