using System.Collections.Generic;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Preferences
{
    public class PreferencesProviderTests
    {
        [Fact]
        public void ReadPreferences_NoPreferencesFile_AllDefaults()
        {
            SetupPreferencesProvider(out IPreferencesProvider preferencesProvider, out _);

            ConfirmAllPreferencesAreDefaults(preferencesProvider);
        }

        [Fact]
        public void ReadPreferences_BlankPreferencesFile_AllDefaults()
        {
            SetupPreferencesProvider(out IPreferencesProvider preferencesProvider, out MockedFileSystem fileSystem);
            fileSystem.AddFile(preferencesProvider.PreferencesFilePath, "");

            ConfirmAllPreferencesAreDefaults(preferencesProvider);
        }

        [Fact]
        public void ReadPreferences_InvalidPreferencesFile_AllDefaults()
        {
            SetupPreferencesProvider(out IPreferencesProvider preferencesProvider, out MockedFileSystem fileSystem);
            fileSystem.AddFile(preferencesProvider.PreferencesFilePath, "This is not a valid preferences file.");

            ConfirmAllPreferencesAreDefaults(preferencesProvider);
        }

        [Fact]
        public void ReadPreferences_PartiallyInvalidPreferencesFile_ValidPrefsAreSet()
        {
            SetupPreferencesProvider(out IPreferencesProvider preferencesProvider, out MockedFileSystem fileSystem);
            string settingName = WellKnownPreference.DefaultEditorCommand;
            string expectedValue = "Code.exe";
            string prefsFileContent = $@"This first line is invalid for a prefs file.
{settingName}={expectedValue}
This third line is invalid as well";
            fileSystem.AddFile(preferencesProvider.PreferencesFilePath, prefsFileContent);

            Dictionary<string, string> preferences = preferencesProvider.ReadPreferences();

            Assert.Equal(expectedValue, preferences[settingName]);
        }

        [Fact]
        public void WritePreferences_SomeDefault_OnlyWritesNonDefaultValues()
        {
            string defaultEditor = "Code.exe";
            string errorColor = "BoldMagenta";
            string expected = $@"{WellKnownPreference.ErrorColor}={errorColor}
{WellKnownPreference.DefaultEditorCommand}={defaultEditor}";

            SetupPreferencesProvider(out IPreferencesProvider preferencesProvider, out MockedFileSystem fileSystem);

            // Setup the preferences dictionary to have the default preferences, except for one that was modified
            // and one that was added. Only the modified and the added preferences should be written to the file system
            Dictionary<string, string> preferencesToWrite = new Dictionary<string, string>(preferencesProvider.GetDefaultPreferences());
            preferencesToWrite[WellKnownPreference.DefaultEditorCommand] = defaultEditor;
            preferencesToWrite[WellKnownPreference.ErrorColor] = errorColor;

            bool succeeded = preferencesProvider.WritePreferences(preferencesToWrite);

            Assert.True(succeeded);

            Assert.Equal(expected, fileSystem.ReadFile(preferencesProvider.PreferencesFilePath));
        }

        private void SetupPreferencesProvider(out IPreferencesProvider preferencesProvider, out MockedFileSystem fileSystem)
        {
            fileSystem = new MockedFileSystem();
            IUserProfileDirectoryProvider userProfileDirectoryProvider = new UserProfileDirectoryProvider();
            preferencesProvider = new PreferencesProvider(fileSystem, userProfileDirectoryProvider);
        }

        private void ConfirmAllPreferencesAreDefaults(IPreferencesProvider preferencesProvider)
        {
            var defaultPreferences = preferencesProvider.GetDefaultPreferences();
            var currentPreferences = preferencesProvider.ReadPreferences();
            Assert.Equal(defaultPreferences.Count, currentPreferences.Count);
            foreach (KeyValuePair<string, string> kvp in defaultPreferences)
            {
                Assert.True(currentPreferences.ContainsKey(kvp.Key));
                Assert.Equal(kvp.Value, currentPreferences[kvp.Key]);
            }
        }
    }
}
