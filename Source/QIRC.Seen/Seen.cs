/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using ChatSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the seen command. The bot will store the latest message of
    /// every user, and output it on demand.
    /// </summary>
    public class Seen : IrcCommand
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
            return "seen";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Outputs the latest message of a user and the time when he wrote it.";
        }

        /// <summary>
        /// The Parameters of the Command
        /// </summary>
        public override String[] GetParameters()
        {
            return new String[]
            {
                "channel", "The channel where the user was seen the last time"
            };
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
            return Settings.Read<String>("control") + GetName() + " Thomas";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (StartsWithParam("channel", message.Message))
            {
                // Create a wildcard
                String text = message.Message;
                String target = StripParam("channel", ref text);
                String wildcard = "^" + Regex.Escape(text.Trim()).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
                ProtoIrcMessage[] messages = QIRC.messages.Where(p => Regex.IsMatch(p.User, wildcard, RegexOptions.IgnoreCase) && p.IsChannelMessage && p.Source == target).ToArray();
                if (messages.Length == 0)
                {
                    // User is MIA
                    QIRC.SendMessage(client, "I haven't seen the user [b]" + text.Trim() + "[/b] in the channel [b]" + target + "[/b] yet.", message.User, message.Source);
                    return;
                }
                ProtoIrcMessage lastMsg = messages.OrderBy(p => p.Time.Ticks).ElementAtOrDefault(messages.Length - 2);
                QIRC.SendMessage(client, "I last saw [b]" + lastMsg.User + "[/b] on [b][" + lastMsg.Time.ToString("dd.MM.yyyy HH:mm:ss") + "][/b] in [b]" + target + "[/b] saying: \"" + lastMsg.Message + "\"", message.User, message.Source);
            }
            else
            {
                // Create a wildcard
                String wildcard = "^" + Regex.Escape(message.Message.Trim()).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
                ProtoIrcMessage[] messages = QIRC.messages.Where(p => Regex.IsMatch(p.User, wildcard, RegexOptions.IgnoreCase) && p.IsChannelMessage).ToArray();
                if (messages.Length == 0)
                {
                    // User is MIA
                    QIRC.SendMessage(client, "I haven't seen the user [b]" + message.Message.Trim() + "[/b] yet.", message.User, message.Source);
                    return;
                }
                ProtoIrcMessage lastMsg = messages.OrderBy(p => p.Time.Ticks).ElementAtOrDefault(messages.Length - 2);
                QIRC.SendMessage(client, "I last saw [b]" + lastMsg.User + "[/b] on [b][" + lastMsg.Time.ToString("dd.MM.yyyy HH:mm:ss") + "][/b] in [b]" + lastMsg.Source + "[/b] saying: \"" + lastMsg.Message + "\"", message.User, message.Source);
            }
        }
    }
}
