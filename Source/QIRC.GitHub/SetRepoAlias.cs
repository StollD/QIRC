/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
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

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            String[] split = message.Message.Split(' ');
            String split0 = split[0];
            String split1 = split[1];
            if (RepoAlias.Query.Count(r => r.Alias == split0) == 0)
                RepoAlias.Query.Insert(split0, split1);
            else
            {
                RepoAlias alias = RepoAlias.Query.First(r => r.Alias == split0);
                alias.Repository = split[1];
                BotController.Database.Update(alias);
            }
            BotController.SendMessage(client, "Set alias for https://github.com/" + split[1].Trim() + "/ to " + split[0].Trim(), message.User, message.Source);
        }
    }
}