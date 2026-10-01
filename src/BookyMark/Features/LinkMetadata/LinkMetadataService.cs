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
    public async Task<LinkMetadataResponse?> GetAsync(string url, CancellationToken ct = default)
    {
        if (
            !Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp) && (uri.Scheme != Uri.UriSchemeHttps)
        )
        {
            logger.LogWarning("Invalid URL for {Url}", url);
            throw new ArgumentException("URL must be a valid HTTP or HTTPS.", nameof(url));
        }

        var response = await client.GetFromJsonAsync<LinkMetadataResponse>(
            $"/v1/metadata?url={Uri.EscapeDataString(url)}",
            ct
        );

        return response;
    }
}
