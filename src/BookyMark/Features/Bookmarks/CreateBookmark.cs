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
        var url = request.Url?.Trim() ?? string.Empty;

        await using var db = await dbCtxFactory.CreateDbContextAsync(ct);

        var existingBookmarkUrl = await db.Bookmarks.AnyAsync(b => b.Url == url, ct);

        if (existingBookmarkUrl)
        {
            logger.LogWarning("Bookmark URL already exists, URL: {BookmarkUrl}", url);
            return Result<CreateBookmarkResponse>.Failure("Bookmark URL already exists.");
        }

        var metadataResult = await metadataService.GetAsync(url, ct);

        if (!metadataResult.IsSuccess)
        {
            return Result<CreateBookmarkResponse>.Failure(metadataResult.Error);
        }

        var metadata = metadataResult.Value;

        var title = !string.IsNullOrWhiteSpace(metadata?.Title) ? metadata.Title : request.Title;

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result<CreateBookmarkResponse>.Failure(
                "Title is required when metadata is unavailable."
            );
        }

        var newBookmark = new Bookmark
        {
            Title = title.Trim(),
            Url = url,
            ImageUrl = metadata?.ImageUrl?.Url,
            FaviconUrl = metadata?.FaviconUrl?.Url,
        };

        db.Bookmarks.Add(newBookmark);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created bookmark, Title: {BookmarkTitle}, URL: {BookmarkUrl} & ID: {BookmarkId}",
            newBookmark.Title,
            newBookmark.Url,
            newBookmark.Id
        );

        return Result<CreateBookmarkResponse>.Success(new CreateBookmarkResponse(newBookmark.Id));
    }
}
