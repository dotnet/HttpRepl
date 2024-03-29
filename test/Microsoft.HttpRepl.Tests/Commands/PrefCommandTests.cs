// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Telemetry;
using Microsoft.HttpRepl.Tests.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl.ConsoleHandling;
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
            HttpState httpState = new HttpState(preferences, httpClient);
            MockedShellState shellState = new MockedShellState();
            PrefCommand command = new PrefCommand(preferences, new NullTelemetry());

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

        [Fact]
        public async Task ExecuteAsync_WithGet_SendsTelemetry()
        {
            Arrange($"pref get {WellKnownPreference.DefaultEditorCommand}",
                    out HttpState httpState,
                    out MockedShellState shellState,
                    out ICoreParseResult parseResult,
                    out UserFolderPreferences preferences);

            TelemetryCollector telemetry = new TelemetryCollector();

            PrefCommand command = new PrefCommand(preferences, telemetry);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(telemetry.Telemetry);
            TelemetryCollector.CollectedTelemetry collectedTelemetry = telemetry.Telemetry[0];
            Assert.Equal("Preference", collectedTelemetry.EventName);
            Assert.Equal("Get", collectedTelemetry.Properties["GetOrSet"]);
            Assert.Equal(WellKnownPreference.DefaultEditorCommand, collectedTelemetry.Properties["PreferenceName"]);
        }

        [Fact]
        public async Task ExecuteAsync_WithSet_SendsTelemetry()
        {
            Arrange($"pref set {WellKnownPreference.DefaultEditorCommand} value",
                    out HttpState httpState,
                    out MockedShellState shellState,
                    out ICoreParseResult parseResult,
                    out UserFolderPreferences preferences);

            TelemetryCollector telemetry = new TelemetryCollector();

            PrefCommand command = new PrefCommand(preferences, telemetry);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(telemetry.Telemetry);
            TelemetryCollector.CollectedTelemetry collectedTelemetry = telemetry.Telemetry[0];
            Assert.Equal("Preference", collectedTelemetry.EventName);
            Assert.Equal("Set", collectedTelemetry.Properties["GetOrSet"]);
            Assert.Equal(WellKnownPreference.DefaultEditorCommand, collectedTelemetry.Properties["PreferenceName"]);
        }

        [Fact]
        public async Task ExecuteAsync_WithGetAndUnknownName_SendsTelemetryWithHashedName()
        {
            Arrange("pref set preferenceName value",
                    out HttpState httpState,
                    out MockedShellState shellState,
                    out ICoreParseResult parseResult,
                    out UserFolderPreferences preferences);

            TelemetryCollector telemetry = new TelemetryCollector();

            PrefCommand command = new PrefCommand(preferences, telemetry);

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(telemetry.Telemetry);
            TelemetryCollector.CollectedTelemetry collectedTelemetry = telemetry.Telemetry[0];
            Assert.Equal("Preference", collectedTelemetry.EventName);
            Assert.Equal("Set", collectedTelemetry.Properties["GetOrSet"]);
            Assert.Equal(Sha256Hasher.Hash("preferenceName"), collectedTelemetry.Properties["PreferenceName"]);
        }

        [Theory]
        [MemberData(nameof(ExecuteAsync_SetDefaultEditorToVSCode_ShowsWarning_Data))]
        public async Task ExecuteAsync_SetDefaultEditorToVSCode_ShowsWarning(string commandText, OSPlatform intendedPlatform)
        {
            // Arrange
            Arrange($"pref set {WellKnownPreference.DefaultEditorCommand} \"{commandText}\"",
                    out HttpState httpState,
                    out MockedShellState shellState,
                    out ICoreParseResult parseResult,
                    out UserFolderPreferences preferences);

            PrefCommand command = new PrefCommand(preferences, new NullTelemetry());

            string expectedWarning = string.Format(Resources.Strings.PrefCommand_Set_VSCode, WellKnownPreference.DefaultEditorArguments).SetColor(httpState.WarningColor);

            // Act
            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            // Assert
            if (RuntimeInformation.IsOSPlatform(intendedPlatform))
            {
                Assert.Contains(expectedWarning, shellState.Output, StringComparer.CurrentCulture);
            }
            else
            {
                Assert.DoesNotContain(expectedWarning, shellState.Output, StringComparer.CurrentCulture);
            }
        }

        public static IEnumerable<object[]> ExecuteAsync_SetDefaultEditorToVSCode_ShowsWarning_Data { get; } = new List<object[]>()
        {
            new object[] { "c:\\users\\username\\appdata\\local\\programs\\Microsoft VS Code\\Code.exe", OSPlatform.Windows },
            new object[] { "c:\\users\\username\\appdata\\local\\programs\\Microsoft VS Code Insiders\\Code - Insiders.exe", OSPlatform.Windows },
            new object[] { "C:\\Program Files\\Microsoft VS Code\\Code.exe", OSPlatform.Windows },
            new object[] { "/usr/bin/code", OSPlatform.Linux },
            new object[] { "/usr/bin/code-insiders", OSPlatform.Linux },
            new object[] { "/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code", OSPlatform.OSX },
            new object[] { "/Applications/Visual Studio Code - Insiders.app/Contents/Resources/app/bin/code-insiders", OSPlatform.OSX },
        };

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
            httpState = new HttpState(preferences, httpClient);
            shellState = new MockedShellState();
            parseResult = CoreParseResultHelper.Create(commandText);
            command = new PrefCommand(preferences, new NullTelemetry());
        }

        private static void Arrange(string commandText, out HttpState httpState, out MockedShellState shellState, out ICoreParseResult parseResult, out UserFolderPreferences preferences)
        {
            Arrange(commandText, out httpState, out shellState, out parseResult, out _, out preferences);
        }
    }
}
