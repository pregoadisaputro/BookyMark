namespace BookyMark.Features.Bookmarks;

public static class DependencyInjection
{
    public static IServiceCollection AddBookmarkFeatures(this IServiceCollection services)
    {
        services.AddScoped<CreateBookmark>();
        services.AddScoped<UpdateBookmark>();

        services.AddScoped<GetBookmarks>();

        return services;
    }
}
