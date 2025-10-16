namespace BuildingBlocks.Storage.Minio;

public interface IMinioBucket
{
    Task<(string objectName, string objectUrl)> SendImageAsync(string imageUrl);
    Task<string?> GetImageAsync(string imageName);
    Task<string> GetImageToDownload(string imageName);
    Task DeleteImageAsync(string imageName);
}