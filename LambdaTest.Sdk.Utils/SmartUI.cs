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

        public static async Task<string> PostSnapshot(DomObject snapshot, string pkg,  object options = null)
        {
            try
            {     
                object snapshotObject = new
                {
                    dom = snapshot.Dom,
                    name = snapshot.Name,
                    url = snapshot.Url
                };

                var jsonObject = new
                {
                    snapshot = options != null ? new { dom = snapshot.Dom, name = snapshot.Name, url = snapshot.Url, options } : snapshotObject,
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
        public class DomContent 
        {
            public string html { get; set; } = string.Empty;
            public List<string> warnings { get; set; } = new List<string>();
            public List<string> resources { get; set; } = new List<string>();
            public List<string> hints { get; set; } = new List<string>();
        }

        public class DomObject
        {
            public DomContent Dom { get; set; } = new DomContent();
            public string Url { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }
    }
}
