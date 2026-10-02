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

        var bookmark = await db.Bookmarks.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);

        if (bookmark is null)
        {
            logger.LogWarning("Bookmark is null, ID: {BookmarkId}", id);
            return Result<GetBookmarkDetailsResponse>.Failure("Bookmark was not found or empty.");
        }

        logger.LogInformation("Getting bookmark, ID: {BookmarkId}", id);

        return Result<GetBookmarkDetailsResponse>.Success(
            new GetBookmarkDetailsResponse(
                bookmark.Id,
                bookmark.Title,
                bookmark.Url,
                bookmark.Notes,
                bookmark.Favorite,
                bookmark.ImageUrl,
                bookmark.FaviconUrl,
                bookmark.CreatedAt,
                bookmark.UpdatedAt,
                bookmark.CollectionId,
                bookmark.Collection != null ? bookmark.Collection.Name : null
            )
        );
    }
}
