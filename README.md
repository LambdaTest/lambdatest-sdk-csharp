# LambdaTest C# SDK

## Overview

The LambdaTest C# SDK provides a set of utilities and integrations for working with LambdaTest's SmartUI and Selenium & Playwright services. This SDK is designed to streamline the process of capturing and analyzing SmartUI snapshots using various testing frameworks.

## Packages

### LambdaTest.Sdk.Utils

LambdaTest.Sdk.Utils is a utility library that offers core functionalities for the LambdaTest SmartUI CLI. It includes:

- Environment-based logging
- SmartUI server health check
- DOM serializer fetching
- SmartUI snapshot posting

### LambdaTest.Selenium.Driver

LambdaTest.Selenium.Driver integrates seamlessly with Selenium WebDriver to capture SmartUI snapshots. It utilizes LambdaTest.Sdk.Utils for core functionalities, providing:

- SmartUI snapshot capture using Selenium WebDriver
- Integration with LambdaTest SmartUI CLI
- Support for RemoteWebDriver and local WebDriver instances

### LambdaTest.Playwright.Driver

LambdaTest.Playwright.Driver provides Playwright integration with LambdaTest SmartUI for capturing visual snapshots during automated testing. Features include:

- SmartUI snapshot capture using Playwright
- Multi-level support (Page, BrowserContext, Browser)
- Cross-browser support (Chromium, Firefox, WebKit)

## Installation

To install the packages, use the .NET CLI:

```sh
# Core utilities
dotnet add package LambdaTest.Sdk.Utils --version 1.0.3

# Selenium integration
dotnet add package LambdaTest.Selenium.Driver --version 1.0.3

# Playwright integration
dotnet add package LambdaTest.Playwright.Driver --version 1.0.0
```

## Quick Start

### Selenium Example

```csharp
using OpenQA.Selenium;
using LambdaTest.Selenium.Driver;

var driver = new ChromeDriver();
driver.Navigate().GoToUrl("https://example.com");

await SmartUISnapshot.CaptureSnapshot(driver, "my-snapshot");
```

### Playwright Example

```csharp
using Microsoft.Playwright;
using LambdaTest.Playwright.Driver;

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();

await page.GotoAsync("https://example.com");
await SmartUISnapshot.CaptureSnapshot(page, "my-snapshot");
```

## License 

This project is licensed under the MIT License - see the LICENSE file for details.













