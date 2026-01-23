using System.Collections;
using System.Net.Http.Headers;
using System.Reflection;

namespace BackendForFrontend.Test.Api;

public class HttpFixture(BackendForFrontendFactory factory) : IClassFixture<BackendForFrontendFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    protected async Task<HttpResponseMessage> DoPost(string method, object request, string token = "",
        string culture = "en-US")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);
        return await _httpClient.PostAsJsonAsync(method, request);
    }

    protected async Task<HttpResponseMessage> DoGet(string method, string token = "", string culture = "en-US")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);
        return await _httpClient.GetAsync(method);
    }

    protected async Task<HttpResponseMessage> DoPut(string method, object request, string token = "", string culture = "en-US")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);
        return await _httpClient.PutAsJsonAsync(method, request);
    }

    protected async Task<HttpResponseMessage> DoPatch(string method, object request, string token = "", string culture = "en-US")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);
        return await _httpClient.PatchAsJsonAsync(method, request);
    }

    protected async Task<HttpResponseMessage> DoPostFormData(
        string method,
        object request,
        string token,
        string culture = "en-US")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);

        MultipartFormDataContent multipartContent = new();

        List<PropertyInfo> requestProperties = request.GetType().GetProperties().ToList();

        foreach (PropertyInfo property in requestProperties)
        {
            object? propertyValue = property.GetValue(request);

            if (string.IsNullOrWhiteSpace(propertyValue?.ToString()))
                continue;

            if (propertyValue is IList list)
            {
                AddListToMultipartContent(multipartContent, property.Name, list);
            }
            else
            {
                multipartContent.Add(new StringContent(propertyValue.ToString()!), property.Name);
            }
        }

        return await _httpClient.PostAsync(method, multipartContent);
    }

    protected async Task<HttpResponseMessage> DoDelete(string method, string token = "", string culture = "en-US")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);

        return await _httpClient.DeleteAsync(method);
    }

    private void ChangeRequestCulture(string culture)
    {
        if (_httpClient.DefaultRequestHeaders.Contains("Accept-Language"))
            _httpClient.DefaultRequestHeaders.Remove("Accept-Language");

        _httpClient.DefaultRequestHeaders.Add("Accept-Language", culture);
    }

    private void AuthorizeRequest(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static void AddListToMultipartContent(
        MultipartFormDataContent multipartContent,
        string propertyName,
        IList list)
    {
        Type itemType = list.GetType().GetGenericArguments().Single();

        if (itemType.IsClass && itemType != typeof(string))
        {
            AddClassListToMultipartContent(multipartContent, propertyName, list);
        }
        else
        {
            foreach (object? item in list)
            {
                multipartContent.Add(new StringContent(item.ToString()!), propertyName);
            }
        }
    }

    private static void AddClassListToMultipartContent(
        MultipartFormDataContent multipartContent,
        string propertyName,
        IList list)
    {
        int index = 0;

        foreach (object? item in list)
        {
            List<PropertyInfo> classPropertiesInfo = item.GetType().GetProperties().ToList();

            foreach (PropertyInfo prop in classPropertiesInfo)
            {
                object? value = prop.GetValue(item, null);
                multipartContent.Add(new StringContent(value!.ToString()!), $"{propertyName}[{index}][{prop.Name}]");
            }

            index++;
        }
    }
}