namespace DeployAgent.Services;

public class OpenApiService : IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed;

    public OpenApiService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<byte[]> DownloadOpenApiSpecAsync(string openApiSpecUrl)
    {
        if (string.IsNullOrWhiteSpace(openApiSpecUrl))
        {
            throw new ArgumentException("OpenAPI spec URL cannot be null or empty", nameof(openApiSpecUrl));
        }

        Console.WriteLine($"Downloading OpenAPI specification from: {openApiSpecUrl}");

        try
        {
            var openApiSpec = await _httpClient.GetByteArrayAsync(openApiSpecUrl);
            Console.WriteLine("OpenAPI specification downloaded successfully.");
            return openApiSpec;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error: Failed to download OpenAPI specification: {ex.Message}");
            throw new InvalidOperationException($"Failed to download OpenAPI specification from {openApiSpecUrl}", ex);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }
}