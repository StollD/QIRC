/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using ChatSharp;
using ChatSharp.Events;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the tell command. The bot will store messages for users and deliver
    /// them when the given user is online.
    /// </summary>
    public class Tell : IrcCommand
    {
        /// <summary>
        /// The messages that should be delivered
        /// </summary>
        public static SerializeableList<Msg> tells { get; set; }

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
            return "tell";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Stores messages and delivers them when the specified user is online.";
        }

        /// <summary>
        /// The Parameters of the Command
        /// </summary>
        public override String[] GetParameters()
        {
            return new String[]
            {
                "channel", "The channel where the message should be delivered to.",
                "private", "Whether the message should get delivered privately.",
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
            return Settings.Read<String>("control") + GetName() + " Thomas I like your bot";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (tells == null)
                tells = new SerializeableList<Msg>("tell");

            if (StartsWithParam("channel", message.Message))
            {
                String text = message.Message;
                String target = StripParam("channel", ref text);
                String[] split = text.Trim().Split(new Char[] { ' ' }, 2);
                foreach (String name in split[0].Split(','))
                {
                    String wildcard = "^" + Regex.Escape(name.Trim()).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
                    tells.Add(new Msg
                    {
                        channel = true,
                        channelName = target,
                        message = split[1],
                        pm = false,
                        source = message.IsChannelMessage && !QIRC.GetChannel(message.Source).secret ? message.Source : "Private",
                        time = DateTime.UtcNow,
                        to = wildcard,
                        user = message.User
                    });
                }
            }
            else
            {
                String text = message.Message;
                Boolean pm = StartsWithParam("private", text);
                StripParam("private", ref text);
                String[] split = text.Trim().Split(new Char[] { ' ' }, 2);
                foreach (String name in split[0].Split(','))
                {
                    String wildcard = "^" + Regex.Escape(name.Trim()).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
                    tells.Add(new Msg
                    {
                        channel = false,
                        channelName = "",
                        message = split[1],
                        pm = pm,
                        source = message.IsChannelMessage && !QIRC.GetChannel(message.Source).secret ? message.Source : "Private",
                        time = DateTime.UtcNow,
                        to = wildcard,
                        user = message.User
                    });
                }
            }
            QIRC.SendMessage(client, "I'll redirect this as soon as they are around.", message.User, message.Source);
        }
    
        /// <summary>
        /// Deliver the messages from the tell command
        /// </summary>
        public override void OnPrivateMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            if (tells == null)
                tells = new SerializeableList<Msg>("tell");
            List<Msg> toDelete = new List<Msg>();
            foreach (Msg tell in tells)
            {
                if (!Regex.IsMatch(e.PrivateMessage.User.Nick, tell.to, RegexOptions.IgnoreCase))
                    continue;
                if (tell.channel && tell.channelName != e.PrivateMessage.Source)
                    continue;
                String message = "[b]" + tell.user + "[/b] left a message for you in [b]" + tell.source + " [" + tell.time.ToString("dd.MM.yyyy HH:mm:ss") + "][/b]: \"" + tell.message + "\"";
                if (tell.pm)
                    QIRC.SendMessage(client, message, e.PrivateMessage.User.Nick, e.PrivateMessage.User.Nick, true);
                else
                    QIRC.SendMessage(client, message, e.PrivateMessage.User.Nick, e.PrivateMessage.Source);
                toDelete.Add(tell);
            }
            toDelete.ForEach(t => tells.Remove(t));
        }
    }
}
