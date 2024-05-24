# LambdaTest C# SDK

## Overview

The LambdaTest C# SDK provides a set of utilities and integrations for working with LambdaTest's SmartUI and Selenium services. This SDK is designed to streamline the process of capturing and analyzing SmartUI snapshots using various testing frameworks.

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

## Installation

To install the packages, use the .NET CLI:

```sh
dotnet add package LambdaTest.Sdk.Utils --version 1.0.0
dotnet add package LambdaTest.Selenium.Driver --version 1.0.0
```

## License 

This project is licensed under the MIT License - see the LICENSE file for details.













