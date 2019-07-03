// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.UserProfile;

namespace Microsoft.HttpRepl.Preferences
{
    public class UserFolderPreferences : IPreferences
    {
        private string _prefsFilePath;
        private readonly IFileSystem _fileSystem;
        private readonly IUserProfileDirectoryProvider _userProfileDirectoryProvider;

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

        public UserFolderPreferences(IFileSystem fileSystem, IUserProfileDirectoryProvider userProfileDirectoryProvider)
        {
            _fileSystem = fileSystem;
            _userProfileDirectoryProvider = userProfileDirectoryProvider;
        }

        public Dictionary<string, string> ReadPreferences(IReadOnlyDictionary<string, string> defaultPreferences)
        {
            var preferences = new Dictionary<string, string>(defaultPreferences);

            if (_fileSystem.FileExists(PreferencesFilePath))
            {
                string[] prefsFile = _fileSystem.ReadAllLinesFromFile(PreferencesFilePath);

                foreach (string line in prefsFile)
                {
                    int equalsIndex = line.IndexOf('=');

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

        public bool WritePreferences(Dictionary<string, string> preferences, IReadOnlyDictionary<string, string> defaultPreferences)
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
    }
}
