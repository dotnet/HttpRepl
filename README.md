HttpRepl
=======
[![Build Status](https://dev.azure.com/dnceng/public/_apis/build/status/aspnet/HttpRepl/aspnet-HttpRepl-CI?branchName=master)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=538&branchName=master)

The HTTP Read-Eval-Print Loop (REPL) is:

- A lightweight, cross-platform command-line tool that's supported everywhere .NET Core is supported.
- Used for making HTTP requests to test ASP.NET Core web APIs and view their results.

## Installation

To install the HttpRepl, run the following command:

```
dotnet tool install -g Microsoft.dotnet-httprepl
```

A [.NET Core Global Tool](https://docs.microsoft.com/dotnet/core/tools/global-tools#install-a-global-tool) is installed from the [Microsoft.dotnet-httprepl](https://www.nuget.org/packages/Microsoft.dotnet-httprepl) NuGet package.

## Usage

See the [documentation](https://aka.ms/http-repl-doc) for how to use and configure HttpRepl.

## Telemetry

See the [documentation](https://docs.microsoft.com/aspnet/core/web-api/http-repl/telemetry) for information about the usage data collection.

## Building

To build this repo, run the `build.cmd` or `build.sh` in the root of this repo. This repo uses the .NET [Arcade toolset](https://github.com/dotnet/arcade).

## Contributing

See the [Contributing Guide](/CONTRIBUTING.md) for details on what it means to contribute and how to do so.

## Reporting security issues and bugs

Security issues and bugs should be reported privately, via email, to the Microsoft Security Response Center (MSRC) secure@microsoft.com. You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message. Further information, including the MSRC PGP key, can be found in the [Security TechCenter](https://technet.microsoft.com/security/ff852094.aspx).
