// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Preferences
{
    public class ResponseConfig : RequestOrResponseConfig
    {
        public ResponseConfig(IPreferences preferences)
            : base(preferences)
        {
        }

        public override AllowedColors BodyColor => Preferences.GetColorValue(WellKnownPreference.ResponseBodyColor, base.BodyColor);

        public override AllowedColors SchemeColor => Preferences.GetColorValue(WellKnownPreference.ResponseSchemeColor, base.SchemeColor);

        public override AllowedColors HeaderKeyColor => Preferences.GetColorValue(WellKnownPreference.ResponseHeaderKeyColor, base.HeaderKeyColor);

        public override AllowedColors HeaderSeparatorColor => Preferences.GetColorValue(WellKnownPreference.ResponseHeaderSeparatorColor, base.HeaderSeparatorColor);

        public override AllowedColors HeaderValueSeparatorColor => Preferences.GetColorValue(WellKnownPreference.ResponseHeaderValueSeparatorColor, base.HeaderValueSeparatorColor);

        public override AllowedColors HeaderValueColor => Preferences.GetColorValue(WellKnownPreference.ResponseHeaderValueColor, base.HeaderValueColor);

        public override AllowedColors HeaderColor => Preferences.GetColorValue(WellKnownPreference.ResponseHeaderColor, base.HeaderColor);

        public override AllowedColors GeneralColor => Preferences.GetColorValue(WellKnownPreference.ResponseColor, base.GeneralColor);

        public override AllowedColors ProtocolColor => Preferences.GetColorValue(WellKnownPreference.ResponseProtocolColor, base.ProtocolColor);

        public override AllowedColors ProtocolNameColor => Preferences.GetColorValue(WellKnownPreference.ResponseProtocolNameColor, base.ProtocolNameColor);

        public override AllowedColors ProtocolVersionColor => Preferences.GetColorValue(WellKnownPreference.ResponseProtocolVersionColor, base.ProtocolVersionColor);

        public override AllowedColors ProtocolSeparatorColor => Preferences.GetColorValue(WellKnownPreference.ResponseProtocolSeparatorColor, base.ProtocolSeparatorColor);

        public override AllowedColors StatusColor => Preferences.GetColorValue(WellKnownPreference.ResponseStatusColor, base.StatusColor);

        public override AllowedColors StatusCodeColor => Preferences.GetColorValue(WellKnownPreference.ResponseStatusCodeColor, base.StatusCodeColor);

        public override AllowedColors StatusReasonPhraseColor => Preferences.GetColorValue(WellKnownPreference.ResponseStatusReaseonPhraseColor, base.StatusReasonPhraseColor);
    }
}
