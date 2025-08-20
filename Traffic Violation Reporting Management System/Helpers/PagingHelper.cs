using System;
using System.Collections.Generic;
using System.Linq;
namespace Traffic_Violation_Reporting_Management_System.Helpers
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }

    public static class PagingHelper
    {
        public static PagedResult<T> GetPaged<T>(this IQueryable<T> query, int page, int pageSize)
        {
            var result = new PagedResult<T>
            {
                TotalRecords = query.Count(),
                Page = page,
                PageSize = pageSize,
                Items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            };
            return result;
        }
    }
}
