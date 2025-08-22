using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace BackendForFrontend.Api.Merchant.Services;

public interface IMerchantService
{
    
}

public class MerchantService(ILogger<MerchantService> service, HttpClient httpClient) : BaseService(httpClient), IMerchantService
{
    
}