using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LambdaTest.Sdk.Utils
{
    public static class SmartUI
    {
        private static readonly HttpClient HttpClient;
        private static readonly ILogger SdkUtilsLogger = Logger.CreateLogger("LambdaTest.Sdk.Utils");

        // Static constructor with best balance approach
        static SmartUI()
        {
#if NET48 || NET472 || NET462 || NET461 || NET46 || NETFRAMEWORK
            ServicePointManager.DefaultConnectionLimit = 50;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
#endif
            
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            
            HttpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        private static string FormatHeaders(HttpHeaders headers)
        {
            var headerStrings = new List<string>();
            foreach (var header in headers)
            {
                headerStrings.Add($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            return headerStrings.Count > 0 ? string.Join("; ", headerStrings) : "None";
        }

        public static async Task<bool> IsSmartUIEnabled()
        {
            var url = $"{Constants.GetSmartUIServerAddress()}/healthcheck";
            SdkUtilsLogger.LogInformation($"[SmartUI] IsSmartUIEnabled - Attempting to call API: {url}");
            
            try
            {
                SdkUtilsLogger.LogInformation($"[SmartUI] IsSmartUIEnabled - Making GET request to: {url}");
                SdkUtilsLogger.LogInformation($"[SmartUI] IsSmartUIEnabled - Timeout: {HttpClient.Timeout}");
                SdkUtilsLogger.LogInformation($"[SmartUI] IsSmartUIEnabled - Request Headers: {FormatHeaders(HttpClient.DefaultRequestHeaders)}");
                
                var response = await HttpClient.GetAsync(url);
                
                SdkUtilsLogger.LogInformation($"[SmartUI] IsSmartUIEnabled - Response Status Code: {(int)response.StatusCode} {response.StatusCode}");
                SdkUtilsLogger.LogInformation($"[SmartUI] IsSmartUIEnabled - Response Headers: {FormatHeaders(response.Headers)}");
                
                if (response.Content != null)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    SdkUtilsLogger.LogInformation($"[SmartUI] IsSmartUIEnabled - Response Content-Type: {response.Content.Headers.ContentType}, Content-Length: {response.Content.Headers.ContentLength}");
                    SdkUtilsLogger.LogInformation($"[SmartUI] IsSmartUIEnabled - Response Body: {responseBody ?? "null"}");
                }
                
                response.EnsureSuccessStatusCode();
                SdkUtilsLogger.LogInformation($"[SmartUI] IsSmartUIEnabled - API call successful, SmartUI is enabled");
                return true;
            }
            catch (HttpRequestException e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] IsSmartUIEnabled - HttpRequestException occurred. URL: {url}, Message: {e.Message}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] IsSmartUIEnabled - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] IsSmartUIEnabled - StackTrace: {e.StackTrace}");
                return false;
            }
            catch (TaskCanceledException e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] IsSmartUIEnabled - TaskCanceledException (Timeout) occurred. URL: {url}, Message: {e.Message}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] IsSmartUIEnabled - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] IsSmartUIEnabled - StackTrace: {e.StackTrace}");
                return false;
            }
            catch (Exception e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] IsSmartUIEnabled - Exception occurred. URL: {url}, Message: {e.Message}, Type: {e.GetType().FullName}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] IsSmartUIEnabled - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] IsSmartUIEnabled - StackTrace: {e.StackTrace}");
                return false;
            }
        }

        public static async Task<string> FetchDomSerializer()
        {
            var url = $"{Constants.GetSmartUIServerAddress()}/domserializer";
            SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - Attempting to call API: {url}");
            
            try
            {
                SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - Making GET request to: {url}");
                SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - Timeout: {HttpClient.Timeout}");
                SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - Request Headers: {FormatHeaders(HttpClient.DefaultRequestHeaders)}");
                
                var response = await HttpClient.GetAsync(url);
                
                SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - Response Status Code: {(int)response.StatusCode} {response.StatusCode}");
                SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - Response Headers: {FormatHeaders(response.Headers)}");
                
                if (response.Content != null)
                {
                    SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - Response Content-Type: {response.Content.Headers.ContentType}, Content-Length: {response.Content.Headers.ContentLength}");
                    
                    var responseBody = await response.Content.ReadAsStringAsync();
                    SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - Response Body Length: {responseBody?.Length ?? 0} characters");
                    if (responseBody != null && responseBody.Length < 1000)
                    {
                        SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - Response Body: {responseBody}");
                    }
                    else if (responseBody != null)
                    {
                        SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - Response Body (first 500 chars): {responseBody.Substring(0, Math.Min(500, responseBody.Length))}...");
                    }
                    
                    response.EnsureSuccessStatusCode();
                    SdkUtilsLogger.LogInformation($"[SmartUI] FetchDomSerializer - API call successful");
                    return responseBody;
                }
                else
                {
                    SdkUtilsLogger.LogWarning($"[SmartUI] FetchDomSerializer - Response content is null");
                    response.EnsureSuccessStatusCode();
                    return string.Empty;
                }
            }
            catch (HttpRequestException e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] FetchDomSerializer - HttpRequestException occurred. URL: {url}, Message: {e.Message}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] FetchDomSerializer - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] FetchDomSerializer - StackTrace: {e.StackTrace}");
                throw new Exception($"fetch DOMSerializer failed - URL: {url}, Message: {e.Message}", e);
            }
            catch (TaskCanceledException e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] FetchDomSerializer - TaskCanceledException (Timeout) occurred. URL: {url}, Message: {e.Message}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] FetchDomSerializer - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] FetchDomSerializer - StackTrace: {e.StackTrace}");
                throw new Exception($"fetch DOMSerializer failed - URL: {url}, Timeout occurred: {e.Message}", e);
            }
            catch (Exception e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] FetchDomSerializer - Exception occurred. URL: {url}, Message: {e.Message}, Type: {e.GetType().FullName}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] FetchDomSerializer - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] FetchDomSerializer - StackTrace: {e.StackTrace}");
                throw new Exception($"fetch DOMSerializer failed - URL: {url}, Message: {e.Message}", e);
            }
        }

        public static async Task<string> PostSnapshot(DomObject snapshot, string pkg, Dictionary<string, object> options = null)
        {
            var url = $"{Constants.GetSmartUIServerAddress()}/snapshot";
            SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Attempting to call API: {url}");
            
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

                var json = JsonConvert.SerializeObject(jsonObject);

                SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Snapshot Name: {snapshot?.Name ?? "null"}, URL: {snapshot?.Url ?? "null"}, Test Type: {pkg ?? "null"}");
                SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Request Payload Length: {json.Length} characters");
                
                if (json.Length < 2000)
                {
                    SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Request Payload: {json}");
                }
                else
                {
                    SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Request Payload (first 500 chars): {json.Substring(0, Math.Min(500, json.Length))}...");
                }
                
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Making POST request to: {url}");
                SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Timeout: {HttpClient.Timeout}");
                SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Request Headers: {FormatHeaders(HttpClient.DefaultRequestHeaders)}");
                SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Content-Type: {content.Headers.ContentType}, Content-Length: {content.Headers.ContentLength}");
                
                var response = await HttpClient.PostAsync(url, content);
                            
                SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Response Status Code: {(int)response.StatusCode} {response.StatusCode}");
                SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Response Headers: {FormatHeaders(response.Headers)}");
                
                if (response.Content != null)
                {
                    SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Response Content-Type: {response.Content.Headers.ContentType}, Content-Length: {response.Content.Headers.ContentLength}");
                    
                    var responseBody = await response.Content.ReadAsStringAsync();
                    SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Response Body Length: {responseBody?.Length ?? 0} characters");
                    if (responseBody != null && responseBody.Length < 1000)
                    {
                        SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Response Body: {responseBody}");
                    }
                    else if (responseBody != null)
                    {
                        SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - Response Body (first 500 chars): {responseBody.Substring(0, Math.Min(500, responseBody.Length))}...");
                    }
                    
                    response.EnsureSuccessStatusCode();
                    SdkUtilsLogger.LogInformation($"[SmartUI] PostSnapshot - API call successful");
                    return responseBody;
                }
                else
                {
                    SdkUtilsLogger.LogWarning($"[SmartUI] PostSnapshot - Response content is null");
                    response.EnsureSuccessStatusCode();
                    return string.Empty;
                }
            }
            catch (HttpRequestException e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] PostSnapshot - HttpRequestException occurred. URL: {url}, Message: {e.Message}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] PostSnapshot - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] PostSnapshot - StackTrace: {e.StackTrace}");
                throw new Exception($"Snapshot Error - URL: {url}, Message: {e.Message}", e);
            }
            catch (TaskCanceledException e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] PostSnapshot - TaskCanceledException (Timeout) occurred. URL: {url}, Message: {e.Message}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] PostSnapshot - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] PostSnapshot - StackTrace: {e.StackTrace}");
                throw new Exception($"Snapshot Error - URL: {url}, Timeout occurred: {e.Message}", e);
            }
            catch (Exception e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] PostSnapshot - Exception occurred. URL: {url}, Message: {e.Message}, Type: {e.GetType().FullName}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] PostSnapshot - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] PostSnapshot - StackTrace: {e.StackTrace}");
                throw new Exception($"post snapshot failed - URL: {url}, Message: {e.Message}", e);
            }
        }

        public static async Task<string> GetSnapshotStatus(string contextId, int pollTimeout, string snapshotName)
        {
            var url = $"{Constants.GetSmartUIServerAddress()}/snapshot/status?contextId={contextId}&pollTimeout={pollTimeout}&snapshotName={snapshotName}";
            SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - Attempting to call API: {url}");
            SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - ContextId: {contextId}, PollTimeout: {pollTimeout}, SnapshotName: {snapshotName ?? "null"}");
            
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromHours(4);
                    SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - Making GET request to: {url}");
                    SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - HttpClient Timeout: {client.Timeout}");
                    SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - Request Headers: {FormatHeaders(client.DefaultRequestHeaders)}");

                    var response = await client.GetAsync(url);
                        
                    SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - Response Status Code: {(int)response.StatusCode} {response.StatusCode}");
                    SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - Response Headers: {FormatHeaders(response.Headers)}");
                    
                    if (response.Content != null)
                    {
                        SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - Response Content-Type: {response.Content.Headers.ContentType}, Content-Length: {response.Content.Headers.ContentLength}");
                    }
                    
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - Response Body Length: {responseBody?.Length ?? 0} characters");
                    if (responseBody != null && responseBody.Length < 1000)
                    {
                        SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - Response Body: {responseBody}");
                    }
                    else if (responseBody != null)
                    {
                        SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - Response Body (first 500 chars): {responseBody.Substring(0, Math.Min(500, responseBody.Length))}...");
                    }
                    
                    SdkUtilsLogger.LogInformation($"[SmartUI] GetSnapshotStatus - API call successful");
                    return responseBody;
                }
            }
            catch (HttpRequestException e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] GetSnapshotStatus - HttpRequestException occurred. URL: {url}, Message: {e.Message}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] GetSnapshotStatus - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] GetSnapshotStatus - StackTrace: {e.StackTrace}");
                throw new Exception($"Snapshot Status Error - URL: {url}, Message: {e.Message}", e);
            }
            catch (TaskCanceledException e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] GetSnapshotStatus - TaskCanceledException (Timeout) occurred. URL: {url}, Message: {e.Message}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] GetSnapshotStatus - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] GetSnapshotStatus - StackTrace: {e.StackTrace}");
                throw new Exception($"Snapshot Status Error - URL: {url}, Timeout occurred: {e.Message}", e);
            }
            catch (Exception e)
            {
                SdkUtilsLogger.LogError(e, $"[SmartUI] GetSnapshotStatus - Exception occurred. URL: {url}, Message: {e.Message}, Type: {e.GetType().FullName}");
                if (e.InnerException != null)
                {
                    SdkUtilsLogger.LogError(e.InnerException, $"[SmartUI] GetSnapshotStatus - Inner Exception: {e.InnerException.Message}, StackTrace: {e.InnerException.StackTrace}");
                }
                SdkUtilsLogger.LogError($"[SmartUI] GetSnapshotStatus - StackTrace: {e.StackTrace}");
                throw new Exception($"Get Snapshot Status failed - URL: {url}, Message: {e.Message}", e);
            }
        }

        // Data classes
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
            public DomContent dom { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> options { get; set; }
        }
    }
}