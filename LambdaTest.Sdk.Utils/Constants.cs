using System;

namespace LambdaTest.Sdk.Utils
{
    public static class Constants
    {
        public static string GetSmartUIServerAddress()
        {
            var address = Environment.GetEnvironmentVariable("SMARTUI_SERVER_ADDRESS");
            if (string.IsNullOrEmpty(address))
            {
                return "http://localhost:49152";
            }
            return address;
        }
    }
}
