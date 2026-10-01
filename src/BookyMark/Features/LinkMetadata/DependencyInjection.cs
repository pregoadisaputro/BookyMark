namespace BookyMark.Features.LinkMetadata;

public static class DependencyInjection
{
    public static IServiceCollection AddLinkMetadataService(this IServiceCollection services)
    {
        services.AddHttpClient<LinkMetadataService>(client =>
        {
            client.BaseAddress = new Uri("https://api.linkmetadata.com");
        });

        return services;
    }
}
