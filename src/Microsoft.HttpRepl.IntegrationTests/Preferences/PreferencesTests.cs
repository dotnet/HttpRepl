using System.Collections.Generic;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Xunit;
using Prefs = Microsoft.HttpRepl.Preferences.Preferences;

namespace Microsoft.HttpRepl.IntegrationTests.Preferences
{
    public class PreferencesTests
    {
        [Fact]
        public void ReadPreferences_NoPreferencesFile_AllDefaults()
        {
            SetupPreferences(out Prefs preferences, out _);

            ConfirmAllPreferencesAreDefaults(preferences);
        }

        [Fact]
        public void ReadPreferences_BlankPreferencesFile_AllDefaults()
        {
            SetupPreferences(out Prefs preferences, out MockedFileSystem fileSystem);
            fileSystem.AddFile(preferences.PreferencesFilePath, "");

            ConfirmAllPreferencesAreDefaults(preferences);
        }

        [Fact]
        public void ReadPreferences_InvalidPreferencesFile_AllDefaults()
        {
            SetupPreferences(out Prefs preferences, out MockedFileSystem fileSystem);
            fileSystem.AddFile(preferences.PreferencesFilePath, "This is not a valid preferences file.");

            ConfirmAllPreferencesAreDefaults(preferences);
        }

        [Fact]
        public void ReadPreferences_PartiallyInvalidPreferencesFile_ValidPrefsAreSet()
        {
            SetupPreferences(out Prefs preferences, out MockedFileSystem fileSystem);
            string settingName = WellKnownPreference.DefaultEditorCommand;
            string expectedValue = "Code.exe";
            string prefsFileContent = $@"This first line is invalid for a prefs file.
{settingName}={expectedValue}
This third line is invalid as well";
            fileSystem.AddFile(preferences.PreferencesFilePath, prefsFileContent);

            Dictionary<string, string> preferencesDictionary = preferences.ReadPreferences();

            Assert.Equal(expectedValue, preferencesDictionary[settingName]);
        }

        [Fact]
        public void WritePreferences_SomeDefault_OnlyWritesNonDefaultValues()
        {
            string defaultEditor = "Code.exe";
            string errorColor = "BoldMagenta";
            string expected = $@"{WellKnownPreference.ErrorColor}={errorColor}
{WellKnownPreference.DefaultEditorCommand}={defaultEditor}";

            SetupPreferences(out Prefs preferences, out MockedFileSystem fileSystem);

            // Setup the preferences dictionary to have the default preferences, except for one that was modified
            // and one that was added. Only the modified and the added preferences should be written to the file system
            Dictionary<string, string> preferencesToWrite = new Dictionary<string, string>(preferences.GetDefaultPreferences());
            preferencesToWrite[WellKnownPreference.DefaultEditorCommand] = defaultEditor;
            preferencesToWrite[WellKnownPreference.ErrorColor] = errorColor;

            bool succeeded = preferences.WritePreferences(preferencesToWrite);

            Assert.True(succeeded);

            Assert.Equal(expected, fileSystem.ReadFile(preferences.PreferencesFilePath));
        }

        private void SetupPreferences(out Prefs preferences, out MockedFileSystem fileSystem)
        {
            fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            preferences = new Prefs(fileSystem, userProfileDirectoryProvider);
        }

        private void ConfirmAllPreferencesAreDefaults(IPreferences preferences)
        {
            var defaultPreferences = preferences.GetDefaultPreferences();
            var currentPreferences = preferences.ReadPreferences();
            Assert.Equal(defaultPreferences.Count, currentPreferences.Count);
            foreach (KeyValuePair<string, string> kvp in defaultPreferences)
            {
                Assert.True(currentPreferences.ContainsKey(kvp.Key));
                Assert.Equal(kvp.Value, currentPreferences[kvp.Key]);
            }
        }
    }
}
