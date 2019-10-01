// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl.ConsoleHandling;
using Xunit;

namespace Microsoft.HttpRepl.Tests.Preferences
{
    public class UserFolderPreferencesTests
    {
        [Fact]
        public void ReadPreferences_NoPreferencesFile_AllDefaults()
        {
            SetupPreferences(out UserFolderPreferences preferences, out _);

            ConfirmAllPreferencesAreDefaults(preferences);
        }

        [Fact]
        public void ReadPreferences_BlankPreferencesFile_AllDefaults()
        {
            SetupPreferences(out UserFolderPreferences preferences, out MockedFileSystem fileSystem);
            fileSystem.AddFile(preferences.PreferencesFilePath, "");

            ConfirmAllPreferencesAreDefaults(preferences);
        }

        [Fact]
        public void ReadPreferences_InvalidPreferencesFile_AllDefaults()
        {
            SetupPreferences(out UserFolderPreferences preferences, out MockedFileSystem fileSystem);
            fileSystem.AddFile(preferences.PreferencesFilePath, "This is not a valid preferences file.");

            ConfirmAllPreferencesAreDefaults(preferences);
        }

        [Fact]
        public void ReadPreferences_PartiallyInvalidPreferencesFile_ValidPrefsAreSet()
        {
            SetupPreferences(out UserFolderPreferences preferences, out MockedFileSystem fileSystem);
            string settingName = WellKnownPreference.DefaultEditorCommand;
            string expectedValue = "Code.exe";
            string prefsFileContent = $@"This first line is invalid for a prefs file.
{settingName}={expectedValue}
This third line is invalid as well";
            fileSystem.AddFile(preferences.PreferencesFilePath, prefsFileContent);

            Assert.Equal(expectedValue, preferences.CurrentPreferences[settingName]);
        }

        [Fact]
        public void WritePreferences_SomeDefault_OnlyWritesNonDefaultValues()
        {
            string defaultEditor = "Code.exe";
            string errorColor = "BoldMagenta";
            string expected = $@"{WellKnownPreference.ErrorColor}={errorColor}
{WellKnownPreference.DefaultEditorCommand}={defaultEditor}";

            SetupPreferences(out UserFolderPreferences preferences, out MockedFileSystem fileSystem);

            // Add one and change one
            // Only the changes should be written, not any of the defaults.
            bool succeeded = preferences.SetValue(WellKnownPreference.DefaultEditorCommand, defaultEditor);

            Assert.True(succeeded);

            succeeded = preferences.SetValue(WellKnownPreference.ErrorColor, errorColor);

            Assert.True(succeeded);
            Assert.Equal(expected, fileSystem.ReadFile(preferences.PreferencesFilePath));
        }

        [Fact]
        public void WritePreferences_ChangeNonDefaultToDefault_RemovesDefaultValue()
        {
            string originalValue = "BoldMagenta";
            string defaultValue = "Red";
            IDictionary<string, string> defaultPreferences = new Dictionary<string, string> { { WellKnownPreference.ProtocolColor, defaultValue } };

            MockedFileSystem fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            UserFolderPreferences preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, defaultPreferences);           

            // Create a file with a non-default value, read it from the file system and
            // validate that it was read correctly
            fileSystem.AddFile(preferences.PreferencesFilePath, $"{WellKnownPreference.ProtocolColor}={originalValue}");

            Assert.Equal(originalValue, preferences.CurrentPreferences[WellKnownPreference.ProtocolColor]);

            // Now change it to the default value, write it back to the file system and
            // validate that it was removed from the file
            bool succeeded = preferences.SetValue(WellKnownPreference.ProtocolColor, defaultPreferences[WellKnownPreference.ProtocolColor]);

            Assert.True(succeeded);
            Assert.Equal(string.Empty, fileSystem.ReadFile(preferences.PreferencesFilePath));
        }

        [Fact]
        public void SetValue_NoValueNoDefault_PreferenceIsRemoved()
        {
            string initialValue = "BoldRed";
            SetupPreferencesWithFileContent($"{WellKnownPreference.JsonBraceColor}={initialValue}", out UserFolderPreferences preferences);
            
            Assert.Equal("BoldRed", preferences.GetValue(WellKnownPreference.JsonBraceColor));

            // JsonBraceColor has no default, so this should remove the preference
            preferences.SetValue(WellKnownPreference.JsonBraceColor, "");

            bool found = preferences.TryGetValue(WellKnownPreference.JsonBraceColor, out _);

            Assert.False(found);
        }

        [Theory]
        [MemberData(nameof(GetStringValuesTestData))]
        public void GetValue_CorrectOutput(string expected, string fileContent, string preferenceName, string defaultValue)
        {
            SetupPreferencesWithFileContent(fileContent, out UserFolderPreferences preferences);

            string result = preferences.GetValue(preferenceName, defaultValue);

            Assert.Equal(expected, result, StringComparer.OrdinalIgnoreCase);
        }

        [Theory]
        [MemberData(nameof(GetColorValuesTestData))]
        public void GetColorValue_CorrectOutput(AllowedColors expected, string fileContent, string preferenceName, AllowedColors defaultValue)
        {
            SetupPreferencesWithFileContent(fileContent, out UserFolderPreferences preferences);

            AllowedColors result = preferences.GetColorValue(preferenceName, defaultValue);

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetIntValuesTestData))]
        public void GetIntValue_CorrectOutput(int expected, string fileContent, string preferenceName, int defaultValue)
        {
            SetupPreferencesWithFileContent(fileContent, out UserFolderPreferences preferences);

            int result = preferences.GetIntValue(preferenceName, defaultValue);

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(GetBoolValuesTestData))]
        public void GetBoolValue_CorrectOutput(bool expected, string fileContent, string preferenceName, bool defaultValue)
        {
            SetupPreferencesWithFileContent(fileContent, out UserFolderPreferences preferences);

            bool result = preferences.GetBoolValue(preferenceName, defaultValue);

            Assert.Equal(expected, result);
        }

        public static IEnumerable<object[]> GetStringValuesTestData()
        {
            // Empty/blank preferences file falls back to the passed in default.
            yield return new object[] { "code.exe", string.Empty, WellKnownPreference.DefaultEditorCommand, "code.exe" };
            // Actual value is returned
            yield return new object[] { "code.exe", $"{WellKnownPreference.DefaultEditorCommand}=code.exe", WellKnownPreference.DefaultEditorCommand, "notepad.exe" };
        }

        public static IEnumerable<object[]> GetColorValuesTestData()
        {
            // Empty/blank preferences file falls back to the passed in default
            yield return new object[] { AllowedColors.Magenta, string.Empty, WellKnownPreference.ErrorColor, AllowedColors.Magenta };
            // Preference value that isn't an actual color falls back to the passed in default
            yield return new object[] { AllowedColors.Magenta, $"{WellKnownPreference.ErrorColor}=ThisIsGibberish", WellKnownPreference.ErrorColor, AllowedColors.Magenta };
            // Actual value is returned
            yield return new object[] { AllowedColors.BoldRed, $"{WellKnownPreference.ErrorColor}=BoldRed", WellKnownPreference.ErrorColor, AllowedColors.Cyan };
        }

        public static IEnumerable<object[]> GetIntValuesTestData()
        {
            // Empty/blank preferences file falls back to the passed in default
            yield return new object[] { 5, string.Empty, WellKnownPreference.JsonIndentSize, 5 };
            // Preference value that isn't an int falls back to the passed in default
            yield return new object[] { 5, $"{WellKnownPreference.JsonIndentSize}=ThisIsGibberish", WellKnownPreference.JsonIndentSize, 5 };
            // Actual value is returned
            yield return new object[] { 5, $"{WellKnownPreference.JsonIndentSize}=5", WellKnownPreference.JsonIndentSize, 42 };
        }

        public static IEnumerable<object[]> GetBoolValuesTestData()
        {
            // Empty/blank preferences file falls back to the passed in default
            yield return new object[] { false, string.Empty, WellKnownPreference.UseDefaultCredentials, false };
            // Preference value that isn't a bool falls back to the passed in default
            yield return new object[] { false, $"{WellKnownPreference.UseDefaultCredentials}=ThisIsGibberish", WellKnownPreference.UseDefaultCredentials, false };
            // Actual value is returned
            yield return new object[] { true, $"{WellKnownPreference.UseDefaultCredentials}=true", WellKnownPreference.UseDefaultCredentials, false };
        }

        private void SetupPreferences(out UserFolderPreferences preferences, out MockedFileSystem fileSystem)
        {
            fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, TestDefaultPreferences.GetDefaultPreferences());
        }

        private void SetupPreferencesWithFileContent(string fileContent, out UserFolderPreferences preferences)
        {
            SetupPreferences(out preferences, out MockedFileSystem fileSystem);

            fileSystem.AddFile(preferences.PreferencesFilePath, fileContent);
        }

        private void ConfirmAllPreferencesAreDefaults(IPreferences preferences)
        {
            var defaultPreferences = TestDefaultPreferences.GetDefaultPreferences();
            var currentPreferences = preferences.CurrentPreferences;
            Assert.Equal(defaultPreferences.Count, currentPreferences.Count);
            foreach (KeyValuePair<string, string> kvp in defaultPreferences)
            {
                Assert.True(currentPreferences.ContainsKey(kvp.Key));
                Assert.Equal(kvp.Value, currentPreferences[kvp.Key]);
            }
        }
    }
}
