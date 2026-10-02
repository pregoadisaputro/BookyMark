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

        var bookmark = await db.Bookmarks.FindAsync([id], ct);

        if (bookmark is null)
        {
            logger.LogWarning("Bookmark is null, ID: {BookmarkId}", id);
            return Result.Failure("Bookmark was not found or empty.");
        }

        var cleanedUrl = request.Url?.Trim();

        if (!string.IsNullOrWhiteSpace(cleanedUrl) && bookmark.Url != cleanedUrl)
        {
            var existingUrl = await db.Bookmarks.AnyAsync(
                b => b.Url == cleanedUrl && b.Id != id,
                ct
            );

            if (existingUrl)
            {
                logger.LogWarning("Bookmark URL already exists, URL: {BookmarkUrl}", cleanedUrl);
                return Result.Failure("Bookmark URL already exists.");
            }

            var metadataResult = await metadataService.GetAsync(cleanedUrl, ct);

            if (!metadataResult.IsSuccess)
            {
                return Result.Failure(metadataResult.Error);
            }

            var metadata = metadataResult.Value;

            var title = !string.IsNullOrWhiteSpace(metadata?.Title)
                ? metadata.Title
                : bookmark.Title;

            bookmark.Title = title;
            bookmark.Url = cleanedUrl;
            bookmark.ImageUrl = metadata?.Image?.Url;
            bookmark.FaviconUrl = metadata?.Favicon?.Url;
        }

        if (request.CollectionId is not null)
        {
            var collectionExists = await db.Collections.AnyAsync(
                c => c.Id == request.CollectionId,
                ct
            );

            if (!collectionExists)
            {
                logger.LogWarning("Collection is null, ID: {CollectionId}", request.CollectionId);
                return Result.Failure("Collection was not found or empty.");
            }
        }

        bookmark.Notes = request.Notes;
        bookmark.Favorite = request.Favorite;
        bookmark.CollectionId = request.CollectionId;
        bookmark.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
