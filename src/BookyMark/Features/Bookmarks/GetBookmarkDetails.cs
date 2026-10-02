using BookyMark.Data;
using BookyMark.Features.Shared;
using Microsoft.EntityFrameworkCore;

namespace BookyMark.Features.Bookmarks;

public sealed record GetBookmarkDetailsResponse(
    int Id,
    string Title,
    string Url,
    string? Notes,
    bool Favorite,
    string? ImageUrl,
    string? FaviconUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int? CollectionId,
    string? CollectionName
);

public sealed class GetBookmarkDetails(
    IDbContextFactory<AppDbContext> dbCtxFactory,
    ILogger<GetBookmarkDetails> logger
)
{
    public async Task<Result<GetBookmarkDetailsResponse>> GetAsync(
        int id,
        CancellationToken ct = default
    )
    {
        await using var db = await dbCtxFactory.CreateDbContextAsync(ct);

        var bookmark = await db
            .Bookmarks.AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new GetBookmarkDetailsResponse(
                b.Id,
                b.Title,
                b.Url,
                b.Notes,
                b.Favorite,
                b.ImageUrl,
                b.FaviconUrl,
                b.CreatedAt,
                b.UpdatedAt,
                b.CollectionId,
                b.Collection != null ? b.Collection.Name : null
            ))
            .FirstOrDefaultAsync(ct);

        if (bookmark is null)
        {
            logger.LogWarning("Bookmark is null, ID: {BookmarkId}", id);
            return Result<GetBookmarkDetailsResponse>.Failure("Bookmark was not found or empty.");
        }

        logger.LogInformation("Getting bookmark, ID: {BookmarkId}", id);

        return Result<GetBookmarkDetailsResponse>.Success(bookmark);
    }
}
