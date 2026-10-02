namespace BookyMark.Features.Bookmarks;

public static class DependencyInjection
{
    public static IServiceCollection AddBookmarkFeatures(this IServiceCollection services)
    {
        services.AddScoped<CreateBookmark>();
        services.AddScoped<UpdateBookmark>();
        services.AddScoped<DeleteBookmark>();

        services.AddScoped<GetBookmarks>();
        services.AddScoped<GetBookmarkDetails>();

        return services;
    }
}
