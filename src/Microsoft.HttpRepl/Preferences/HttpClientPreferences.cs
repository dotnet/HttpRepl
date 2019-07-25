using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.HttpRepl.Preferences
{
    public static class HttpClientPreferences
    {
        public static class Catalog
        {
            private static IReadOnlyList<string> _names;

            public static IReadOnlyList<string> Names
            {
                get
                {
                    if (_names != null)
                    {
                        return _names;
                    }

                    List<string> matchingProperties = new List<string>();

                    foreach (PropertyInfo property in typeof(HttpClientPreferences).GetProperties(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (property.PropertyType == typeof(string) && property.GetMethod != null && property.GetValue(null) is string val)
                        {
                            matchingProperties.Add(val);
                        }
                    }

                    return _names = matchingProperties;
                }
            }
        }

        public static string UseDefaultCredentials { get; } = "httpClient.useDefaultCredentials";
    }
}
