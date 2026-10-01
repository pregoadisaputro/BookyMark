using BookyMark.Data;
using BookyMark.Data.Entity;
using BookyMark.Features.LinkMetadata;
using BookyMark.Features.Shared;
using Microsoft.EntityFrameworkCore;

namespace BookyMark.Features.Bookmarks;

public sealed record CreateBookmarkRequest(string Title, string Url);

public sealed record CreateBookmarkResponse(int Id);

public sealed class CreateBookmark(
    IDbContextFactory<AppDbContext> dbCtxFactory,
    LinkMetadataService metadataService,
    ILogger<CreateBookmark> logger
)
{
    public async Task<Result<CreateBookmarkResponse>> HandleAsync(
        CreateBookmarkRequest request,
        CancellationToken ct = default
    )
    {
        var metadata = await metadataService.GetAsync(request.Url, ct);

        if (metadata is null)
        {
            logger.LogWarning("Metadata response is null {Metadata}", metadata);
            return Result<CreateBookmarkResponse>.Failure("Metadata response is empty or null");
        }

        await using var db = await dbCtxFactory.CreateDbContextAsync(ct);

        var newBookmark = new Bookmark
        {
            Title = metadata.Title ?? request.Title,
            Url = request.Url,
            ImageUrl = metadata.Image?.Url,
            FaviconUrl = metadata.Favicon?.Url,
        };

        db.Bookmarks.Add(newBookmark);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created bookmark, Title: {BookmarkTitle} & ID: {BookmarkId}",
            newBookmark.Title,
            newBookmark.Id
        );

        return Result<CreateBookmarkResponse>.Success(new CreateBookmarkResponse(newBookmark.Id));
    }
}
