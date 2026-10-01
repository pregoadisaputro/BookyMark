namespace BookyMark.Features.LinkMetadata;

public sealed record LinkMetadataResponse(
    string? Title,
    string? Url,
    ImageMetadata? Image,
    ImageMetadata? Favicon
);

public sealed record ImageMetadata(string? Url);

public sealed class LinkMetadataService(HttpClient client, ILogger<LinkMetadataService> logger)
{
    private const string UrlPath = "v1/metadata?url=";

    public async Task<LinkMetadataResponse?> GetAsync(string url, CancellationToken ct = default)
    {
        if (
            !Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        )
        {
            logger.LogWarning("Invalid URL for {Url}", url);
            throw new ArgumentException("URL must be a valid HTTP or HTTPS.", nameof(url));
        }

        var response = await client.GetFromJsonAsync<LinkMetadataResponse>(
            $"{UrlPath}{Uri.EscapeDataString(url)}",
            ct
        );

        return response;
    }
}
