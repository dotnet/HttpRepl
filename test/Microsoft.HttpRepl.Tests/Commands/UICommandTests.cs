// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Resources;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl.Parsing;
using Moq;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class UICommandTests : CommandTestsBase
    {
        [Fact]
        public void CanHandle_WithNoParseResultSections_ReturnsNull()
        {
            ArrangeInputs(commandText: string.Empty,
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            UICommand uiCommand = new UICommand(new UriLauncher(), preferences);

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithMoreThanOneParseResultSections_ReturnsTrue()
        {
            ArrangeInputs(commandText: "ui test",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            UICommand uiCommand = new UICommand(new UriLauncher(), preferences);

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void CanHandle_WithInvalidName_ReturnsNull()
        {
            ArrangeInputs(commandText: "test",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            UICommand uiCommand = new UICommand(new UriLauncher(), preferences);

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidName_ReturnsTrue()
        {
            ArrangeInputs(commandText: "ui",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            UICommand uiCommand = new UICommand(new UriLauncher(), preferences);

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsHelpSummary()
        {
            ArrangeInputs(commandText: string.Empty,
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out _,
                          out _,
                          out IPreferences preferences);

            UICommand uiCommand = new UICommand(new UriLauncher(), preferences);

            string result = uiCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(Strings.UICommand_HelpSummary, result);
        }

        [Fact]
        public void GetHelpDetails_WithInvalidParseResultSection_ReturnsNull()
        {
            ArrangeInputs(commandText: "section1",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            UICommand uiCommand = new UICommand(new UriLauncher(), preferences);

            string result = uiCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public async Task ExecuteAsync_WithHttpStateBaseAddressSetToNull_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("ui");
            HttpState httpState = GetHttpState(out _, out IPreferences preferences);
            httpState.BaseAddress = null;

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            UICommand uiCommand = new UICommand(mockLauncher.Object, preferences);

            await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidHttpStateBaseAddress_VerifyLaunchUriAsyncWasCalledOnce()
        {
            ArrangeInputs(commandText: "ui",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            Uri uri = new Uri("https://localhost:44366/");
            httpState.BaseAddress = uri;

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            UICommand uiCommand = new UICommand(mockLauncher.Object, preferences);

            mockLauncher.Setup(s => s.LaunchUriAsync(It.IsAny<Uri>()))
                .Returns(Task.CompletedTask);

            await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            mockLauncher.Verify(l => l.LaunchUriAsync(It.Is<Uri>(u => u.AbsoluteUri == "https://localhost:44366/swagger")), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_WithLaunchUriFailure_ThrowsException()
        {
            ArrangeInputs(commandText: "ui",
                          baseAddress: null,
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            Uri uri = new Uri("https://localhost:44366/");
            httpState.BaseAddress = uri;

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            string expectedErrorMessage = "Unable to launch https://localhost:44366/swagger";
            mockLauncher.Setup(s => s.LaunchUriAsync(It.IsAny<Uri>()))
                .Returns(Task.FromException(new Exception(expectedErrorMessage)));

            UICommand uiCommand = new UICommand(mockLauncher.Object, preferences);
            var exception = await Record.ExceptionAsync(async () => await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None));

            Assert.NotNull(exception);
            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        [Fact]
        public async Task ExecuteAsync_WithRelativeParameter_VerifyLaunchUriAsyncWasCalledOnce()
        {
            ArrangeInputs(commandText: "ui /mySwaggerPath",
                          baseAddress: "https://localhost:44366/",
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            UICommand uiCommand = new UICommand(mockLauncher.Object, preferences);

            mockLauncher.Setup(s => s.LaunchUriAsync(It.IsAny<Uri>()))
                        .Returns(Task.CompletedTask);

            await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            mockLauncher.Verify(l => l.LaunchUriAsync(It.Is<Uri>(u => u.AbsoluteUri == "https://localhost:44366/mySwaggerPath")), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_WithAbsoluteParameter_VerifyLaunchUriAsyncWasCalledOnce()
        {
            ArrangeInputs(commandText: "ui https://localhost:12345/mySwaggerPath",
                          baseAddress: "https://localhost:44366/",
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            UICommand uiCommand = new UICommand(mockLauncher.Object, preferences);

            mockLauncher.Setup(s => s.LaunchUriAsync(It.IsAny<Uri>()))
                        .Returns(Task.CompletedTask);

            await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            mockLauncher.Verify(l => l.LaunchUriAsync(It.Is<Uri>(u => u.AbsoluteUri == "https://localhost:12345/mySwaggerPath")), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_WithNoParameterAndNoPreference_VerifyLaunchUriAsyncWasCalledOnce()
        {
            ArrangeInputs(commandText: "ui",
                          baseAddress: "https://localhost:44366/",
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            UICommand uiCommand = new UICommand(mockLauncher.Object, preferences);

            mockLauncher.Setup(s => s.LaunchUriAsync(It.IsAny<Uri>()))
                        .Returns(Task.CompletedTask);

            await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            mockLauncher.Verify(l => l.LaunchUriAsync(It.Is<Uri>(u => u.AbsoluteUri == "https://localhost:44366/swagger")), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_WithNoParameterAndRelativePreference_VerifyLaunchUriAsyncWasCalledOnce()
        {
            string commandText = "ui";
            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);
            MockedShellState shellState = new MockedShellState();
            MockedFileSystem fileSystem = new MockedFileSystem();
            UserFolderPreferences preferences = new UserFolderPreferences(fileSystem, new UserProfileDirectoryProvider(), null);
            preferences.SetValue(WellKnownPreference.SwaggerUIEndpoint, "/mySwaggerPath");
            HttpState httpState = new HttpState(fileSystem, preferences, new HttpClient());
            httpState.BaseAddress = new Uri("https://localhost:44366", UriKind.Absolute);

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            UICommand uiCommand = new UICommand(mockLauncher.Object, preferences);

            mockLauncher.Setup(s => s.LaunchUriAsync(It.IsAny<Uri>()))
                        .Returns(Task.CompletedTask);

            await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            mockLauncher.Verify(l => l.LaunchUriAsync(It.Is<Uri>(u => u.AbsoluteUri == "https://localhost:44366/mySwaggerPath")), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_WithNoParameterAndAbsolutePreference_VerifyLaunchUriAsyncWasCalledOnce()
        {
            string commandText = "ui";
            ICoreParseResult parseResult = CoreParseResultHelper.Create(commandText);
            MockedShellState shellState = new MockedShellState();
            MockedFileSystem fileSystem = new MockedFileSystem();
            UserFolderPreferences preferences = new UserFolderPreferences(fileSystem, new UserProfileDirectoryProvider(), null);
            preferences.SetValue(WellKnownPreference.SwaggerUIEndpoint, "https://localhost:12345/mySwaggerPath");
            HttpState httpState = new HttpState(fileSystem, preferences, new HttpClient());
            httpState.BaseAddress = new Uri("https://localhost:44366", UriKind.Absolute);

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            UICommand uiCommand = new UICommand(mockLauncher.Object, preferences);

            mockLauncher.Setup(s => s.LaunchUriAsync(It.IsAny<Uri>()))
                        .Returns(Task.CompletedTask);

            await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            mockLauncher.Verify(l => l.LaunchUriAsync(It.Is<Uri>(u => u.AbsoluteUri == "https://localhost:12345/mySwaggerPath")), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidParameterAndNoPreference_DisplaysError()
        {
            string invalidParameter = "https:///localhost/swagger";
            string expectedError = string.Format(Strings.UICommand_InvalidParameter, invalidParameter);
            ArrangeInputs(commandText: $"ui {invalidParameter}",
                          baseAddress: "https://localhost:44366/",
                          path: null,
                          urlsWithResponse: null,
                          out MockedShellState shellState,
                          out HttpState httpState,
                          out ICoreParseResult parseResult,
                          out _,
                          out IPreferences preferences);

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            UICommand uiCommand = new UICommand(mockLauncher.Object, preferences);

            await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Equal(expectedError, shellState.ErrorMessage, StringComparer.Ordinal);
        }
    }
}
