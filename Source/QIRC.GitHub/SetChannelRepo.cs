/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using System;
using ChatSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;

namespace QIRC.GitHub
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
        
        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (!message.IsChannelMessage)
                return;
            if (ChannelRepo.Query.Count(r => r.Channel == message.Source) == 0)
                ChannelRepo.Query.Insert(message.Source, message.Message.Trim());
            else
            {
                ChannelRepo repo = ChannelRepo.Query.First(r => r.Channel == message.Source);
                repo.Repository = message.Message.Trim();
                BotController.Database.Update(repo);
            }
            BotController.SendMessage(client, "Set default repository for " + message.Source + " to " + message.Message.Trim(), message.User, message.Source);
        }
    }
}
