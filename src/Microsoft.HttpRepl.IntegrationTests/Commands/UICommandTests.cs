// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
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
    public class UICommandTests : CommandHelper<UICommand>
    {
        public UICommandTests()
            : base(new UICommand(new UriLauncher()))
        {
        }

        [Fact]
        public void CanHandle_WithNoParseResultSections_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: string.Empty);

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithMoreThanOneParseResultSections_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: "ui test");

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithInvalidName_ReturnsNull()
        {
            bool? result = CanHandle(parseResultSections: "test");

            Assert.Null(result);
        }

        [Fact]
        public void CanHandle_WithValidName_ReturnsTrue()
        {
            bool? result = CanHandle(parseResultSections: "ui");

            Assert.True(result);
        }

        [Fact]
        public void GetHelpSummary_ReturnsDescription()
        {
            string result = GetHelpSummary();

            Assert.Equal(Strings.UICommand_Description, result);
        }

        [Fact]
        public void GetHelpDetails_WithInvalidParseResultSection_ReturnsNull()
        {
            string result = GetHelpDetails(parseResultSections: "section1");

            Assert.Null(result);
        }

        [Fact]
        public void GetHelpDetails_WithMoreThanOneParseResultSections_ReturnsNull()
        {
            string result = GetHelpDetails(parseResultSections: "ui section2");

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
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("ui");
            HttpState httpState = GetHttpState(string.Empty);
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
            MockedShellState shellState = new MockedShellState();
            ICoreParseResult parseResult = CoreParseResultHelper.Create("ui");
            HttpState httpState = GetHttpState(string.Empty);
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
