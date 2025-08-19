using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using LambdaTest.Playwright.Driver;

namespace Tests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("LambdaTest Playwright Driver Test");
            Console.WriteLine("===============================");
            
            try
            {
                // Initialize Playwright
                using var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });
                
                var context = await browser.NewContextAsync();
                var page = await context.NewPageAsync();
                
                // Navigate to a test page
                await page.GotoAsync("https://example.com");
                Console.WriteLine("✓ Successfully navigated to test page");
                
                // Test basic snapshot capture
                try
                {
                    await SmartUISnapshot.CaptureSnapshot(page, "test-snapshot");
                    Console.WriteLine("✓ Basic snapshot capture successful");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"x Snapshot capture failed (expected if SmartUI server not running): {ex.Message}");
                }
                
                // Test context-level snapshot
                try
                {
                    await SmartUISnapshot.CaptureSnapshot(context, "context-snapshot");
                    Console.WriteLine("✓ Context-level snapshot capture successful");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"x Context-level snapshot capture failed: {ex.Message}");
                }
                
                // Test browser-level snapshot
                try
                {
                    await SmartUISnapshot.CaptureSnapshot(browser, "browser-snapshot");
                    Console.WriteLine("✓ Browser-level snapshot capture successful");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"x Browser-level snapshot capture failed: {ex.Message}");
                }
                
                // Cleanup
                await page.CloseAsync();
                await context.CloseAsync();
                await browser.CloseAsync();
                
                Console.WriteLine("\n✓ All operations completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed: {ex.Message}");
            }
        }
    }
}