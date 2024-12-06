using System.IO;
using System.Net.Http;

namespace Vendor.Services;

public static class RestService
{
    private static HttpClient GetClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        //var token = Preferences.Get("token", $"");
        //var token_type = Preferences.Get("token_type", $"bearer");

        //if (!string.IsNullOrEmpty(token))
        //{
        //    client.DefaultRequestHeaders.Add("Authorization", $"{token_type} {token}");
        //}

        return client;
    }

    public static async Task<string> PostImage(string localpath)
    {
        string resulturl = "";
        var lisUrl = $"https://lis.4dev.kz/upload";
        var client = GetClient();
        MultipartFormDataContent form = new MultipartFormDataContent();
        Stream fileStream = File.OpenRead(localpath);
        form.Add(new StreamContent(fileStream), "file", "tempitem.NamePhoto");
        var req = new HttpRequestMessage(HttpMethod.Post, lisUrl)
        {
            Content = form
        };

        try
        {
            var response = await client.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                resulturl = await response.Content.ReadAsStringAsync();
            }
            else
            {
                var responseData = await response.Content.ReadAsStringAsync();
                throw new Exception(responseData);
            }
        }
        catch (Exception)
        {
            resulturl = string.Empty;
        }

        return resulturl;
    }
}
