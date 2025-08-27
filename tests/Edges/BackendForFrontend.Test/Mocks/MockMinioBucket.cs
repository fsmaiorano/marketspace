using BuildingBlocks.Storage.Minio;

namespace BackendForFrontend.Test.Mocks;

public class MockMinioBucket : IMinioBucket
{
    public Task<(string objectName, string objectUrl)> SendImageAsync(string imageUrl)
    {
        string objectName = $"test-{Guid.NewGuid()}.jpg";
        string objectUrl = $"https://mock-minio.test/{objectName}";
        
        return Task.FromResult((objectName, objectUrl));
    }

    public Task<string?> GetImageAsync(string imageName)
    {
        return Task.FromResult($"https://mock-minio.test/{imageName}");
    }

    public Task<string> GetImageToDownload(string imageName)
    {
        return Task.FromResult($"https://mock-minio.test/{imageName}?download=true");
    }

    public Task DeleteImageAsync(string imageName)
    {
        // For testing, just return completed task
        return Task.CompletedTask;
    }
}
