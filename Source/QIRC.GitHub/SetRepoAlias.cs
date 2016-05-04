/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;
using System.Collections.Generic;
using System.Linq;
using ChatSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;

namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the github tool suite. The command can set the default repository
    /// for the channel and the IrcPlugin is resposible for posting Links to issues and so on
    /// </summary>
    public class GitHubAlias : IrcCommand
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
            return "setrepoalias";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Sets an alias for the given repository";
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
            return Settings.Read<String>("control") + GetName() + " QIRC ThomasKerman/QIRC";
        }

        public static SerializeableList<KeyValuePair<String, String>> alias { get; set; }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (alias == null)
                alias = new SerializeableList<KeyValuePair<String, String>>("repoalias");
            if (!message.IsChannelMessage)
                return;
            if (alias.Count(r => r.Key == message.Source) == 0)
                alias.Add(new KeyValuePair<String, String>(message.Message.Split(' ')[0].Trim(), message.Message.Split(' ')[1].Trim()));
            else
                alias[alias.IndexOf(alias.First(r => r.Key == message.Message.Split(' ')[0].Trim()))] = new KeyValuePair<String, String>(message.Message.Split(' ')[0].Trim(), message.Message.Split(' ')[1].Trim());
            QIRC.SendMessage(client, "Set alias for https://github.com/" + message.Message.Split(' ')[1].Trim() + "/ to " + message.Message.Split(' ')[0].Trim(), message.User, message.Source);
        }
    }
}