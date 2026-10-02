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
        var url = request.Url?.Trim();

        if (
            string.IsNullOrWhiteSpace(url)
            || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        )
        {
            return Result<CreateBookmarkResponse>.Failure(
                "URL must be a valid HTTP or HTTPS address."
            );
        }

        await using var db = await dbCtxFactory.CreateDbContextAsync(ct);

        var existingBookmarkUrl = await db.Bookmarks.AnyAsync(b => b.Url == url, ct);

        if (existingBookmarkUrl)
        {
            logger.LogWarning("Bookmark URL already exists, URL: {BookmarkUrl}", url);
            return Result<CreateBookmarkResponse>.Failure("Bookmark URL already exists.");
        }

        var metadata = await metadataService.GetAsync(url, ct);

        var title = !string.IsNullOrWhiteSpace(metadata?.Title) ? metadata.Title : request.Title;

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result<CreateBookmarkResponse>.Failure(
                "Title is required when metadata is unavailable."
            );
        }

        var cleanedName = title.Trim();

        var newBookmark = new Bookmark
        {
            Title = cleanedName,
            Url = url,
            ImageUrl = metadata?.Image?.Url,
            FaviconUrl = metadata?.Favicon?.Url,
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
