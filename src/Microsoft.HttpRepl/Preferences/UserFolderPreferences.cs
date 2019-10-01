// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Preferences
{
    public class UserFolderPreferences : IPreferences
    {
        private string _prefsFilePath;
        private readonly IFileSystem _fileSystem;
        private readonly IUserProfileDirectoryProvider _userProfileDirectoryProvider;

        private Dictionary<string, string> _preferences;

        private Dictionary<string, string> Preferences
        {
            get
            {
                if (_preferences == null)
                {
                    _preferences = ReadPreferences(DefaultPreferences);
                }

                return _preferences;
            }
        }

        public IReadOnlyDictionary<string, string> DefaultPreferences { get; }

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

        public UserFolderPreferences(IFileSystem fileSystem, IUserProfileDirectoryProvider userProfileDirectoryProvider, IDictionary<string, string> defaultPreferences)
        {
            _fileSystem = fileSystem;
            _userProfileDirectoryProvider = userProfileDirectoryProvider;
            DefaultPreferences = SetupDefaults(defaultPreferences);
        }

        public AllowedColors GetColorValue(string preference, AllowedColors defaultValue = AllowedColors.None)
        {
            if (!Preferences.TryGetValue(preference, out string preferenceValueString) || !Enum.TryParse(preferenceValueString, true, out AllowedColors result))
            {
                result = defaultValue;
            }

            return result;
        }

        public int GetIntValue(string preference, int defaultValue)
        {
            if (!Preferences.TryGetValue(preference, out string preferenceValueString) || !int.TryParse(preferenceValueString, out int result))
            {
                result = defaultValue;
            }

            return result;
        }

        public bool GetBoolValue(string preference, bool defaultValue)
        {
            if (!Preferences.TryGetValue(preference, out string preferenceValueString) || !bool.TryParse(preferenceValueString, out bool result))
            {
                result = defaultValue;
            }

            return result;
        }

        public string GetValue(string preference, string defaultValue = default)
        {
            if (!Preferences.TryGetValue(preference, out string result))
            {
                result = defaultValue;
            }

            return result;
        }

        public bool TryGetValue(string preference, out string value)
        {
            return Preferences.TryGetValue(preference, out value);
        }

        public bool SetValue(string preference, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (!DefaultPreferences.TryGetValue(preference, out string defaultValue))
                {
                    Preferences.Remove(preference);
                }
                else
                {
                    Preferences[preference] = defaultValue;
                }
            }
            else
            {
                Preferences[preference] = value;
            }

            return WritePreferences(Preferences, DefaultPreferences);
        }

        public IReadOnlyDictionary<string, string> CurrentPreferences
        {
            get
            {
                return new ReadOnlyDictionary<string, string>(Preferences);
            }
        }

        private Dictionary<string, string> ReadPreferences(IReadOnlyDictionary<string, string> defaultPreferences)
        {
            var preferences = new Dictionary<string, string>(defaultPreferences);

            if (_fileSystem.FileExists(PreferencesFilePath))
            {
                string[] prefsFile = _fileSystem.ReadAllLinesFromFile(PreferencesFilePath);

                foreach (string line in prefsFile)
                {
                    int equalsIndex = line.IndexOf('=', StringComparison.Ordinal);

                    // If there's no = or = is the first character on the line
                    // (meaning no preference name), move to the next line
                    if (equalsIndex <= 0)
                    {
                        continue;
                    }

                    preferences[line.Substring(0, equalsIndex)] = line.Substring(equalsIndex + 1);
                }
            }

            return preferences;
        }

        private bool WritePreferences(Dictionary<string, string> preferences, IReadOnlyDictionary<string, string> defaultPreferences)
        {
            List<string> lines = new List<string>();
            foreach (KeyValuePair<string, string> entry in preferences.OrderBy(x => x.Key))
            {
                //If the value didn't exist in the defaults or the value's different, include it in the user's preferences file
                if (!defaultPreferences.TryGetValue(entry.Key, out string value) || !string.Equals(value, entry.Value, StringComparison.Ordinal))
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

        private IReadOnlyDictionary<string, string> SetupDefaults(IDictionary<string, string> defaults)
        {
            Dictionary<string, string> tempDictionary = new Dictionary<string, string>();

            if (defaults != null)
            {
                foreach (var kvp in defaults)
                {
                    tempDictionary.Add(kvp.Key, kvp.Value);
                }
            }

            return new ReadOnlyDictionary<string, string>(tempDictionary);
        }
    }
}
