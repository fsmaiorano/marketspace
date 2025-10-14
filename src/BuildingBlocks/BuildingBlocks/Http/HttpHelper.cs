using System.Collections;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BuildingBlocks.Http;

public abstract class HttpHelper(HttpClient httpClient)
{
    protected async Task<HttpResponseMessage> DoPost(string method, object request, string token = "",
        string culture = "en-US")
    {
        ChangeRequestCulture(httpClient, culture);
        AuthorizeRequest(httpClient, token);
        return await httpClient.PostAsJsonAsync(method, request);
    }

    protected async Task<HttpResponseMessage> DoGet(string method, string token = "", string culture = "en-US")
    {
        ChangeRequestCulture(httpClient, culture);
        AuthorizeRequest(httpClient, token);
        return await httpClient.GetAsync(method);
    }

    protected async Task<HttpResponseMessage> DoPut(string method, object request, string token = "",
        string culture = "en-US")
    {
        ChangeRequestCulture(httpClient, culture);
        AuthorizeRequest(httpClient, token);
        return await httpClient.PutAsJsonAsync(method, request);
    }

    protected async Task<HttpResponseMessage> DoPatch(string method, object request, string token = "",
        string culture = "en-US")
    {
        ChangeRequestCulture(httpClient, culture);
        AuthorizeRequest(httpClient, token);
        return await httpClient.PatchAsJsonAsync(method, request);
    }

    protected async Task<HttpResponseMessage> DoDelete(string method, string token = "", string culture = "en-US")
    {
        ChangeRequestCulture(httpClient, culture);
        AuthorizeRequest(httpClient, token);

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return await httpClient.DeleteAsync(method);
    }

    protected async Task<HttpResponseMessage> DoPostFormData(string method, object request, string token,
        string culture = "en-US")
    {
        ChangeRequestCulture(httpClient, culture);
        AuthorizeRequest(httpClient, token);

        var multipartContent = new MultipartFormDataContent();
        var requestProperties = request.GetType().GetProperties().ToList();

        foreach (var property in requestProperties)
        {
            var propertyValue = property.GetValue(request);
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

        return await httpClient.PostAsync(method, multipartContent);
    }

    private static void ChangeRequestCulture(HttpClient client, string culture)
    {
        if (client.DefaultRequestHeaders.Contains("Accept-Language"))
            client.DefaultRequestHeaders.Remove("Accept-Language");

        client.DefaultRequestHeaders.Add("Accept-Language", culture);
    }

    private static void AuthorizeRequest(HttpClient client, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        if (client.DefaultRequestHeaders.Contains("Authorization"))
            client.DefaultRequestHeaders.Remove("Authorization");

        token = token.StartsWith("Bearer ") ? token[7..] : token;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    private static void AddListToMultipartContent(MultipartFormDataContent multipartContent, string propertyName,
        IList list)
    {
        var itemType = list.GetType().GetGenericArguments().Single();

        if (itemType.IsClass && itemType != typeof(string))
        {
            AddClassListToMultipartContent(multipartContent, propertyName, list);
        }
        else
        {
            foreach (var item in list)
            {
                multipartContent.Add(new StringContent(item.ToString()!), propertyName);
            }
        }
    }

    private static void AddClassListToMultipartContent(MultipartFormDataContent multipartContent, string propertyName,
        IList list)
    {
        var index = 0;

        foreach (var item in list)
        {
            var classPropertiesInfo = item.GetType().GetProperties().ToList();

            foreach (var prop in classPropertiesInfo)
            {
                var value = prop.GetValue(item, null);
                multipartContent.Add(new StringContent(value!.ToString()!), $"{propertyName}[{index}][{prop.Name}]");
            }

            index++;
        }
    }
}