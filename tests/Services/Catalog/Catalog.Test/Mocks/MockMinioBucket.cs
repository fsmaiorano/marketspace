using BuildingBlocks.Storage.Minio;

namespace Catalog.Test.Mocks;

public class MockMinioBucket : IMinioBucket
{
    public Task<(string objectName, string objectUrl)> SendImageAsync(string imageUrl)
    {
        // For testing, just return a mock object name and URL
        string objectName = $"catalog-test-{Guid.CreateVersion7()}.jpg";
        string objectUrl = $"https://mock-minio-catalog.test/{objectName}";
        
        return Task.FromResult((objectName, objectUrl));
    }

    public Task<string?> GetImageAsync(string imageName)
    {
        return Task.FromResult($"https://mock-minio-catalog.test/{imageName}");
    }

    public Task<string> GetImageToDownload(string imageName)
    {
        return Task.FromResult($"https://mock-minio-catalog.test/{imageName}?download=true");
    }

    public Task DeleteImageAsync(string imageName)
    {
        // For testing, just return completed task
        return Task.CompletedTask;
    }
}
