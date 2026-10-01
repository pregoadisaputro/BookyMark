using BookyMark.Data;
using BookyMark.Features.Shared;
using Microsoft.EntityFrameworkCore;

namespace BookyMark.Features.Bookmarks;

public enum BookmarkSortBy
{
    RecentlyAdded = 0,
    RecentlyUpdated,
}

public sealed record GetBookmarksRequest(
    int PageNumber = 1,
    int PageSize = 24,
    string? Title = null,
    bool? Favorite = null,
    BookmarkSortBy SortBy = BookmarkSortBy.RecentlyAdded
);

public sealed record GetBookmarksResponse(
    int Id,
    string Title,
    bool? Favorite,
    string? Image,
    string? Favicon,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public sealed record GetBookmarksData(
    int PageNumber,
    int PageSize,
    int TotalPages,
    int TotalItems,
    IReadOnlyList<GetBookmarksResponse> BookmarksResponses
);

public sealed class GetBookmarks(IDbContextFactory<AppDbContext> dbCtxFactory)
{
    public async Task<Result<GetBookmarksData>> GetAsync(
        GetBookmarksRequest request,
        CancellationToken ct = default
    )
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize switch
        {
            < 1 => 24,
            >= 100 => 100,
            _ => request.PageSize,
        };

        await using var db = await dbCtxFactory.CreateDbContextAsync(ct);

        var query = db.Bookmarks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            query = query.Where(b => EF.Functions.Like(b.Title, $"%{request.Title}%"));
        }

        if (request.Favorite.HasValue)
        {
            query = query.Where(b => b.Favorite == request.Favorite);
        }

        query = request.SortBy switch
        {
            BookmarkSortBy.RecentlyUpdated => query
                .OrderByDescending(b => b.UpdatedAt)
                .ThenBy(b => b.Id),
            _ => query.OrderByDescending(b => b.CreatedAt).ThenBy(b => b.Id),
        };

        var totalItems = await query.CountAsync(ct);
        var skip = (pageNumber - 1) * pageSize;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var response = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(b => new GetBookmarksResponse(
                b.Id,
                b.Title,
                b.Favorite,
                b.ImageUrl,
                b.FaviconUrl,
                b.CreatedAt,
                b.UpdatedAt
            ))
            .ToListAsync(ct);

        return Result<GetBookmarksData>.Success(
            new GetBookmarksData(pageNumber, pageSize, totalPages, totalItems, response)
        );
    }
}
