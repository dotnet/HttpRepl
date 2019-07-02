using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.UserProfile;

namespace Microsoft.HttpRepl.Preferences
{
    public class PreferencesProvider : IPreferencesProvider
    {
        private string _prefsFilePath;
        private readonly IFileSystem _fileSystem;
        private readonly IUserProfileDirectoryProvider _userProfileDirectoryProvider;
        private readonly Dictionary<string, string> _defaultPreferences = new Dictionary<string, string>()
        {
            { WellKnownPreference.ProtocolColor, "BoldGreen" },
            { WellKnownPreference.StatusColor, "BoldYellow" },

            { WellKnownPreference.JsonArrayBraceColor, "BoldCyan" },
            { WellKnownPreference.JsonCommaColor, "BoldYellow" },
            { WellKnownPreference.JsonNameColor, "BoldMagenta" },
            { WellKnownPreference.JsonNameSeparatorColor, "BoldWhite" },
            { WellKnownPreference.JsonObjectBraceColor, "Cyan" },
            { WellKnownPreference.JsonColor, "Green" }
        };

        public string PreferencesFilePath
        {
            get
            {
                if (_prefsFilePath == null)
                {
                    string userProfileDirectory = _userProfileDirectoryProvider.GetUserProfileDirectory();
                    _prefsFilePath = Path.Combine(userProfileDirectory, ".httpreplprefs");
                }
                return _prefsFilePath;
            }
        }         

        public PreferencesProvider(IFileSystem fileSystem, IUserProfileDirectoryProvider userProfileDirectoryProvider)
        {
            _fileSystem = fileSystem;
            _userProfileDirectoryProvider = userProfileDirectoryProvider;
        }

        public IReadOnlyDictionary<string, string> GetDefaultPreferences()
        {
            return _defaultPreferences;
        }

        public Dictionary<string, string> ReadPreferences()
        {
            var preferences = new Dictionary<string, string>(_defaultPreferences);

            if (_fileSystem.FileExists(PreferencesFilePath))
            {
                string[] prefsFile = _fileSystem.ReadAllLinesFromFile(PreferencesFilePath);

                foreach (string line in prefsFile)
                {
                    int equalsIndex = line.IndexOf('=');

                    if (equalsIndex < 0)
                    {
                        continue;
                    }

                    preferences[line.Substring(0, equalsIndex)] = line.Substring(equalsIndex + 1);
                }
            }

            return preferences;
        }

        public bool WritePreferences(Dictionary<string, string> preferences)
        {
            List<string> lines = new List<string>();
            foreach (KeyValuePair<string, string> entry in preferences.OrderBy(x => x.Key))
            {
                //If the value didn't exist in the defaults or the value's different, include it in the user's preferences file
                if (!_defaultPreferences.TryGetValue(entry.Key, out string value) || !string.Equals(value, entry.Value, StringComparison.Ordinal))
                {
                    lines.Add($"{entry.Key}={entry.Value}");
                }
            }

            try
            {
                _fileSystem.WriteAllLinesToFile(PreferencesFilePath, lines);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
