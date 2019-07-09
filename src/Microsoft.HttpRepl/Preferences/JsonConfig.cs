// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Preferences
{
    public class JsonConfig : IJsonConfig
    {
        private readonly IPreferences _preferences;

        public int IndentSize => _preferences.GetIntValue(WellKnownPreference.JsonIndentSize, 2);

        public AllowedColors DefaultColor => _preferences.GetColorValue(WellKnownPreference.JsonColor);

        private AllowedColors DefaultBraceColor => _preferences.GetColorValue(WellKnownPreference.JsonBraceColor, DefaultSyntaxColor);

        private AllowedColors DefaultSyntaxColor => _preferences.GetColorValue(WellKnownPreference.JsonSyntaxColor, DefaultColor);

        private AllowedColors DefaultLiteralColor => _preferences.GetColorValue(WellKnownPreference.JsonLiteralColor, DefaultColor);

        public AllowedColors ArrayBraceColor => _preferences.GetColorValue(WellKnownPreference.JsonArrayBraceColor, DefaultBraceColor);

        public AllowedColors ObjectBraceColor => _preferences.GetColorValue(WellKnownPreference.JsonObjectBraceColor, DefaultBraceColor);

        public AllowedColors CommaColor => _preferences.GetColorValue(WellKnownPreference.JsonCommaColor, DefaultSyntaxColor);

        public AllowedColors NameColor => _preferences.GetColorValue(WellKnownPreference.JsonNameColor, StringColor);

        public AllowedColors NameSeparatorColor => _preferences.GetColorValue(WellKnownPreference.JsonNameSeparatorColor, DefaultSyntaxColor);

        public AllowedColors BoolColor => _preferences.GetColorValue(WellKnownPreference.JsonBoolColor, DefaultLiteralColor);

        public AllowedColors NumericColor => _preferences.GetColorValue(WellKnownPreference.JsonNumericColor, DefaultLiteralColor);

        public AllowedColors StringColor => _preferences.GetColorValue(WellKnownPreference.JsonStringColor, DefaultLiteralColor);

        public AllowedColors NullColor => _preferences.GetColorValue(WellKnownPreference.JsonNullColor, DefaultLiteralColor);

        public JsonConfig(IPreferences preferences)
        {
            _preferences = preferences;
        }
    }
}
