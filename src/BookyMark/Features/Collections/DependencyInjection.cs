namespace BookyMark.Features.Collections;

public static class DependencyInjection
{
    public static IServiceCollection AddCollectionFeatures(this IServiceCollection services)
    {
        services.AddScoped<CreateCollection>();
        services.AddScoped<UpdateCollection>();

        return services;
    }
}
