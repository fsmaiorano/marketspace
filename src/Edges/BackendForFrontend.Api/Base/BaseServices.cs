using BuildingBlocks.Http;

namespace BackendForFrontend.Api.Base;

public class BaseService(HttpClient httpClient) : HttpHelper(httpClient)
{
}