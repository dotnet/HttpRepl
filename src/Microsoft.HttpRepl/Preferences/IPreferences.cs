using System.Collections.Generic;

namespace Microsoft.HttpRepl.Preferences
{
    public interface IPreferences
    {
        Dictionary<string, string> ReadPreferences(IReadOnlyDictionary<string, string> defaultPreferences);
        bool WritePreferences(Dictionary<string, string> preferences, IReadOnlyDictionary<string, string> defaultPreferences);
    }
}
