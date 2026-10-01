namespace BookyMark.Data.Entity;

public sealed class Bookmark
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Url { get; set; }
    public string? Notes { get; set; }
    public bool Favorite { get; set; }
    public string? ImageUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public int? CollectionId { get; set; }
    public Collection? Collection { get; set; }
}
