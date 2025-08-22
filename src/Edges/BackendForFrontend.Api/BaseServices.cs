using BuildingBlocks.Http;
using System.Net.Http;

namespace BackendForFrontend.Api;

public class BaseService(HttpClient httpClient) : HttpHelper(httpClient)
{
}