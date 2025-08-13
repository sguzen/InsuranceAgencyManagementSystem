using Microsoft.EntityFrameworkCore;

namespace IAMS.Application.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        // Computed properties
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int FirstItemIndex => TotalCount > 0 ? (PageNumber - 1) * PageSize + 1 : 0;
        public int LastItemIndex => Math.Min(FirstItemIndex + PageSize - 1, TotalCount);
        public bool IsFirstPage => PageNumber == 1;
        public bool IsLastPage => PageNumber == TotalPages;
        public bool HasItems => Items?.Any() == true;
        public int ItemsCount => Items?.Count ?? 0;

        public PagedResult() { }

        public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items ?? new List<T>();
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

        #region Result-style Properties and Methods
        public bool IsSuccess { get; private set; } = true;
        public bool IsFailure => !IsSuccess;
        public string Message { get; private set; } = string.Empty;
        public List<string> Errors { get; private set; } = new();
        public int StatusCode { get; private set; } = 200;

        private PagedResult(bool isSuccess, string message, List<T> items, int totalCount, int pageNumber, int pageSize, List<string>? errors = null, int statusCode = 200)
            : this(items, totalCount, pageNumber, pageSize)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? new List<string>();
            StatusCode = statusCode;
        }

        public static PagedResult<T> Success(List<T> items, int totalCount, int pageNumber, int pageSize, string message = "Data retrieved successfully")
        {
            return new PagedResult<T>(true, message, items, totalCount, pageNumber, pageSize, null, 200);
        }

        public static PagedResult<T> Failure(string message, List<string>? errors = null, int pageNumber = 1, int pageSize = 10)
        {
            return new PagedResult<T>(false, message, new List<T>(), 0, pageNumber, pageSize, errors, 400);
        }

        public static PagedResult<T> NotFound(string message = "No data found", int pageNumber = 1, int pageSize = 10)
        {
            return new PagedResult<T>(false, message, new List<T>(), 0, pageNumber, pageSize, null, 404);
        }

        public static PagedResult<T> Unauthorized(string message = "Unauthorized access to data", int pageNumber = 1, int pageSize = 10)
        {
            return new PagedResult<T>(false, message, new List<T>(), 0, pageNumber, pageSize, null, 401);
        }

        public static PagedResult<T> Forbidden(string message = "Access to data forbidden", int pageNumber = 1, int pageSize = 10)
        {
            return new PagedResult<T>(false, message, new List<T>(), 0, pageNumber, pageSize, null, 403);
        }

        public static PagedResult<T> ValidationFailure(string message, List<string> validationErrors, int pageNumber = 1, int pageSize = 10)
        {
            return new PagedResult<T>(false, message, new List<T>(), 0, pageNumber, pageSize, validationErrors, 422);
        }

        public static PagedResult<T> InternalError(string message = "Error retrieving data", List<string>? errors = null, int pageNumber = 1, int pageSize = 10)
        {
            return new PagedResult<T>(false, message, new List<T>(), 0, pageNumber, pageSize, errors, 500);
        }

        public static PagedResult<T> BadRequest(string message = "Invalid request parameters", List<string>? errors = null, int pageNumber = 1, int pageSize = 10)
        {
            return new PagedResult<T>(false, message, new List<T>(), 0, pageNumber, pageSize, errors, 400);
        }

        // Implicit conversion to bool
        public static implicit operator bool(PagedResult<T> result) => result.IsSuccess;
        #endregion

        /// <summary>
        /// Create PagedResult from IQueryable with async execution
        /// </summary>
        public static async Task<PagedResult<T>> CreateAsync(
            IQueryable<T> query,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// Map PagedResult to different type
        /// </summary>
        public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            return new PagedResult<TResult>(
                Items.Select(mapper).ToList(),
                TotalCount,
                PageNumber,
                PageSize);
        }

        /// <summary>
        /// Map PagedResult to different type with async mapper
        /// </summary>
        public async Task<PagedResult<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper)
        {
            var mappedItems = new List<TResult>();
            foreach (var item in Items)
            {
                mappedItems.Add(await mapper(item));
            }

            return new PagedResult<TResult>(
                mappedItems,
                TotalCount,
                PageNumber,
                PageSize);
        }

        /// <summary>
        /// Get page numbers for pagination UI
        /// </summary>
        public List<int> GetPageNumbers(int maxPages = 10)
        {
            var pageNumbers = new List<int>();

            if (TotalPages <= maxPages)
            {
                for (int i = 1; i <= TotalPages; i++)
                {
                    pageNumbers.Add(i);
                }
            }
            else
            {
                var halfMax = maxPages / 2;
                var start = Math.Max(1, PageNumber - halfMax);
                var end = Math.Min(TotalPages, start + maxPages - 1);

                if (end - start + 1 < maxPages)
                {
                    start = Math.Max(1, end - maxPages + 1);
                }

                for (int i = start; i <= end; i++)
                {
                    pageNumbers.Add(i);
                }
            }

            return pageNumbers;
        }

        /// <summary>
        /// Convert to API response format
        /// </summary>
        public PagedResponse<T> ToResponse()
        {
            return new PagedResponse<T>
            {
                Data = Items,
                Meta = new PaginationMeta
                {
                    CurrentPage = PageNumber,
                    PageSize = PageSize,
                    TotalCount = TotalCount,
                    TotalPages = TotalPages,
                    HasNext = HasNextPage,
                    HasPrevious = HasPreviousPage,
                    FirstItemIndex = FirstItemIndex,
                    LastItemIndex = LastItemIndex
                }
            };
        }

        /// <summary>
        /// Filter items in current page
        /// </summary>
        public PagedResult<T> Filter(Func<T, bool> predicate)
        {
            var filteredItems = Items.Where(predicate).ToList();
            return new PagedResult<T>(filteredItems, filteredItems.Count, 1, filteredItems.Count);
        }
    }

    // Extension methods for IQueryable
    public static class QueryablePagedExtensions
    {
        /// <summary>
        /// Convert IQueryable to PagedResult
        /// </summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return await PagedResult<T>.CreateAsync(query, pageNumber, pageSize, cancellationToken);
        }

        /// <summary>
        /// Convert IQueryable to PagedResult with mapping
        /// </summary>
        public static async Task<PagedResult<TResult>> ToPagedResultAsync<T, TResult>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            Func<T, TResult> mapper,
            CancellationToken cancellationToken = default)
        {
            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
            return pagedResult.Map(mapper);
        }

        /// <summary>
        /// Apply pagination without executing query
        /// </summary>
        public static IQueryable<T> ApplyPaging<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            return query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }
    }

    // Extension methods for List
    public static class ListPagedExtensions
    {
        /// <summary>
        /// Convert List to PagedResult
        /// </summary>
        public static PagedResult<T> ToPagedResult<T>(
            this List<T> list,
            int pageNumber,
            int pageSize)
        {
            var totalCount = list.Count;
            var items = list
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return PagedResult<T>.Create(items, totalCount, pageNumber, pageSize);
        }
    }

    // For API responses - cleaner JSON structure
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationMeta Meta { get; set; } = new();

        public static PagedResponse<T> FromPagedResult(PagedResult<T> pagedResult)
        {
            return pagedResult.ToResponse();
        }
    }

    public class PaginationMeta
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
        public int FirstItemIndex { get; set; }
        public int LastItemIndex { get; set; }
    }
}