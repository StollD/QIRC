/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// IRC
using ChatSharp;
using ChatSharp.Events;

/// QIRC
using QIRC;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;

/// System
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Here's everything that is an IrcCommand
/// </summary>
namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the join command. It tells the bot to join the given channel.
    /// The command requires Admin Access
    /// </summary>
    public class Join : IrcCommand
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
            return "join";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Makes the bot join the given channel.";
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
            return Settings.Read<String>("control") + GetName() + " #botwar";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (String.IsNullOrWhiteSpace(message.Message))
            {
                QIRC.SendAction(client, "tries to unlock the tremendous energy of the vacuum", message.Source);
            }
            else if(!message.Message.StartsWith("#"))
            {
                QIRC.SendMessage(client, "Invalid channel name!", message.User, message.Source);
            }
            else if (Settings.Read<List<ProtoIrcChannel>>("channels").Count(c => c.name == message.Message) > 0)
            {
                QIRC.SendMessage(client, "I am already active in " + message.Message + ".", message.User, message.Source);
            }
            else 
            {
                String channel = message.Message;
                ProtoIrcChannel proto = new ProtoIrcChannel() { name = channel, password = "", serious = true };
                QIRC.JoinChannel(proto);
                QIRC.SendMessage(client, "I have joined " + channel + "!", message.User, message.Source);
            }
        }
    }
}
