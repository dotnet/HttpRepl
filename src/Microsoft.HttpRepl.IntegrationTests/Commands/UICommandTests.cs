// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Resources;
using Microsoft.Repl.Parsing;
using Moq;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class UICommandTests : CommandTestsBase
    {
        [Fact]
        public void CanHandle_WithNoParseResultSections_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            UICommand uiCommand = new UICommand(new UriLauncher());

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithMoreThanOneParseResultSections_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "ui test",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            UICommand uiCommand = new UICommand(new UriLauncher());

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithInvalidName_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "test",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            UICommand uiCommand = new UICommand(new UriLauncher());

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidName_ReturnsTrue()
        {
            ArrangeInputs(parseResultSections: "ui",
                out MockedShellState shellState,
                out HttpState httpState,
                out ICoreParseResult parseResult);

            UICommand uiCommand = new UICommand(new UriLauncher());

            bool? result = uiCommand.CanHandle(shellState, httpState, parseResult);

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsHelpSummary()
        {
            ArrangeInputs(parseResultSections: string.Empty,
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult _);

            UICommand uiCommand = new UICommand(new UriLauncher());

            string result = uiCommand.GetHelpSummary(shellState, httpState);

            Assert.Equal(Strings.UICommand_HelpSummary, result);
        }

        [Fact]
        public void GetHelpDetails_WithInvalidParseResultSection_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "section1",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            UICommand uiCommand = new UICommand(new UriLauncher());

            string result = uiCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithMoreThanOneParseResultSections_ReturnsNull()
        {
            ArrangeInputs(parseResultSections: "ui section1",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            UICommand uiCommand = new UICommand(new UriLauncher());

            string result = uiCommand.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Null(result);
        }

        [Fact]
        public async Task ExecuteAsync_WithHttpStateBaseAddressSetToNull_WritesToConsoleManagerError()
        {
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("ui");
            HttpState httpState = GetHttpState(string.Empty);
            httpState.BaseAddress = null;

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            UICommand uiCommand = new UICommand(mockLauncher.Object);

            await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            VerifyErrorMessageWasWrittenToConsoleManagerError(shellState);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidHttpStateBaseAddress_VerifyLaunchUriAsyncWasCalledOnce()
        {
            ArrangeInputs(parseResultSections: "ui",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            Uri uri = new Uri("https://localhost:44366/");
            httpState.BaseAddress = uri;

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            UICommand uiCommand = new UICommand(mockLauncher.Object);

            mockLauncher.Setup(s => s.LaunchUriAsync(It.IsAny<Uri>()))
                .Returns(Task.CompletedTask);

            await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            mockLauncher.Verify(l => l.LaunchUriAsync(It.Is<Uri>(u => u.AbsoluteUri == "https://localhost:44366/swagger")), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_WithLaunchUriFailure_ThrowsException()
        {
            ArrangeInputs(parseResultSections: "ui",
                 out MockedShellState shellState,
                 out HttpState httpState,
                 out ICoreParseResult parseResult);

            Uri uri = new Uri("https://localhost:44366/");
            httpState.BaseAddress = uri;

            Mock<IUriLauncher> mockLauncher = new Mock<IUriLauncher>();
            string expectedErrorMessage = "Unable to launch https://localhost:44366/swagger";
            mockLauncher.Setup(s => s.LaunchUriAsync(It.IsAny<Uri>()))
                .Returns(Task.FromException(new Exception(expectedErrorMessage)));

            UICommand uiCommand = new UICommand(mockLauncher.Object);
            var exception = await Record.ExceptionAsync(async () => await uiCommand.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None));

            Assert.NotNull(exception);
            Assert.Equal(expectedErrorMessage, exception.Message);
        }
    }
}
