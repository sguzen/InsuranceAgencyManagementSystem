namespace IAMS.Application.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult() { }

        public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static PagedResult<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
        {
            return new PagedResult<T>(new List<T>(), 0, pageNumber, pageSize);
        }
    }

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