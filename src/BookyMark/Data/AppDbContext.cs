using BookyMark.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookyMark.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<Collection> Collections => Set<Collection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset))
                {
                    property.SetValueConverter(
                        new ValueConverter<DateTimeOffset, DateTime>(
                            value => value.UtcDateTime,
                            value => new DateTimeOffset(
                                DateTime.SpecifyKind(value, DateTimeKind.Utc)
                            )
                        )
                    );
                }
            }
        }
    }
}
