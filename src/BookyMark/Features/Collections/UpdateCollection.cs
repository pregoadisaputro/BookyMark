using BookyMark.Data;
using BookyMark.Features.Shared;
using Microsoft.EntityFrameworkCore;

namespace BookyMark.Features.Collections;

public sealed record UpdateCollectionRequest(string? Name, string? Description);

public sealed class UpdateCollection(
    IDbContextFactory<AppDbContext> dbCtxFactory,
    ILogger<UpdateCollection> logger
)
{
    public async Task<Result> HandleAsync(
        int id,
        UpdateCollectionRequest request,
        CancellationToken ct = default
    )
    {
        await using var db = await dbCtxFactory.CreateDbContextAsync(ct);

        var collection = await db.Collections.FirstOrDefaultAsync(c => c.Id == id, ct);

        if (collection is null)
        {
            logger.LogWarning("Collection is null, ID: {CollectionId}", id);
            return Result.Failure("Collection was not found or null.");
        }

        var cleanedName = request.Name?.Trim();

        if (
            !string.IsNullOrWhiteSpace(cleanedName)
            && !string.Equals(collection.Name, cleanedName, StringComparison.Ordinal)
        )
        {
            var loweredName = cleanedName.ToLower();

            var existingName = await db.Collections.AnyAsync(
                c => c.Id != id && c.Name.ToLower() == loweredName,
                ct
            );

            if (existingName)
            {
                logger.LogWarning(
                    "Collection Name already exists, Name: {CollectionName}",
                    cleanedName
                );
                return Result.Failure("Collection Name already exists.");
            }

            collection.Name = cleanedName;
        }

        var cleanedDescription = request.Description?.Trim();
        collection.Description = string.IsNullOrWhiteSpace(cleanedDescription)
            ? null
            : cleanedDescription;

        collection.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated Collection, ID: {CollectionId}", id);

        return Result.Success();
    }
}
