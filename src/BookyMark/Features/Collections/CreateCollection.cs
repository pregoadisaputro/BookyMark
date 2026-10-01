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
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            logger.LogWarning("Name cannot be empty or whitespace.");
            return Result<CreateCollectionResponse>.Failure("Name cannot be empty or whitespace.");
        }

        await using var db = await dbCtxFactory.CreateDbContextAsync(ct);

        var newCollection = new Collection
        {
            Name = request.Name,
            Description = request.Description,
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
