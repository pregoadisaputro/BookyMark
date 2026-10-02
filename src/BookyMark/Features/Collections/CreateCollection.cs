using BookyMark.Data;
using BookyMark.Data.Entity;
using BookyMark.Features.Shared;
using Microsoft.EntityFrameworkCore;

namespace BookyMark.Features.Collections;

public sealed record CreateCollectionRequest(string Name, string? Description);

public sealed record CreateCollectionResponse(int Id);

public sealed class CreateCollection(
    IDbContextFactory<AppDbContext> dbCtxFactory,
    ILogger<CreateCollection> logger
)
{
    public async Task<Result<CreateCollectionResponse>> HandleAsync(
        CreateCollectionRequest request,
        CancellationToken ct = default
    )
    {
        var cleanedName = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(cleanedName))
        {
            logger.LogWarning("Name cannot be empty or whitespace.");
            return Result<CreateCollectionResponse>.Failure("Name cannot be empty or whitespace.");
        }

        await using var db = await dbCtxFactory.CreateDbContextAsync(ct);

        var loweredName = cleanedName.ToLower();

        var existingName = await db.Collections.AnyAsync(c => c.Name.ToLower() == loweredName, ct);

        if (existingName)
        {
            logger.LogWarning(
                "Collection Name already exists, Name: {CollectionName}",
                cleanedName
            );
            return Result<CreateCollectionResponse>.Failure("Collection Name already exists.");
        }

        var cleanedDescription = request.Description?.Trim();

        var newCollection = new Collection
        {
            Name = cleanedName,
            Description = string.IsNullOrWhiteSpace(cleanedDescription) ? null : cleanedDescription,
        };

        db.Collections.Add(newCollection);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created collection, Name: {CollectionName} & ID: {CollectionId}",
            newCollection.Name,
            newCollection.Id
        );

        return Result<CreateCollectionResponse>.Success(
            new CreateCollectionResponse(newCollection.Id)
        );
    }
}
