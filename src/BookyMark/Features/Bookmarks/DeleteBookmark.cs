using BookyMark.Data;
using BookyMark.Features.Shared;
using Microsoft.EntityFrameworkCore;

namespace BookyMark.Features.Bookmarks;

public sealed class DeleteBookmark(
    IDbContextFactory<AppDbContext> dbCtxFactory,
    ILogger<DeleteBookmark> logger
)
{
    public async Task<Result> HandleAsync(int id, CancellationToken ct = default)
    {
        await using var db = await dbCtxFactory.CreateDbContextAsync(ct);

        var deletedCount = await db.Bookmarks.Where(b => b.Id == id).ExecuteDeleteAsync(ct);

        if (deletedCount == 0)
        {
            logger.LogWarning("Trying deleting non-existing bookmark, ID: {BookmarkId}", id);
            return Result.Failure($"Bookmark was not found or null.");
        }

        logger.LogInformation("Deleted bookmark: {Count} & ID: {BookmarkId}", deletedCount, id);
        return Result.Success();
    }
}
