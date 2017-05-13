/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;
using System.Linq;
using System.Text.RegularExpressions;
using ChatSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;

namespace QIRC.Seen
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
                Regex wildcard = new Regex("^" + Regex.Escape(text.Trim()).Replace(@"\*", ".*").Replace(@"\?", ".") + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                ProtoIrcMessage[] messages = ProtoIrcMessage.Query.OrderByDescending(m => m.Time).ToArray();
                ProtoIrcMessage lastMsg = null;
                for (Int32 i = 0; i < messages.Length; i++)
                {
                    ProtoIrcMessage p = messages[i];
                    if (wildcard.IsMatch(p.User) && p.IsChannelMessage && p.Source == target && (!BotController.GetChannel(p.Source).secret || message.Source == p.Source))
                    {
                        lastMsg = p;
                        break;
                    }
                }
                if (lastMsg == null)
                {
                    // User is MIA
                    BotController.SendMessage(client, "I haven't seen the user [b]" + text.Trim() + "[/b] in the channel [b]" + target + "[/b] yet.", message.User, message.Source);
                    return;
                }
                BotController.SendMessage(client, "I last saw [b]" + lastMsg.User + "[/b] on [b][" + lastMsg.Time.ToString("dd.MM.yyyy HH:mm:ss") + "][/b] in [b]" + target + "[/b] saying: \"" + lastMsg.Message + "\"", message.User, message.Source);
            }
            else
            {
                // Create a wildcard
                Regex wildcard = new Regex("^" + Regex.Escape(message.Message.Trim()).Replace(@"\*", ".*").Replace(@"\?", ".") + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                ProtoIrcMessage[] messages = ProtoIrcMessage.Query.OrderByDescending(m => m.Time).ToArray();
                ProtoIrcMessage lastMsg = null;
                for (Int32 i = 0; i < messages.Length; i++)
                {
                    ProtoIrcMessage p = messages[i];
                    if (wildcard.IsMatch(p.User) && p.IsChannelMessage && (!BotController.GetChannel(p.Source).secret || message.Source == p.Source))
                    {
                        lastMsg = p;
                        break;
                    }
                }
                if (lastMsg == null)
                {
                    // User is MIA
                    BotController.SendMessage(client, "I haven't seen the user [b]" + message.Message.Trim() + "[/b] yet.", message.User, message.Source);
                    return;
                }
                BotController.SendMessage(client, "I last saw [b]" + lastMsg.User + "[/b] on [b][" + lastMsg.Time.ToString("dd.MM.yyyy HH:mm:ss") + "][/b] in [b]" + lastMsg.Source + "[/b] saying: \"" + lastMsg.Message + "\"", message.User, message.Source);
            }
        }
    }
}
