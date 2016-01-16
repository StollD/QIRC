/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2016
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// System
using System;

/// <summary>
/// The main namespace. Here's everything that executes actively.
/// </summary>
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
            public const String Bold = "\x02";
            public const String Color = "\x03";
            public const String Italic = "\x09";
            public const String StrikeThrough = "\x13";
            public const String Reset = "\x0f";
            public const String Underline = "\x15";
            public const String Reverse = "\x16";
        }

        /// <summary>
        /// Color Codes for IRC
        /// </summary>
        public static class ColorCode
        {
            public const Int32 White = 0;
            public const Int32 Black = 1;
            public const Int32 DarkBlue = 2;
            public const Int32 DarkGreen = 3;
            public const Int32 Red = 4;
            public const Int32 DarkRed = 5;
            public const Int32 DarkViolet = 6;
            public const Int32 Orange = 7;
            public const Int32 Yellow = 8;
            public const Int32 LightGreen = 9;
            public const Int32 Cyan = 10;
            public const Int32 LightCyan = 11;
            public const Int32 Blue = 12;
            public const Int32 Violet = 13;
            public const Int32 DarkGray = 14;
            public const Int32 LightGray = 15;

            /// <summary>
            /// Parses a string into a color code
            /// </summary>
            public static Boolean TryParse(String input, out Int32 color)
            {
                try
                {
                    color = (Int32)typeof(ColorCode).GetField(input).GetValue(null);
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
        public static String Format(String input)
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
                String color = input.Split(new[] { "[color=" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                String code = color.Split(new[] { "]" }, 2, StringSplitOptions.RemoveEmptyEntries)[0];
                if (code.Contains(","))
                {
                    String[] colors = code.Split(',');
                    Int32 foreground, background = 0;
                    ColorCode.TryParse(colors[0], out foreground);
                    ColorCode.TryParse(colors[1], out background);
                    input = input.Replace("[color=" + code + "]", ControlCode.Color + foreground.ToString("00") + "," + background.ToString("00"));
                }
                else
                {
                    Int32 foreground = 0;
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