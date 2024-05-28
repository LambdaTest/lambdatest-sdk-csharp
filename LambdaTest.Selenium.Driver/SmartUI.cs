using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using LambdaTest.Sdk.Utils;

namespace LambdaTest.Selenium.Driver
{
    public static class SmartUISnapshot
    {
         private static readonly ILogger SmartUILogger = Logger.CreateLogger("Lambdatest.Selenium.Driver");

        public static async Task CaptureSnapshot(IWebDriver driver, string name, Dictionary<string, object>? options = null)
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
                    throw new Exception("Failed to fetch DOM serializer script repsonse.");
                }

                var domSerializerScript = JsonSerializer.Deserialize<FetchDomSerializerResponse>(domSerializerResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (domSerializerScript?.Data?.Dom == null)
                {
                    throw new Exception("Failed to json serialize the DOM serializer script.");
                }

                string script = domSerializerScript.Data.Dom;

                ((IJavaScriptExecutor)driver).ExecuteScript(script);

                var optionsJSON = JsonSerializer.Serialize(options);
                var snapshotScript = @"
                    var options = " + optionsJSON + @";
                    return JSON.stringify({
                        dom: SmartUIDOM.serialize(options),
                        url: document.URL
                    });";

                var domJSON = (string)((IJavaScriptExecutor)driver).ExecuteScript(snapshotScript);

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

                var apiResponseJSON = await LambdaTest.Sdk.Utils.SmartUI.PostSnapshot(dom, "Lambdatest.Selenium.Driver", options);
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(apiResponseJSON, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Data?.Warnings != null && apiResponse.Data.Warnings.Count > 0)
                {
                    foreach (var warning in apiResponse.Data.Warnings)
                    {
                        SmartUILogger.LogWarning(warning);
                    }
                }

                SmartUILogger.LogInformation($"Snapshot captured: {name}");
            }
            catch (Exception e)
            {
                SmartUILogger.LogError($"SmartUI snapshot failed: {name}");
                SmartUILogger.LogError(e.ToString());
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
            public List<string> resources { get; set; } = new List<string>();
            public List<string> hints { get; set; } = new List<string>();
        }
        private class DomDeserializerResponse 
        {
            public DomJSONContent Dom { get; set; } = new DomJSONContent();
            public string Url { get; set; } = string.Empty;

        }
    }
}
