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

        var existingBookmark = await db.Bookmarks.FindAsync([id], ct);

        if (existingBookmark is null)
        {
            logger.LogWarning("Bookmark is null, ID: {BookmarkId}", id);
            return Result.Failure("Bookmark was not found or empty.");
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

        if (request.CollectionId is not null)
        {
            var collection = await db.Collections.FindAsync([request.CollectionId], ct);

            if (collection is null)
            {
                logger.LogWarning("Collection is null, ID: {CollectionId}", request.CollectionId);
                return Result<GetBookmarkDetailsResponse>.Failure(
                    "Collection was not found or empty."
                );
            }
        }

        existingBookmark.Notes = request.Notes;
        existingBookmark.Favorite = request.Favorite;
        existingBookmark.CollectionId = request.CollectionId;

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
