// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Tests.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Commands
{
    public class PrefCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_UppercaseGetCommand_CorrectOutput()
        {
            await ValidateOutput(commandText: $"pref GET {WellKnownPreference.ProtocolColor}",
                                 expectedOutputLines: 1,
                                 expectedOutputOnLastLineCallback: (preferences) => $"Configured value: {preferences.CurrentPreferences[WellKnownPreference.ProtocolColor]}");
        }

        [Fact]
        public async Task ExecuteAsync_LowercaseGetCommand_CorrectOutput()
        {
            await ValidateOutput(commandText: $"pref get {WellKnownPreference.ProtocolColor}",
                                 expectedOutputLines: 1,
                                 expectedOutputOnLastLineCallback: (preferences) => $"Configured value: {preferences.CurrentPreferences[WellKnownPreference.ProtocolColor]}");
        }

        [Fact]
        public async Task ExecuteAsync_MixedCaseGetCommand_CorrectOutput()
        {
            await ValidateOutput(commandText: $"pref gEt {WellKnownPreference.ProtocolColor}",
                                 expectedOutputLines: 1,
                                 expectedOutputOnLastLineCallback: (preferences) => $"Configured value: {preferences.CurrentPreferences[WellKnownPreference.ProtocolColor]}");
        }

        [Fact]
        public async Task ExecuteAsync_UppercaseSetCommand_PreferenceSet()
        {
            string expectedValue = "BoldMagenta";
            await ValidatePreference(commandText: $"pref SET {WellKnownPreference.ProtocolColor} {expectedValue}",
                                     preferenceName: WellKnownPreference.ProtocolColor,
                                     expectedValueCallback: (httpState) => expectedValue);
        }

        [Fact]
        public async Task ExecuteAsync_LowercaseSetCommand_PreferenceSet()
        {
            string expectedValue = "BoldMagenta";
            await ValidatePreference(commandText: $"pref set {WellKnownPreference.ProtocolColor} {expectedValue}",
                                     preferenceName: WellKnownPreference.ProtocolColor,
                                     expectedValueCallback: (httpState) => expectedValue);
        }

        [Fact]
        public async Task ExecuteAsync_MixedCaseSetCommand_PreferenceSet()
        {
            string expectedValue = "BoldMagenta";
            await ValidatePreference(commandText: $"pref sEt {WellKnownPreference.ProtocolColor} {expectedValue}",
                                     preferenceName: WellKnownPreference.ProtocolColor,
                                     expectedValueCallback: (httpState) => expectedValue);
        }

        [Fact]
        public async Task ExecuteAsync_SetCommandNoValue_SetToDefault()
        {
            IFileSystem fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            UserFolderPreferences preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, TestDefaultPreferences.GetDefaultPreferences());
            HttpClient httpClient = new HttpClient();
            HttpState httpState = new HttpState(fileSystem, preferences, httpClient);
            MockedShellState shellState = new MockedShellState();
            PrefCommand command = new PrefCommand(preferences);

            // First, set it to something other than the default and make sure that works.
            string firstCommandExpectedValue = "BoldMagenta";
            string firstCommandText = $"pref set {WellKnownPreference.ProtocolColor} {firstCommandExpectedValue}";
            ICoreParseResult firstParseResult = CoreParseResultHelper.Create(firstCommandText);

            await command.ExecuteAsync(shellState, httpState, firstParseResult, CancellationToken.None);

            Assert.Empty(shellState.Output);
            Assert.Equal(firstCommandExpectedValue, preferences.CurrentPreferences[WellKnownPreference.ProtocolColor]);

            // Then, set it to nothing and make sure it goes back to the default
            string secondCommandText = $"pref set {WellKnownPreference.ProtocolColor}";
            ICoreParseResult secondParseResult = CoreParseResultHelper.Create(secondCommandText);

            await command.ExecuteAsync(shellState, httpState, secondParseResult, CancellationToken.None);

            Assert.Empty(shellState.Output);
            Assert.Equal(preferences.DefaultPreferences[WellKnownPreference.ProtocolColor], preferences.CurrentPreferences[WellKnownPreference.ProtocolColor]);

        }

        [Fact]
        public void CanHandle_NoArguments_ReturnsNull()
        {
            string commandText = "";

            Arrange(commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out PrefCommand command, out _);

            bool? canHandle = command.CanHandle(shellState, httpState, parseResult);

            Assert.Null(canHandle);
        }

        [Fact]
        public void CanHandle_InvalidFirstArgument_ReturnsNull()
        {
            string commandText = "preferences set colors.protocol";

            Arrange(commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out PrefCommand command, out _);

            bool? canHandle = command.CanHandle(shellState, httpState, parseResult);

            Assert.Null(canHandle);
        }

        [Fact]
        public void CanHandle_SetWithNoName_DisplaysError()
        {
            string commandText = "pref set";

            Arrange(commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out PrefCommand command, out _);

            bool? canHandle = command.CanHandle(shellState, httpState, parseResult);

            Assert.False(canHandle);
            Assert.Equal(Resources.Strings.PrefCommand_Error_NoPreferenceName, shellState.ErrorMessage);
        }

        [Fact]
        public void CanHandle_SetWithBlankName_DisplaysError()
        {
            string commandText = "pref set  ";

            Arrange(commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out PrefCommand command, out _);

            bool? canHandle = command.CanHandle(shellState, httpState, parseResult);

            Assert.False(canHandle);
            Assert.Equal(Resources.Strings.PrefCommand_Error_NoPreferenceName, shellState.ErrorMessage);
        }

        [Fact]
        public void GetHelpDetails_NoSubCommands_DisplaysCommandSyntax()
        {
            string commandText = "pref";

            Arrange(commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out PrefCommand command, out _);

            string output = command.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Contains("pref [get/set] {setting} [{value}]", output);
        }

        [Fact]
        public void GetHelpDetails_Get_DisplaysCommandSyntax()
        {
            string commandText = "pref get";

            Arrange(commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out PrefCommand command, out _);

            string output = command.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Contains("pref get [{setting}]", output);
        }

        [Fact]
        public void GetHelpDetails_Set_DisplaysCommandSyntax()
        {
            string commandText = "pref set";

            Arrange(commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out PrefCommand command, out _);

            string output = command.GetHelpDetails(shellState, httpState, parseResult);

            Assert.Contains("pref set {setting} [{value}]", output);
        }

        private static async Task ValidatePreference(string commandText, string preferenceName, Func<HttpState, string> expectedValueCallback)
        {
            Arrange(commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out PrefCommand command, out UserFolderPreferences preferences);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Empty(shellState.Output);
            Assert.Equal(expectedValueCallback(httpState), preferences.CurrentPreferences[preferenceName]);
        }

        private static async Task ValidateOutput(string commandText, int expectedOutputLines, Func<UserFolderPreferences, string> expectedOutputOnLastLineCallback)
        {
            Arrange(commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out PrefCommand command, out UserFolderPreferences preferences);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(shellState.Output);
            Assert.Equal(expectedOutputOnLastLineCallback(preferences), shellState.Output[0]);
        }

        private static void Arrange(string commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out PrefCommand command, out UserFolderPreferences preferences)
        {
            IFileSystem fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, TestDefaultPreferences.GetDefaultPreferences());
            HttpClient httpClient = new HttpClient();
            httpState = new HttpState(fileSystem, preferences, httpClient);
            shellState = new MockedShellState();
            parseResult = CoreParseResultHelper.Create(commandText);
            command = new PrefCommand(preferences);
        }

    }
}
