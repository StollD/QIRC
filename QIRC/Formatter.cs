/**
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) 2015 @ThomasKerman (GitLab|GitHub) / Thomas (EsperNet IRC)
 * License: MIT License
 */

using System;
using System.IO;

namespace QIRC
{
    /// <summary>
    /// Class to format a string to match IRC control characters
    /// </summary>
    public class Formatter
    {
        /// <summary>
        /// Control Codes for IRC
        /// </summary>
        public static class ControlCode
        {
            public const string Bold = "\x02";
            public const string Color = "\x03";
            public const string Italic = "\x09";
            public const string StrikeThrough = "\x13";
            public const string Reset = "\x0f";
            public const string Underline = "\x15";
            public const string Reverse = "\x16";
        }

        /// <summary>
        /// Color Codes for IRC
        /// </summary>
        public static class ColorCode
        {
            public const int White = 0;
            public const int Black = 1;
            public const int DarkBlue = 2;
            public const int DarkGreen = 3;
            public const int Red = 4;
            public const int DarkRed = 5;
            public const int DarkViolet = 6;
            public const int Orange = 7;
            public const int Yellow = 8;
            public const int LightGreen = 9;
            public const int Cyan = 10;
            public const int LightCyan = 11;
            public const int Blue = 12;
            public const int Violet = 13;
            public const int DarkGray = 14;
            public const int LightGray = 15;

            /// <summary>
            /// Parses a string into a color code
            /// </summary>
            public static bool TryParse(string input, out int color)
            {
                try
                {
                    color = (int)typeof(ColorCode).GetField(input).GetValue(null);
                    return true;
                }
                catch
                {
                    color = 0;
                    return false;
                }
            }
        }

        /// <summary>
        /// Formats a message to make color codes easier to write
        /// </summary>
        public static string Format(string input)
        {
            /// Bold
            input = input.Replace("[b]", ControlCode.Bold.ToString()).Replace("[/b]", ControlCode.Bold.ToString());

            /// Italic
            input = input.Replace("[i]", ControlCode.Italic.ToString()).Replace("[/i]", ControlCode.Italic.ToString());

            /// Strikethrough
            input = input.Replace("[s]", ControlCode.StrikeThrough.ToString()).Replace("[/s]", ControlCode.StrikeThrough.ToString());

            /// Underline
            input = input.Replace("[u]", ControlCode.Underline.ToString()).Replace("[/u]", ControlCode.Underline.ToString());

            /// Reverse
            input = input.Replace("[r]", ControlCode.Reverse.ToString()).Replace("[/r]", ControlCode.Reverse.ToString());

            /// Color
            while (input.Contains("[color="))
            {
                string color = input.Split("[color=", 2)[1];
                string code = color.Split("]", 2)[0];
                if (code.Contains(","))
                {
                    string[] colors = code.Split(",");
                    int foreground, background = 0;
                    ColorCode.TryParse(colors[0], out foreground);
                    ColorCode.TryParse(colors[1], out background);
                    input = input.Replace("[color=" + code + "]", ControlCode.Color + foreground.ToString("00") + "," + background.ToString("00"));
                }
                else
                {
                    int foreground = 0;
                    ColorCode.TryParse(code, out foreground);
                    input = input.Replace("[color=" + code + "]", ControlCode.Color + foreground.ToString("00"));
                }
            }
            input = input.Replace("[/color]", ControlCode.Color.ToString());

            /// Return
            return input;
        }
    }
}