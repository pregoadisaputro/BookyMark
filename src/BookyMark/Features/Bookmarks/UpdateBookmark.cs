using BookyMark.Data;
using BookyMark.Features.LinkMetadata;
using BookyMark.Features.Shared;
using Microsoft.EntityFrameworkCore;

namespace BookyMark.Features.Bookmarks;

public sealed record UpdateBookmarkRequest(
    string? Url,
    string? Notes,
    bool Favorite,
    int? CollectionId
);

public sealed class UpdateBookmark(
    IDbContextFactory<AppDbContext> dbCtxFactory,
    LinkMetadataService metadataService,
    ILogger<UpdateBookmark> logger
)
{
    public async Task<Result> HandleAsync(
        int id,
        UpdateBookmarkRequest request,
        CancellationToken ct = default
    )
    {
        await using var db = await dbCtxFactory.CreateDbContextAsync(ct);

        var existingBookmark = await db.Bookmarks.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (existingBookmark is null)
        {
            logger.LogWarning("Bookmark is null, ID: {BookmarkId}", id);
            return Result.Failure("Bookmark is not found or empty.");
        }

        if (!string.IsNullOrWhiteSpace(request.Url))
        {
            var isChanged = existingBookmark.Url != request.Url;

            if (isChanged)
            {
                var metadata = await metadataService.GetAsync(request.Url, ct);

                existingBookmark.Title = metadata?.Title ?? existingBookmark.Title;
                existingBookmark.Url = request.Url;
                existingBookmark.ImageUrl = metadata?.Image?.Url;
                existingBookmark.FaviconUrl = metadata?.Favicon?.Url;
            }
        }

        existingBookmark.Notes = request.Notes;
        existingBookmark.Favorite = request.Favorite;
        existingBookmark.CollectionId = request.CollectionId;

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
