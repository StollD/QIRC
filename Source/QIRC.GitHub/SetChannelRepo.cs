/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using ChatSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;
using System;
using System.Linq;
using System.Collections.Generic;

namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the github tool suite. The command can set the default repository
    /// for the channel and the IrcPlugin is resposible for posting Links to issues and so on
    /// </summary>
    public class GitHubRepo : IrcCommand
    {
        /// <summary>
        /// The Access Level that is needed to execute the command
        /// </summary>
        public override AccessLevel GetAccessLevel()
        {
            return AccessLevel.OPERATOR;
        }

        /// <summary>
        /// The name of the command
        /// </summary>
        public override String GetName()
        {
            return "setchannelrepo";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Sets the default GitHub repository for this channel.";
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
            return Settings.Read<String>("control") + GetName() + " ThomasKerman/QIRC";
        }

        public static SerializeableList<KeyValuePair<String, String>> repos { get; set; }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (repos == null)
                repos = new SerializeableList<KeyValuePair<String, String>>("repos");
            if (!message.IsChannelMessage)
                return;
            if (repos.Count(r => r.Key == message.Source) == 0)
                repos.Add(new KeyValuePair<String, String>(message.Source, message.Message.Trim()));
            else
                repos[repos.IndexOf(repos.First(r => r.Key == message.Source))] = new KeyValuePair<String, String>(message.Source, message.Message.Trim());
            QIRC.SendMessage(client, "Set default repository for " + message.Source + " to " + message.Message.Trim(), message.User, message.Source);
        }
    }
}
