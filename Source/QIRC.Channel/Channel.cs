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
using System.Collections.Generic;
using System.Linq;

namespace QIRC.Commands
{
    /// <summary>
    /// This command provides channel settings for QIRC
    /// </summary>
    public class Channel : IrcCommand
    {
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
            return "channel";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Edits the settings for the channel";
        }

        /// <summary>
        /// Whether the command can be used in serious channels.
        /// </summary>
        public override Boolean IsSerious()
        {
            return true;
        }

        /// <summary>
        /// The Parameters of the Command
        /// </summary>
        public override String[] GetParameters()
        {
            return new String[]
            {
                "serious", "If the channel should be serious or not",
                "secret", "If the channel should be hidden.",
                "state", "Displays the settings for this channel"
            };
        }

        /// <summary>
        /// An example for using the command.
        /// </summary>
        /// <returns></returns>
        public override String GetExample()
        {
            return Settings.Read<String>("control") + GetName() + " -secret:true";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            // Check if this is a private message
            if (!message.IsChannelMessage)
            {
                QIRC.SendMessage(client, "This command doesn't work in PM", message.User, message.Source, true);
                return;
            }

            // Get the channel
            List<ProtoIrcChannel> list = Settings.Read<List<ProtoIrcChannel>>("channels");
            ProtoIrcChannel channel = list.Find(c => String.Equals(c.name, message.Source, StringComparison.InvariantCultureIgnoreCase));
            String msg = message.Message;
            if (StartsWithParam("serious", msg))
            {
                channel.serious = Boolean.Parse(StripParam("serious", ref msg));
            }
            if (StartsWithParam("secret", msg))
            {
                channel.secret = Boolean.Parse(StripParam("secret", ref msg));
            }
            if (StartsWithParam("state", msg))
            {
                QIRC.SendMessage(client, $"Serious: {channel.serious}, Secret: {channel.secret}", message.User, message.Source);
            }
            list[list.IndexOf(channel)] = channel;
            Settings.Write("channels", list);
        }
    }
}
