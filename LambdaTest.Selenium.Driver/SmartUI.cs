using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LambdaTest.Sdk.Utils;

namespace LambdaTest.Selenium.Driver
{
    public static class SmartUISnapshot
    {
        private static readonly ILogger SmartUILogger = Logger.CreateLogger("Lambdatest.Selenium.Driver");

        public static async Task<String> CaptureSnapshot(dynamic driver, string name, Dictionary<string, object> options = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The `snapshotName` argument is required.", nameof(name));
            }

            if (!await LambdaTest.Sdk.Utils.SmartUI.IsSmartUIEnabled())
            {
                throw new Exception("Cannot find SmartUI server.");
            }

            try
            {
                var domSerializerResponse = await LambdaTest.Sdk.Utils.SmartUI.FetchDomSerializer();

                if (domSerializerResponse == null)
                {
                    throw new Exception("Failed to fetch DOM serializer script response.");
                }

                var domSerializerScript = JsonSerializer.Deserialize<FetchDomSerializerResponse>(domSerializerResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (domSerializerScript == null || domSerializerScript.Data == null || domSerializerScript.Data.Dom == null)
                {
                    throw new Exception("Failed to json serialize the DOM serializer script.");
                }

                string script = domSerializerScript.Data.Dom;

                // Execute script using dynamic driver
                driver.ExecuteScript(script);

                // Extract sessionId from driver
                string sessionId = "";
                try
                {
                    sessionId = driver.SessionId?.ToString() ?? "";
                }
                catch
                {
                    // SessionId not available or accessible
                }

                if (options == null)
                {
                    options = new Dictionary<string, object>();
                }
                if (!string.IsNullOrEmpty(sessionId))
                {
                    options["sessionId"] = sessionId;
                }
                
                var optionsJSON = JsonSerializer.Serialize(options);
                var snapshotScript = @"
                    var options = " + optionsJSON + @";
                    return JSON.stringify({
                        dom: SmartUIDOM.serialize(options),
                        url: document.URL
                    });";

                // Execute script and get DOM JSON using dynamic driver
                var domJSON = (string)driver.ExecuteScript(snapshotScript);

                if (domJSON == null)
                {
                    throw new Exception("Failed to capture DOM object.");
                }

                var domContent = JsonSerializer.Deserialize<DomDeserializerResponse>(domJSON, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (domContent == null)
                {
                    throw new Exception("Failed to convert DOM object into JSON");
                }

                var dom = new LambdaTest.Sdk.Utils.SmartUI.DomObject
                {
                    Dom = new LambdaTest.Sdk.Utils.SmartUI.DomContent
                    {
                        html = domContent.Dom.html,
                        warnings = domContent.Dom.warnings,
                        resources = domContent.Dom.resources,
                        hints = domContent.Dom.hints
                    },
                    Name = name,
                    Url = domContent.Url
                };

                // Handle sync parameter if present
                if (options != null && options.ContainsKey("sync") && (bool)options["sync"])
                {
                    var contextId = Guid.NewGuid().ToString();
                    options["contextId"] = contextId;

                    // Post Snapshot
                    var apiResponseJSON = await LambdaTest.Sdk.Utils.SmartUI.PostSnapshot(dom, "Lambdatest.Selenium.Driver", options);
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(apiResponseJSON, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse != null && apiResponse.Data != null && apiResponse.Data.Warnings != null && apiResponse.Data.Warnings.Count > 0)
                    {
                        foreach (var warning in apiResponse.Data.Warnings)
                        {
                            SmartUILogger.LogWarning(warning);
                        }
                    }

                    SmartUILogger.LogInformation($"Snapshot captured: {name}");

                    // Get Snapshot Status
                    var timeout = 600;
                    if (options.ContainsKey("timeout"))
                    {
                        var tempTimeout = (int)options["timeout"];
                        if (tempTimeout < 30 || tempTimeout > 900)
                        {
                            SmartUILogger.LogWarning("Timeout value is out of range(30-900). Defaulting to 600 seconds.");
                        }
                        else
                        {
                            timeout = tempTimeout;
                        }
                    }
                    var snapshotStatusJSON = await LambdaTest.Sdk.Utils.SmartUI.GetSnapshotStatus(contextId, timeout, name);
                    var snapshotStatus = JsonSerializer.Deserialize<ApiResponse>(snapshotStatusJSON, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return snapshotStatusJSON;
                }
                else
                {
                    // If sync is not true, simply post the snapshot
                    var apiResponseJSON = await LambdaTest.Sdk.Utils.SmartUI.PostSnapshot(dom, "Lambdatest.Selenium.Driver", options);
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(apiResponseJSON, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse != null && apiResponse.Data != null && apiResponse.Data.Warnings != null && apiResponse.Data.Warnings.Count > 0)
                    {
                        foreach (var warning in apiResponse.Data.Warnings)
                        {
                            SmartUILogger.LogWarning(warning);
                        }
                    }

                    SmartUILogger.LogInformation($"Snapshot captured: {name}");
                }
                return "";
            }
            catch (Exception e)
            {
                SmartUILogger.LogError($"SmartUI snapshot failed: {name}");
                SmartUILogger.LogError(e.ToString());
                return "";
            }
        }


        private class ApiResponse
        {
            public ApiData Data { get; set; } = new ApiData();
        }

        private class ApiData
        {
            public string Message { get; set; } = string.Empty;
            public List<string> Warnings { get; set; } = new List<string>();
        }

        private class FetchDomSerializerResponse
        {
            public FetchDomSerializerData Data { get; set; } = new FetchDomSerializerData();
        }
        private class FetchDomSerializerData
        {
            public string Dom { get; set; } = string.Empty;
        }
        private class DomJSONContent
        {
            public string html { get; set; } = string.Empty;
            public List<string> warnings { get; set; } = new List<string>();
            public List<LambdaTest.Sdk.Utils.SmartUI.Resource> resources { get; set; } = new List<LambdaTest.Sdk.Utils.SmartUI.Resource>();
            public List<string> hints { get; set; } = new List<string>();
        }
        private class DomDeserializerResponse 
        {
            public DomJSONContent Dom { get; set; } = new DomJSONContent();
            public string Url { get; set; } = string.Empty;
        }
    }
}