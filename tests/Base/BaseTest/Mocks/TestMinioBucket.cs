using BuildingBlocks.Storage.Minio;

namespace BaseTest.Mocks;

public class TestMinioBucket : IMinioBucket
{
    public Task<(string objectName, string objectUrl)> SendImageAsync(string imageUrl)
    {
        string objectName = $"test-{Guid.NewGuid()}.jpg";
        string objectUrl = $"https://test-minio/{objectName}";
        return Task.FromResult((objectName, objectUrl));
    }

    public Task<string?> GetImageAsync(string imageName)
    {
        return Task.FromResult<string?>($"https://test-minio/{imageName}");
    }

    public Task<string> GetImageToDownload(string imageName)
    {
        return Task.FromResult($"https://test-minio/{imageName}?download=true");
    }

    public Task DeleteImageAsync(string imageName)
    {
        return Task.CompletedTask;
    }
}