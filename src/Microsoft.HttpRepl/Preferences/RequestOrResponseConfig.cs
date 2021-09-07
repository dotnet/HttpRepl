// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Preferences
{
    public abstract class RequestOrResponseConfig
    {
        protected IPreferences Preferences { get; }

        protected RequestOrResponseConfig(IPreferences preferences)
        {
            Preferences = preferences;
        }

        public virtual AllowedColors BodyColor => Preferences.GetColorValue(WellKnownPreference.BodyColor, GeneralColor);

        public virtual AllowedColors SchemeColor => Preferences.GetColorValue(WellKnownPreference.SchemeColor, GeneralColor);

        public virtual AllowedColors HeaderKeyColor => Preferences.GetColorValue(WellKnownPreference.HeaderKeyColor, HeaderColor);

        public virtual AllowedColors HeaderSeparatorColor => Preferences.GetColorValue(WellKnownPreference.HeaderSeparatorColor, HeaderColor);

        public virtual AllowedColors HeaderValueSeparatorColor => Preferences.GetColorValue(WellKnownPreference.HeaderValueSeparatorColor, HeaderSeparatorColor);

        public virtual AllowedColors HeaderValueColor => Preferences.GetColorValue(WellKnownPreference.HeaderValueColor, HeaderColor);

        public virtual AllowedColors HeaderColor => Preferences.GetColorValue(WellKnownPreference.HeaderColor, GeneralColor);

        public virtual AllowedColors GeneralColor => Preferences.GetColorValue(WellKnownPreference.RequestOrResponseColor);

        public virtual AllowedColors ProtocolColor => Preferences.GetColorValue(WellKnownPreference.ProtocolColor, GeneralColor);

        public virtual AllowedColors ProtocolNameColor => Preferences.GetColorValue(WellKnownPreference.ProtocolNameColor, ProtocolColor);

        public virtual AllowedColors ProtocolVersionColor => Preferences.GetColorValue(WellKnownPreference.ProtocolVersionColor, ProtocolColor);

        public virtual AllowedColors ProtocolSeparatorColor => Preferences.GetColorValue(WellKnownPreference.ProtocolSeparatorColor, ProtocolColor);

        public virtual AllowedColors StatusColor => Preferences.GetColorValue(WellKnownPreference.StatusColor, GeneralColor);

        public virtual AllowedColors StatusCodeColor => Preferences.GetColorValue(WellKnownPreference.StatusCodeColor, StatusColor);

        public virtual AllowedColors StatusReasonPhraseColor => Preferences.GetColorValue(WellKnownPreference.StatusReaseonPhraseColor, StatusColor);
    }
}
