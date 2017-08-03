/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Unicode;
using ChatSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;

namespace QIRC.Unicode
{
    /// <summary>
    /// The Unicode command. Takes a sequence of characters or numbers and posts info for them
    /// </summary>
    public class UnicodeInformation : IrcCommand
    {
        /// <summary>
        /// The Access Level that is needed to execute the command
        /// </summary>
        public override AccessLevel GetAccessLevel()
        {
            return AccessLevel.NORMAL;
        }

        /// <summary>
        /// The name of the command
        /// </summary>
        public override String GetName()
        {
            return "u";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Outputs informations about unicode characters.";
        }

        /// <summary>
        /// Whether the command can be used in serious channels.
        /// </summary>
        public override Boolean IsSerious()
        {
            return true;
        }

        /// <summary>
        /// An example for using the command.
        /// </summary>
        /// <returns></returns>
        public override String GetExample()
        {
            return Settings.Read<String>("control") + GetName() + " Ω";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (String.IsNullOrWhiteSpace(message.Message))
            {
                BotController.SendMessage(client, "What code point do you want me to look up?", message.User, message.Source, true);
                return;
            }

            List<UnicodeCharInfo> characters = null;
            if (message.Message.StartsWith("U+", true, CultureInfo.CurrentCulture))
            {
                Int32 codePoint = 0;
                if (Int32.TryParse(message.Message.Substring(2), NumberStyles.HexNumber, new NumberFormatInfo(), out codePoint))
                    characters = new List<UnicodeCharInfo> { UnicodeInfo.GetCharInfo(codePoint) };
                else
                    BotController.SendMessage(client, "That's not a valid code point.", message.User, message.Source, true);
            }
            else
            {
                String[] wide = TextElements(message.Message).ToArray();
                characters = new List<UnicodeCharInfo>();
                for (Int32 i = 0; i < wide.Length; i++)
                    foreach (Int32 p in WideCharCodePoint(wide[i]))
                        characters.Add(UnicodeInfo.GetCharInfo(p));
            }

            // Output
            Int32 maxChars = Settings.Read<UnicodeConfig>("Unicode").maxChars;
            if (maxChars <= characters.Count)
            {
                BotController.SendMessage(client, "Too many characters! (Maximum: " + maxChars + ")", message.User, message.Source);
                return;
            }
            for (Int32 i = 0; i < characters.Count; i++)
            {
                String number = characters[i].CodePoint.ToString("X");
                while (number.Length < 4) number = "0" + number;
                String reply = $"U+{number} {characters[i].Name} ({UnicodeInfo.GetDisplayText(characters[i])})";
                BotController.SendMessage(client, reply, message.User, message.Source, true);
                Thread.Sleep(200);
            }
        }

        public static IEnumerable<Int32> WideCharCodePoint(String s)
        {
            Byte[] bytes = Encoding.UTF32.GetBytes(s);
            for (Int32 i = 0; i < bytes.Length / 4; i++)
                yield return BitConverter.ToInt32(bytes, i * 4);
        }

        // Preparation for wide characters
        public static IEnumerable<String> TextElements(String s)
        {
            var en = StringInfo.GetTextElementEnumerator(s);
            while (en.MoveNext())
            {
                yield return en.GetTextElement();
            }
        }
        
        /// <summary>
        /// Adds the Settings to the config
        /// </summary>
        public override void OnLoad()
        {
            SettingsFile file = null;
            Settings.GetFile("settings", ref file);
            file.Add("Unicode", new UnicodeConfig());
        }
    }
}
