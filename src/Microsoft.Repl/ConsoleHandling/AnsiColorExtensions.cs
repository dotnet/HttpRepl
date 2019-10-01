// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Repl.ConsoleHandling
{
    public static class AnsiColorExtensions
    {
        // For reference on these codes and values, see:
        // https://en.wikipedia.org/wiki/ANSI_escape_code#Escape_sequences
        private static readonly string _ansiControlSequenceIntroducer = "\x1B[";
        private static readonly string _ansiSgrCode = "m";
        private static readonly string _ansiSgrDefaultForegroundColor = $"{_ansiControlSequenceIntroducer}39{_ansiSgrCode}";
        private static readonly string _ansiSgrBold = $"{_ansiControlSequenceIntroducer}1{_ansiSgrCode}";

        public static string Black(this string text)
        {
            return SetColorInternal(text, AllowedColors.Black);
        }

        public static string Red(this string text)
        {
            return SetColorInternal(text, AllowedColors.Red);
        }
        public static string Green(this string text)
        {
            return SetColorInternal(text, AllowedColors.Green);
        }

        public static string Yellow(this string text)
        {
            return SetColorInternal(text, AllowedColors.Yellow);
        }

        public static string Blue(this string text)
        {
            return SetColorInternal(text, AllowedColors.Blue);
        }

        public static string Magenta(this string text)
        {
            return SetColorInternal(text, AllowedColors.Magenta);
        }

        public static string Cyan(this string text)
        {
            return SetColorInternal(text, AllowedColors.Cyan);
        }

        public static string White(this string text)
        {
            return SetColorInternal(text, AllowedColors.White);
        }

        public static string Bold(this string text)
        {
            return $"{_ansiSgrBold}{text}{_ansiSgrDefaultForegroundColor}";
        }

        private static string SetColorInternal(string text, AllowedColors color)
        {
            int sgrParameter = (int)color;
            return $"{_ansiControlSequenceIntroducer}{sgrParameter}{_ansiSgrCode}{text}{_ansiSgrDefaultForegroundColor}";
        }

        public static string SetColor(this string textToColor, AllowedColors color)
        {
            if (color.HasFlag(AllowedColors.Bold))
            {
                textToColor = textToColor.Bold();
                color = color & ~AllowedColors.Bold;
            }

            switch (color)
            {
                case AllowedColors.Black:
                case AllowedColors.Red:
                case AllowedColors.Green:
                case AllowedColors.Yellow:
                case AllowedColors.Blue:
                case AllowedColors.Magenta:
                case AllowedColors.Cyan:
                case AllowedColors.White:
                    return SetColorInternal(textToColor, color);
                default:
                    return textToColor;
            }
        }
    }
}
