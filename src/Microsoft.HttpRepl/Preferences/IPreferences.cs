using System.Collections.Generic;

namespace Microsoft.HttpRepl.Preferences
{
    public interface IPreferences
    {
        IReadOnlyDictionary<string, string> GetDefaultPreferences();
        Dictionary<string, string> ReadPreferences();
        bool WritePreferences(Dictionary<string, string> preferences);
    }
}
