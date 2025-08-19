using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using LambdaTest.Sdk.Utils;

namespace LambdaTest.Playwright.Driver
{
    public static class SmartUISnapshot
    {
        private static readonly ILogger SmartUILogger = Logger.CreateLogger("Lambdatest.Playwright.Driver");
        public static async Task CaptureSnapshot(IPage page, string name, Dictionary<string, object>? options = null)
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
                if (domSerializerScript?.Data?.Dom == null)
                {
                    throw new Exception("Failed to json serialize the DOM serializer script.");
                }

                string script = domSerializerScript.Data.Dom;

                if (options == null)
                {
                    options = new Dictionary<string, object>();
                }

                // Get test details from LambdaTestHook to extract the test ID
                string sessionId = "";
                try
                {
                    var testDetailsResponse = await page.EvaluateAsync<string>("_ => {}", "lambdatest_action: {\"action\": \"getTestDetails\"}");
                    if (!string.IsNullOrEmpty(testDetailsResponse))
                    {
                        var testDetails = JsonSerializer.Deserialize<TestDetailsResponse>(testDetailsResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        sessionId = testDetails?.Data?.SessionId ?? $"playwright_{Guid.NewGuid():N}";
                    }
                }
                catch (Exception)
                {
                    SmartUILogger.LogError("Failed to get test details from LambdaTestHook.");
                }
                if (!string.IsNullOrEmpty(sessionId))
                {   
                    // Append sessionId to options
                    options["sessionId"] = sessionId;
                }
                // Execute the DOM serializer script in the page context
                await page.EvaluateAsync(script);
                var optionsJSON = JsonSerializer.Serialize(options);
                var snapshotScript = @"
                () => {
                    var options = " + optionsJSON + @";
                    return JSON.stringify({
                        dom: SmartUIDOM.serialize(options),
                        url: document.URL
                    });
                }";

                var domJSON = await page.EvaluateAsync<string>(snapshotScript);
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

                var apiResponseJSON = await LambdaTest.Sdk.Utils.SmartUI.PostSnapshot(dom, "lambdatest-csharp-playwright-driver", options);
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
                SmartUILogger.LogError($"Playwright snapshot failed: {name}");
                SmartUILogger.LogError(e.ToString());
                throw;
            }
        }

        public static async Task CaptureSnapshot(IBrowserContext context, string name, Dictionary<string, object>? options = null)
        {
            // Capture snapshot from the first page in the context
            var pages = context.Pages;
            if (pages.Count > 0)
            {
                await CaptureSnapshot(pages[0], name, options);
            }
            else
            {
                throw new Exception("No pages available in the browser context for snapshot capture.");
            }
        }

        public static async Task CaptureSnapshot(IBrowser browser, string name, Dictionary<string, object>? options = null)
        {
            // Capture snapshot from the first context in the browser
            var contexts = browser.Contexts;
            if (contexts.Count > 0)
            {
                await CaptureSnapshot(contexts[0], name, options);
            }
            else
            {
                throw new Exception("No browser contexts available for snapshot capture.");
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
        
        private class TestDetailsResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("data")]
            public TestDetailsData Data { get; set; } = new TestDetailsData();
        }
        
        private class TestDetailsData
        {
            [System.Text.Json.Serialization.JsonPropertyName("test_id")]
            public string TestId { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("session_id")]
            public string SessionId { get; set; } = string.Empty;
        }
    }
}
