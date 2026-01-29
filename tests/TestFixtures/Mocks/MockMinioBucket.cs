using BuildingBlocks.Storage.Minio;

namespace MarketSpace.TestFixtures.Mocks;

public class MockMinioBucket : IMinioBucket
{
    public Task<(string objectName, string objectUrl)> SendImageAsync(string imageUrl)
    {
        string objectName = $"test-{Guid.CreateVersion7()}.jpg";
        string objectUrl = $"https://mock-minio.test/{objectName}";

        return Task.FromResult((objectName, objectUrl));
    }

    public Task<string?> GetImageAsync(string imageName)
    {
        return Task.FromResult<string?>($"https://mock-minio.test/{imageName}");
    }

    public Task<string> GetImageToDownload(string imageName)
    {
        return Task.FromResult($"https://mock-minio.test/{imageName}?download=true");
    }

    public Task DeleteImageAsync(string imageName)
    {
        return Task.CompletedTask;
    }
}
