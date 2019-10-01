// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class ConnectCommandTests : CommandTestsBase
    {
        [Fact]
        public async Task ExecuteAsync_WithNothingSpecified_ShowsError()
        {
            ArrangeInputs("connect",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(Resources.Strings.ConnectCommand_Error_NothingSpecified, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidRoot_ShowsError()
        {
            ArrangeInputs("connect example.com",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(Resources.Strings.ConnectCommand_Error_RootAddressNotValid, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithNoRootAndRelativeBase_ShowsError()
        {
            ArrangeInputs("connect --base /v1",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(Resources.Strings.ConnectCommand_Error_NoRootNoAbsoluteBase, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithNoRootAndRelativeSwagger_ShowsError()
        {
            ArrangeInputs("connect --swagger /v1/swagger.json",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(Resources.Strings.ConnectCommand_Error_NoRootNoAbsoluteSwagger, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithRootAndNoBase_SetsBaseToRoot()
        {
            string rootAddress = "https://localhost/";
            ArrangeInputs($"connect {rootAddress}",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences,
                          fileContents: "");

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(rootAddress, httpState.BaseAddress?.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_WithJustRoot_SetsBaseAndDefinition()
        {
            string rootAddress = "https://localhost/";
            string swaggerAddress = "https://localhost/swagger.json";
            string swaggerContent = @"{
  ""swagger"": ""2.0"",
  ""paths"": {
    ""/api/Values"": {
      ""post"": {
      }
    }
  }
}";
            ArrangeInputs(commandText: $"connect {rootAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { swaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Equal(rootAddress, httpState.BaseAddress.ToString());
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Equal(swaggerAddress, httpState.SwaggerEndpoint.ToString());
            Assert.NotNull(httpState.ApiDefinition);
        }

        [Fact]
        public async Task ExecuteAsync_WithJustBase_FindsSwaggerSetsDefinition()
        {
            string baseAddress = "https://localhost/";
            string swaggerAddress = "https://localhost/swagger.json";
            string swaggerContent = @"{
  ""swagger"": ""2.0"",
  ""paths"": {
    ""/api/Values"": {
      ""post"": {
      }
    }
  }
}";
            ArrangeInputs(commandText: $"connect --base {baseAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { swaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Equal(baseAddress, httpState.BaseAddress.ToString());
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Equal(swaggerAddress, httpState.SwaggerEndpoint.ToString());
            Assert.NotNull(httpState.ApiDefinition);
        }

        [Fact]
        public async Task ExecuteAsync_WithJustSwagger_SetsBaseAndDefinition()
        {
            string baseAddress = "https://localhost/";
            string swaggerAddress = "https://localhost/swagger.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""servers"": [
    {
      ""url"": """ + baseAddress + @""",
      ""description"": ""First Server Address""
    }
  ],
  ""paths"": {
    ""/pets"": {
    }
  }
}";
            ArrangeInputs(commandText: $"connect --swagger {swaggerAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { swaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Equal(baseAddress, httpState.BaseAddress.ToString());
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Equal(swaggerAddress, httpState.SwaggerEndpoint.ToString());
            Assert.NotNull(httpState.ApiDefinition);
        }

        [Fact]
        public async Task ExecuteAsync_WithBaseAndSwagger_SetsBaseAndDefinitionIgnoresSwaggerBase()
        {
            string baseAddress = "https://localhost/";
            string swaggerAddress = "https://localhost/swagger.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""servers"": [
    {
      ""url"": ""https://example.com/"",
      ""description"": ""First Server Address""
    }
  ],
  ""paths"": {
    ""/pets"": {
    }
  }
}";
            ArrangeInputs(commandText: $"connect --base {baseAddress} --swagger {swaggerAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { swaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Equal(baseAddress, httpState.BaseAddress.ToString());
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Equal(swaggerAddress, httpState.SwaggerEndpoint.ToString());
            Assert.NotNull(httpState.ApiDefinition);
        }

        [Fact]
        public async Task ExecuteAsync_WithRootAndBase_FindsSwaggerFromRoot()
        {
            string rootAddress = "https://localhost/";
            string baseAddress = "https://localhost/v2/";
            string swaggerAddress = "https://localhost/swagger.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""servers"": [
    {
      ""url"": ""https://example.com/"",
      ""description"": ""First Server Address""
    }
  ],
  ""paths"": {
    ""/pets"": {
    }
  }
}";
            ArrangeInputs(commandText: $"connect {rootAddress} --base {baseAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { swaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Equal(baseAddress, httpState.BaseAddress.ToString());
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Equal(swaggerAddress, httpState.SwaggerEndpoint.ToString());
            Assert.NotNull(httpState.ApiDefinition);
        }

        [Fact]
        public async Task ExecuteAsync_WithRootAndBase_FindsSwaggerFromBase()
        {
            string rootAddress = "https://localhost/";
            string baseAddress = "https://localhost/v2/";
            string swaggerAddress = "https://localhost/v2/swagger.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""servers"": [
    {
      ""url"": ""https://example.com/"",
      ""description"": ""First Server Address""
    }
  ],
  ""paths"": {
    ""/pets"": {
    }
  }
}";
            ArrangeInputs(commandText: $"connect {rootAddress} --base {baseAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { swaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Equal(baseAddress, httpState.BaseAddress.ToString());
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Equal(swaggerAddress, httpState.SwaggerEndpoint.ToString());
            Assert.NotNull(httpState.ApiDefinition);
        }

        [Fact]
        public async Task ExecuteAsync_WithRootAndSwaggerWithoutBase_SetsBaseToRoot()
        {
            string rootAddress = "https://localhost/";
            string swaggerAddress = "https://localhost/v2/swagger.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
    ""/pets"": {
    }
  }
}";
            ArrangeInputs(commandText: $"connect {rootAddress} --swagger {swaggerAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { swaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Equal(rootAddress, httpState.BaseAddress.ToString());
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Equal(swaggerAddress, httpState.SwaggerEndpoint.ToString());
            Assert.NotNull(httpState.ApiDefinition);
        }

        [Fact]
        public async Task ExecuteAsync_WithRootAndSwaggerWithBase_SetsBaseToSwaggerBase()
        {
            string rootAddress = "https://localhost/";
            string baseAddress = "https://localhost/v2/";
            string swaggerAddress = "https://localhost/v2/swagger.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""servers"": [
    {
      ""url"": """ + baseAddress + @""",
      ""description"": ""First Server Address""
    }
  ],
  ""paths"": {
    ""/pets"": {
    }
  }
}";
            ArrangeInputs(commandText: $"connect {rootAddress} --swagger {swaggerAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { swaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Equal(baseAddress, httpState.BaseAddress.ToString());
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Equal(swaggerAddress, httpState.SwaggerEndpoint.ToString());
            Assert.NotNull(httpState.ApiDefinition);
        }

        [Fact]
        public async Task ExecuteAsync_SwaggerOnlyWithNoBase_ShowsWarning()
        {
            string swaggerAddress = "https://localhost/v2/swagger.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""paths"": {
    ""/pets"": {
    }
  }
}";
            ArrangeInputs(commandText: $"connect --swagger {swaggerAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { swaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Null(httpState.BaseAddress);
            Assert.Contains(Resources.Strings.ConnectCommand_Status_NoBase, shellState.Output, StringComparer.Ordinal);
        }

        [Fact]
        public async Task ExecuteAsync_RootOnlyNoSwaggerFound_ShowsWarning()
        {
            string rootAddress = "https://localhost/";

            ArrangeInputs(commandText: $"connect {rootAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Null(httpState.SwaggerEndpoint);
            Assert.Contains(Resources.Strings.ConnectCommand_Status_NoSwagger, shellState.Output, StringComparer.Ordinal);
        }

        [Fact]
        public async Task ExecuteAsync_SwaggerOnlyWithBase_ShowsBaseAndSwaggerResult()
        {
            string baseAddress = "https://localhost/v2/";
            string swaggerAddress = "https://localhost/v2/swagger.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""v1""
  },
  ""servers"": [
    {
      ""url"": """ + baseAddress + @""",
      ""description"": ""First Server Address""
    }
  ],
  ""paths"": {
    ""/pets"": {
    }
  }
}";
            ArrangeInputs(commandText: $"connect --swagger {swaggerAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { swaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Contains(string.Format(Resources.Strings.ConnectCommand_Status_Base, httpState.BaseAddress), shellState.Output, StringComparer.Ordinal);
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Contains(string.Format(Resources.Strings.ConnectCommand_Status_Swagger, httpState.SwaggerEndpoint), shellState.Output, StringComparer.Ordinal);
        }

        private void ArrangeInputs(string commandText, out MockedShellState shellState, out HttpState httpState, out ICoreParseResult parseResult, out IPreferences preferences, string fileContents = null)
        {
            ArrangeInputs(commandText,
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: null,
                          out shellState,
                          out httpState,
                          out parseResult,
                          out _,
                          out preferences,
                          readBodyFromFile: fileContents is object,
                          fileContents: fileContents);
        }
    }
}
