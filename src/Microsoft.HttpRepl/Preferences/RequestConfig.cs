// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl.Preferences
{
    public class RequestConfig : RequestOrResponseConfig
    {
        public RequestConfig(IPreferences preferences)
            : base(preferences)
        {
        }

        public override AllowedColors BodyColor => Preferences.GetColorValue(WellKnownPreference.RequestBodyColor, base.BodyColor);

        public override AllowedColors SchemeColor => Preferences.GetColorValue(WellKnownPreference.RequestSchemeColor, base.SchemeColor);

        public override AllowedColors HeaderKeyColor => Preferences.GetColorValue(WellKnownPreference.RequestHeaderKeyColor, base.HeaderKeyColor);

        public override AllowedColors HeaderSeparatorColor => Preferences.GetColorValue(WellKnownPreference.RequestHeaderSeparatorColor, base.HeaderSeparatorColor);

        public override AllowedColors HeaderValueSeparatorColor => Preferences.GetColorValue(WellKnownPreference.RequestHeaderValueSeparatorColor, base.HeaderValueSeparatorColor);

        public override AllowedColors HeaderValueColor => Preferences.GetColorValue(WellKnownPreference.RequestHeaderValueColor, base.HeaderValueColor);

        public override AllowedColors HeaderColor => Preferences.GetColorValue(WellKnownPreference.RequestHeaderColor, base.HeaderColor);

        public override AllowedColors GeneralColor => Preferences.GetColorValue(WellKnownPreference.RequestColor, base.GeneralColor);

        public override AllowedColors ProtocolColor => Preferences.GetColorValue(WellKnownPreference.RequestProtocolColor, base.ProtocolColor);

        public override AllowedColors ProtocolNameColor => Preferences.GetColorValue(WellKnownPreference.RequestProtocolNameColor, base.ProtocolNameColor);

        public override AllowedColors ProtocolVersionColor => Preferences.GetColorValue(WellKnownPreference.RequestProtocolVersionColor, base.ProtocolVersionColor);

        public override AllowedColors ProtocolSeparatorColor => Preferences.GetColorValue(WellKnownPreference.RequestProtocolSeparatorColor, base.ProtocolSeparatorColor);

        public override AllowedColors StatusColor => Preferences.GetColorValue(WellKnownPreference.RequestStatusColor, base.StatusColor);

        public override AllowedColors StatusCodeColor => Preferences.GetColorValue(WellKnownPreference.RequestStatusCodeColor, base.StatusCodeColor);

        public override AllowedColors StatusReasonPhraseColor => Preferences.GetColorValue(WellKnownPreference.RequestStatusReaseonPhraseColor, base.StatusReasonPhraseColor);

        public AllowedColors MethodColor => Preferences.GetColorValue(WellKnownPreference.RequestMethodColor, GeneralColor);

        public AllowedColors AddressColor => Preferences.GetColorValue(WellKnownPreference.RequestAddressColor, GeneralColor);
    }
}
