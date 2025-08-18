using Minio;
using Minio.DataModel.Args;

namespace BuildingBlocks.Storage.Minio;

public interface IMinioBucket
{
    Task<(string objectName, string objectUrl)> SendImageAsync(string imageUrl);
    Task<string?> GetImageAsync(string imageName);
    Task<string> GetImageToDownload(string imageName);
    Task DeleteImageAsync(string imageName);
}

public class MinioBucket : IMinioBucket
{
    private const string BucketName = "masketspace";

    private readonly IMinioClient _minioClient;

    public MinioBucket(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<(string objectName, string objectUrl)> SendImageAsync(string imageUrl)
    {
        string fileExtension = Path.GetExtension(imageUrl);

        try
        {
            using HttpClient httpClient = CreateHttpClient();
            string contentType;

            try
            {
                using HttpResponseMessage headResponse =
                    await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, imageUrl));
                headResponse.EnsureSuccessStatusCode();
                contentType = headResponse.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
            {
                using HttpResponseMessage getResponse =
                    await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
                getResponse.EnsureSuccessStatusCode();
                contentType = getResponse.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            }

            if (string.IsNullOrEmpty(fileExtension))
                fileExtension = GetExtensionFromContentType(contentType);

            string objectName = $"{Guid.NewGuid()}{fileExtension}";

            using HttpResponseMessage response = await httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();
            await using Stream imageStream = await response.Content.ReadAsStreamAsync();

            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName));
            if (!found)
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(objectName)
                .WithStreamData(imageStream)
                .WithObjectSize(response.Content.Headers.ContentLength ?? -1)
                .WithContentType(contentType));

            Console.WriteLine($"Image '{objectName}' successfully uploaded to bucket '{BucketName}'!");
            

            string? objectUrl = await GetImageAsync(objectName);
            return (objectName, objectUrl)!;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return (string.Empty, string.Empty);
        }
    }

    public async Task<string?> GetImageAsync(string imageName)
    {
        try
        {
            string? presignedUrl = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(imageName)
                .WithExpiry(60 * 60)); // URL valid for 1 hour

            Console.WriteLine($"Temporary Url: {presignedUrl}");
            return presignedUrl;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error generating presigned URL: {e.Message}");
            return string.Empty;
        }
    }

    public async Task<string> GetImageToDownload(string imageName)
    {
        try
        {
            string? presignedUrl = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(imageName)
                .WithExpiry(60 * 60) // URL valid for 1 hour
                .WithHeaders(new Dictionary<string, string>
                {
                    ["response-content-disposition"] = $"attachment; filename=\"{imageName}\""
                }));

            Console.WriteLine($"Download URL: {presignedUrl}");
            return presignedUrl;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error generating download URL: {e.Message}");
            return string.Empty;
        }
    }

    public async Task DeleteImageAsync(string imageName)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(BucketName)
                .WithObject(imageName));

            Console.WriteLine($"Image '{imageName}' successfully removed!");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error removing image: {e.Message}");
        }
    }

    private static HttpClient CreateHttpClient()
    {
        return new HttpClient();
    }

    private static string GetExtensionFromContentType(string contentType)
    {
        string mimeType = contentType.Split(';')[0].Trim().ToLowerInvariant();

        return mimeType switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/bmp" => ".bmp",
            "image/tiff" => ".tiff",
            "image/svg+xml" => ".svg",
            "image/x-icon" => ".ico",
            "image/vnd.microsoft.icon" => ".ico",
            _ => ".jpg"
        };
    }
}