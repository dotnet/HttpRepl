// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Preferences
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

        private void SetupPreferences(out UserFolderPreferences preferences, out MockedFileSystem fileSystem)
        {
            fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider, TestDefaultPreferences.GetDefaultPreferences());
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
