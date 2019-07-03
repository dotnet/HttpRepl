// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Preferences
{
    public class PreferencesTests
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

            Dictionary<string, string> preferencesDictionary = preferences.ReadPreferences(HttpState.CreateDefaultPreferences());

            Assert.Equal(expectedValue, preferencesDictionary[settingName]);
        }

        [Fact]
        public void WritePreferences_SomeDefault_OnlyWritesNonDefaultValues()
        {
            string defaultEditor = "Code.exe";
            string errorColor = "BoldMagenta";
            string expected = $@"{WellKnownPreference.ErrorColor}={errorColor}
{WellKnownPreference.DefaultEditorCommand}={defaultEditor}";

            SetupPreferences(out UserFolderPreferences preferences, out MockedFileSystem fileSystem);

            // Setup the preferences dictionary to have the default preferences, except for one that was modified
            // and one that was added. Only the modified and the added preferences should be written to the file system
            IReadOnlyDictionary<string, string> defaultPreferences = HttpState.CreateDefaultPreferences();
            Dictionary<string, string> preferencesToWrite = new Dictionary<string, string>(defaultPreferences);
            preferencesToWrite[WellKnownPreference.DefaultEditorCommand] = defaultEditor;
            preferencesToWrite[WellKnownPreference.ErrorColor] = errorColor;

            bool succeeded = preferences.WritePreferences(preferencesToWrite, defaultPreferences);

            Assert.True(succeeded);

            Assert.Equal(expected, fileSystem.ReadFile(preferences.PreferencesFilePath));
        }

        [Fact]
        public void WritePreferences_ChangeNonDefaultToDefault_RemovesDefaultValue()
        {
            string originalValue = "BoldMagenta";
            string defaultValue = "Red";
            IReadOnlyDictionary<string, string> defaultPreferences = new Dictionary<string, string> { { WellKnownPreference.ProtocolColor, defaultValue } };

            SetupPreferences(out UserFolderPreferences preferences, out MockedFileSystem fileSystem);

            // Create a file with a non-default value, read it from the file system and
            // validate that it was read correctly
            fileSystem.AddFile(preferences.PreferencesFilePath, $"{WellKnownPreference.ProtocolColor}={originalValue}");
            Dictionary<string, string> preferencesFromFile = preferences.ReadPreferences(defaultPreferences);

            Assert.Equal(originalValue, preferencesFromFile[WellKnownPreference.ProtocolColor]);

            // Now change it to the default value, write it back to the file system and
            // validate that it was removed from the file
            preferencesFromFile[WellKnownPreference.ProtocolColor] = defaultPreferences[WellKnownPreference.ProtocolColor];
            bool succeeded = preferences.WritePreferences(preferencesFromFile, defaultPreferences);

            Assert.True(succeeded);
            Assert.Equal(string.Empty, fileSystem.ReadFile(preferences.PreferencesFilePath));
        }

        private void SetupPreferences(out UserFolderPreferences preferences, out MockedFileSystem fileSystem)
        {
            fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            preferences = new UserFolderPreferences(fileSystem, userProfileDirectoryProvider);
        }

        private void ConfirmAllPreferencesAreDefaults(IPreferences preferences)
        {
            var defaultPreferences = HttpState.CreateDefaultPreferences();
            var currentPreferences = preferences.ReadPreferences(defaultPreferences);
            Assert.Equal(defaultPreferences.Count, currentPreferences.Count);
            foreach (KeyValuePair<string, string> kvp in defaultPreferences)
            {
                Assert.True(currentPreferences.ContainsKey(kvp.Key));
                Assert.Equal(kvp.Value, currentPreferences[kvp.Key]);
            }
        }
    }
}
