#nullable enable

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace LambdaTest.Sdk.Utils
{
    public static class SmartUI
    {
        private static readonly HttpClient HttpClient = new();
        private static readonly ILogger SdkUtilsLogger = Logger.CreateLogger("LambdaTest.Sdk.Utils");


        public static async Task<bool> IsSmartUIEnabled()
        {
            try
            {
                var response = await HttpClient.GetAsync($"{Constants.GetSmartUIServerAddress()}/healthcheck");
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<string> FetchDomSerializer()
        {
            try
            {
                var response = await HttpClient.GetAsync($"{Constants.GetSmartUIServerAddress()}/domserializer");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                SdkUtilsLogger.LogDebug($"fetch DOMSerializer API failed - {e.Message}");
                throw new Exception("fetch DOMSerializer failed", e);
            }
            catch (Exception e)
            {
                SdkUtilsLogger.LogDebug($"fetch DOMSerializer failed - {e.Message}");
                throw new Exception("fetch DOMSerializer failed", e);
            }
        }

        public static async Task<string> PostSnapshot(DomObject snapshot, string pkg,  Dictionary<string, object>? options =null)
        {
            try
            {     

                var snapshotData = new SnapshotData
                {
                    dom = snapshot.Dom,
                    name = snapshot.Name,
                    url = snapshot.Url,
                    options = options ?? new Dictionary<string, object>() 
                };

                var jsonObject = new
                {
                    snapshot = snapshotData,
                    testType = pkg
                };

                var json = JsonSerializer.Serialize(jsonObject);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await HttpClient.PostAsync($"{Constants.GetSmartUIServerAddress()}/snapshot", content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();

            }
            catch (HttpRequestException e)
            {
                SdkUtilsLogger.LogDebug($"Snapshot Error: {e.Message}");
                throw new Exception("Snapshot Error", e);
            }
            catch (Exception e)
            {
                SdkUtilsLogger.LogDebug($"post snapshot failed : {e.Message}");
                throw new Exception("post snapshot failed", e);
            }
        }

        public static async Task<string> GetSnapshotStatus(string contextId, int pollTimeout, string snapshotName)
        {
            try
            {
                var url = $"{Constants.GetSmartUIServerAddress()}/snapshot/status?contextId={contextId}&pollTimeout={pollTimeout}&snapshotName={snapshotName}";
                
                // Using statement ensures proper disposal of HttpClient
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromHours(4);

                    var response = await client.GetAsync(url);
                    
                    // Ensure successful status code
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
            }
            catch (HttpRequestException e)
            {
                SdkUtilsLogger.LogDebug($"Snapshot Status Error: {e.Message}");
                throw new Exception("Snapshot Status Error", e);
            }
            catch (Exception e)
            {
                SdkUtilsLogger.LogDebug($"Get Snapshot Status failed: {e.Message}");
                throw new Exception("Get Snapshot Status failed", e);
            }
        }

        public class Resource
        {
            public string content { get; set; } = string.Empty;
            public string mimetype { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
        }
        public class DomContent 
        {
            public string html { get; set; } = string.Empty;
            public List<string> warnings { get; set; } = new List<string>();
            public List<Resource> resources { get; set; } = new List<Resource>();
            public List<string> hints { get; set; } = new List<string>();
        }

        public class DomObject
        {
            public DomContent Dom { get; set; } = new DomContent();
            public string Url { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        public class SnapshotData
        {
            public DomContent? dom { get; set; }
            public string? name { get; set; }
            public string? url { get; set; }
            public Dictionary<string, object>? options { get; set; } // Nullable for handling cases with or without options
        }
    }
}
