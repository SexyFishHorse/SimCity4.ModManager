namespace Asser.Sc4Buddy.Server.Api.V1.Client;

using System;
using System.Collections.Generic;
using System.Net;
using Asser.Sc4Buddy.Server.Api.V1.Models;
using Newtonsoft.Json;
using RestSharp;

public class BuddyServerClient(IRestClient client) : IBuddyServerClient
{
    private const int MaxFilesPerPage = 100;

    public bool ServerEnabled { get; set; } = false;

    public IEnumerable<File> GetAllFiles() => GetAllItems<File>("files");

    public IEnumerable<Plugin> GetAllPlugins() => GetAllItems<Plugin>("plugins");

    public Plugin GetPlugin(Guid pluginId)
    {
        var request = new RestRequest("plugins/{pluginId}", Method.Get) { RequestFormat = DataFormat.Json };
        request.AddUrlSegment("pluginId", pluginId.ToString());

        var response = client.Get(request);
        if (response.IsSuccessful == false && response.ErrorException != null)
        {
            throw response.ErrorException;
        }

        return JsonConvert.DeserializeObject<Plugin>(response.Content);
    }

    private IEnumerable<T> GetAllItems<T>(string method)
    {
        if (ServerEnabled == false)
        {
            yield break;
        }

        var page = 1;
        do
        {
            var request = new RestRequest(method, Method.Get) { RequestFormat = DataFormat.Json };
            request.AddQueryParameter("page", page + string.Empty);
            request.AddQueryParameter("perPage", MaxFilesPerPage + string.Empty);

            var response = client.Get(request);

            if (response.IsSuccessful == false)
            {
                if (response.ErrorException != null)
                {
                    if (response.ErrorException is WebException webEx
                        && webEx.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        yield break;
                    }

                    throw response.ErrorException;
                }
            }

            var data = JsonConvert.DeserializeObject<List<T>>(response.Content);

            foreach (var item in data)
            {
                yield return item;
            }

            if (data.Count < MaxFilesPerPage)
            {
                yield break;
            }

            page++;
        } while (true);
    }
}