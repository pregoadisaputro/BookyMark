using System.Text.Json;
using BookyMark.Features.Shared;

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

    public async Task<Result<LinkMetadataResponse?>> GetAsync(
        string url,
        CancellationToken ct = default
    )
    {
        var cleanedUrl = url.Trim();

        if (
            string.IsNullOrWhiteSpace(cleanedUrl)
            || !Uri.TryCreate(cleanedUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        )
        {
            logger.LogWarning("Invalid URL for {Url}", cleanedUrl);
            return Result<LinkMetadataResponse?>.Failure(
                "URL must be a valid HTTP or HTTPS address"
            );
        }

        try
        {
            var response = await client.GetFromJsonAsync<LinkMetadataResponse>(
                $"{UrlPath}{Uri.EscapeDataString(cleanedUrl)}",
                ct
            );

            return Result<LinkMetadataResponse?>.Success(response);
        }
        catch (Exception ex)
            when (ex is HttpRequestException or JsonException
                || (ex is TaskCanceledException && !ct.IsCancellationRequested)
            )
        {
            logger.LogWarning(ex, "Metadata lookup failed for {Url}", cleanedUrl);
            return Result<LinkMetadataResponse?>.Success(null);
        }
    }
}
