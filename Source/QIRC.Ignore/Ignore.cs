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
using QIRC.Serialization;

namespace QIRC.Commands
{
    /// <summary>
    /// A list of users ignored by the bot
    /// </summary>
    public class Ignore : IrcCommand
    {
        public static SerializeableList<String> ignores = new SerializeableList<String>("ignores");

        /// <summary>
        /// The Access Level that is needed to execute the command
        /// </summary>
        public override AccessLevel GetAccessLevel()
        {
            return AccessLevel.ADMIN;
        }

        /// <summary>
        /// The name of the command
        /// </summary>
        public override String GetName()
        {
            return "ignore";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Edits the list of users ignored by the bot";
        }

        /// <summary>
        /// The Parameters of the Command
        /// </summary>
        public override String[] GetParameters()
        {
            return new String[]
            {
                "remove", "Removes an ignored hostmask",
                "list", "Lists all ignored hostmasks"
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
            return Settings.Read<String>("control") + GetName() + " Spambot!*@*";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (String.IsNullOrWhiteSpace(message.Message))
            {
                QIRC.SendMessage(client, "Invalid hostmask!", message.User, message.Source);
                return;
            }

            if (StartsWithParam("remove", message.Message))
            {
                String msg = message.Message;
                StripParam("remove", ref msg);
                String w = "^" + Regex.Escape(message.Message.Trim()).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
                if (!ignores.Contains(w))
                {
                    QIRC.SendMessage(client, "This hostmask isn't ignored!", message.User, message.Source);
                    return;
                }
                ignores.Remove(w);
                QIRC.SendMessage(client, "Unignored \"" + msg.Trim() + "\"", message.User, message.Source);
                return;
            }

            if (StartsWithParam("list", message.Message))
            {
                QIRC.SendMessage(client, String.Join("; ", ignores), message.User, message.User, true);
                return;
            }
            String wildcard = "^" + Regex.Escape(message.Message.Trim()).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            if (ignores.Contains(wildcard))
            {
                QIRC.SendMessage(client, "This hostmask is already ignored!", message.User, message.Source);
                return;
            }
            ignores.Add(wildcard);
            QIRC.SendMessage(client, "Ignored \"" + message.Message + "\"", message.User, message.Source);
        }

        public override void OnLoad()
        {
            IrcCommand.ExecuteCheck.Add(val =>
            {
                IrcUser user = val.Item1;
                return !ignores.Any(s => Regex.IsMatch(user.Hostmask, s, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            });
        }
    }
}
