namespace BookyMark.Features.Bookmarks;

public static class DependencyInjection
{
    public static IServiceCollection AddBookmakrsFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateBookmark>();

        services.AddScoped<GetBookmarks>();

        return services;
    }
}
