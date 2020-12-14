// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(Resources.Strings.ConnectCommand_Error_NoRootNoAbsoluteSwagger, shellState.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteAsync_WithNoRootAndRelativeOpenApi_ShowsError()
        {
            ArrangeInputs("connect --openapi /v1/openapi.json",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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
  ""info"": {
    ""title"": ""OpenAPI v2 Spec"",
    ""version"": ""v1""
  },
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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
  ""info"": {
    ""title"": ""OpenAPI v2 Spec"",
    ""version"": ""v1""
  },
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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
    ""title"": ""OpenAPI v3 Spec"",
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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
    ""title"": ""OpenAPI v3 Spec"",
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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
    ""title"": ""OpenAPI v3 Spec"",
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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
    ""title"": ""OpenAPI v3 Spec"",
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Equal(baseAddress, httpState.BaseAddress.ToString());
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Equal(swaggerAddress, httpState.SwaggerEndpoint.ToString());
            Assert.NotNull(httpState.ApiDefinition);
        }

        [Fact]
        public async Task ExecuteAsync_WithRootAndBase_FindsOpenApiFromRoot()
        {
            string rootAddress = "https://localhost/";
            string baseAddress = "https://localhost/v2/";
            string swaggerAddress = "https://localhost/openapi.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""OpenAPI v3 Spec"",
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Equal(baseAddress, httpState.BaseAddress.ToString());
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Equal(swaggerAddress, httpState.SwaggerEndpoint.ToString());
            Assert.NotNull(httpState.ApiDefinition);
        }

        [Fact]
        public async Task ExecuteAsync_WithRootAndBase_FindsOpenApiFromBase()
        {
            string rootAddress = "https://localhost/";
            string baseAddress = "https://localhost/v2/";
            string swaggerAddress = "https://localhost/v2/openapi.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""OpenAPI v3 Spec"",
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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
    ""title"": ""OpenAPI v3 Spec"",
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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
    ""title"": ""OpenAPI v3 Spec"",
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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
    ""title"": ""OpenAPI v3 Spec"",
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

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
    ""title"": ""OpenAPI v3 Spec"",
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

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.NotNull(httpState.BaseAddress);
            Assert.Contains(string.Format(Resources.Strings.ConnectCommand_Status_Base, httpState.BaseAddress), shellState.Output, StringComparer.Ordinal);
            Assert.NotNull(httpState.SwaggerEndpoint);
            Assert.Contains(string.Format(Resources.Strings.ConnectCommand_Status_Swagger, httpState.SwaggerEndpoint), shellState.Output, StringComparer.Ordinal);
        }

        [Fact]
        public async Task ExecuteAsync_RootOnlyWithVerbose_OutputContainsAttempts()
        {
            string rootAddress = "https://localhost/v2";

            ArrangeInputs(commandText: $"connect {rootAddress} --verbose",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>(),
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Contains(Resources.Strings.ApiConnection_Logging_Parsing + Resources.Strings.ApiConnection_Logging_Failed, shellState.Output, StringComparer.Ordinal);
        }

        [Fact]
        public async Task ExecuteAsync_RootOnlyWithoutVerbose_OutputDoesNotContainAttempts()
        {
            string rootAddress = "https://localhost/v2";

            ArrangeInputs(commandText: $"connect {rootAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>(),
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.DoesNotContain(Resources.Strings.ApiConnection_Logging_Parsing + Resources.Strings.ApiConnection_Logging_Failed, shellState.Output, StringComparer.Ordinal);
        }

        [Fact]
        public async Task ExecuteAsync_SwaggerOnlyWithoutVerbose_OutputContainsAttempts()
        {
            string openApiDescriptionUrl = "https://localhost/v2/swagger.json";

            ArrangeInputs(commandText: $"connect --openapi {openApiDescriptionUrl}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>(),
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Contains(Resources.Strings.ApiConnection_Logging_Parsing + Resources.Strings.ApiConnection_Logging_Failed, shellState.Output, StringComparer.Ordinal);
        }

        [Fact]
        public async Task ExecuteAsync_RootWithSwaggerSuffix_FixesBase()
        {
            string rootAddress = "https://localhost:44368/swagger";
            string expectedBaseAddress = "https://localhost:44368/";
            string expectedSwaggerAddress = "https://localhost:44368/swagger/v1/swagger.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""OpenAPI v3 Spec"",
    ""version"": ""v1""
  },
  ""paths"": {
    ""/WeatherForecast"": {
    }
  }
}";

            ArrangeInputs(commandText: $"connect {rootAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { expectedSwaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Contains(string.Format(Resources.Strings.ConnectCommand_Status_Base, expectedBaseAddress), shellState.Output, StringComparer.Ordinal);
            Assert.Contains(string.Format(Resources.Strings.ConnectCommand_Status_Swagger, expectedSwaggerAddress), shellState.Output, StringComparer.Ordinal);
        }

        [Fact]
        public async Task ExecuteAsync_RootWithSwaggerSuffixAndOverride_DoesNotFixBase()
        {
            string rootAddress = "https://localhost:44368/swagger";
            string expectedBaseAddress = rootAddress + "/";
            string expectedSwaggerAddress = "https://localhost:44368/swagger/v1/swagger.json";
            string swaggerContent = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""OpenAPI v3 Spec"",
    ""version"": ""v1""
  },
  ""paths"": {
    ""/WeatherForecast"": {
    }
  }
}";

            MockedFileSystem fileSystem = new MockedFileSystem();
            UserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            IPreferences preferences = new UserFolderPreferences(fileSystem,
                                                                 userProfileDirectoryProvider,
                                                                 new Dictionary<string, string> { { WellKnownPreference.ConnectCommandSkipRootFix, "true"}});

            ArrangeInputsWithOptional(commandText: $"connect {rootAddress}",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: new Dictionary<string, string>() { { expectedSwaggerAddress, swaggerContent } },
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          ref fileSystem,
                          ref preferences);

            ConnectCommand connectCommand = new ConnectCommand(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Contains(string.Format(Resources.Strings.ConnectCommand_Status_Base, expectedBaseAddress), shellState.Output, StringComparer.Ordinal);
            Assert.Contains(string.Format(Resources.Strings.ConnectCommand_Status_Swagger, expectedSwaggerAddress), shellState.Output, StringComparer.Ordinal);
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyRoot_SendsTelemetry()
        {
            string rootAddress = "https://localhost/";
            ArrangeInputs($"connect {rootAddress}",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences,
                          fileContents: "");

            TelemetryCollector telemetry = new TelemetryCollector();

            ConnectCommand connectCommand = new ConnectCommand(preferences, telemetry);

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(telemetry.Telemetry);
            TelemetryCollector.CollectedTelemetry collectedTelemetry = telemetry.Telemetry[0];
            Assert.Equal("connect", collectedTelemetry.EventName, ignoreCase: true);
            Assert.Equal("True", collectedTelemetry.Properties["RootSpecified"]);
            Assert.Equal("False", collectedTelemetry.Properties["BaseSpecified"]);
            Assert.Equal("False", collectedTelemetry.Properties["OpenApiSpecified"]);
            Assert.Equal("False", collectedTelemetry.Properties["OpenApiFound"]);
        }

        [Fact]
        public async Task ExecuteAsync_WithoutPersistHeaders_DoesNotPersistNonDefaultHeaders()
        {
            string rootAddress = "https://localhost/";
            ArrangeInputs($"connect {rootAddress}",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences,
                          fileContents: "");

            httpState.Headers["TestHeaderName"] = new[] { "TestHeaderValue" };

            ConnectCommand connectCommand = new(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(httpState.Headers);
            Assert.True(httpState.Headers.ContainsKey("User-Agent"));
        }

        [Fact]
        public async Task ExecuteAsync_WithoutPersistPaths_DoesNotPersistPaths()
        {
            string rootAddress = "https://localhost/";
            ArrangeInputs($"connect {rootAddress}",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences,
                          fileContents: "");

            httpState.PathSections.Push("dir1");

            ConnectCommand connectCommand = new(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Empty(httpState.PathSections);
        }

        [Fact]
        public async Task ExecuteAsync_WithPersistHeaders_PersistsHeaders()
        {
            string expectedHeaderName = "TestHeaderName";
            string expectedHeaderValue = "TestHeaderValue";
            string rootAddress = "https://localhost/";
            ArrangeInputs($"connect {rootAddress} --persist-headers",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences,
                          fileContents: "");

            httpState.Headers[expectedHeaderName] = new[] { expectedHeaderValue };

            ConnectCommand connectCommand = new(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.True(httpState.Headers.ContainsKey(expectedHeaderName));
            Assert.Equal(expectedHeaderValue, httpState.Headers[expectedHeaderName].Single());
        }

        [Fact]
        public async Task ExecuteAsync_WithPersistPaths_PersistsPaths()
        {
            string expectedPathSection = "dir1";
            string rootAddress = "https://localhost/";
            ArrangeInputs($"connect {rootAddress} --persist-paths",
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out IPreferences preferences,
                          fileContents: "");

            httpState.PathSections.Push(expectedPathSection);

            ConnectCommand connectCommand = new(preferences, new NullTelemetry());

            await connectCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(httpState.PathSections);
            Assert.Equal(httpState.PathSections.Peek(), expectedPathSection);
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
