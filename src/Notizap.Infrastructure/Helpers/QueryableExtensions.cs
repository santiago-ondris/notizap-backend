using Microsoft.EntityFrameworkCore;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int pageNumber, int pageSize)
    {
        var result = new PagedResult<T>
        {
            TotalItems = await query.CountAsync(),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        result.Items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return result;
    }
}