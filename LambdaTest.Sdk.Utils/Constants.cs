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
                throw new Exception("SmartUI server address not found");
            }
            return address;
        }
    }
}
