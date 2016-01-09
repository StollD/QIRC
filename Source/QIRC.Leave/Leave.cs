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
using System.Linq;

/// <summary>
/// Here's everything that is an IrcCommand
/// </summary>
namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the leave command. It tells the bot to leave a channel.
    /// Only ADMINs can use this
    /// </summary>
    public class Leave : IrcCommand
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
            return "leave";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Makes the bot leave the given channel.";
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
                if (message.IsChannelMessage)
                {
                    QIRC.SendMessage(client, "I will leave this channel now.", message.User, message.Source);
                    QIRC.LeaveChannel(message.Source);
                }
                else
                {
                    QIRC.SendMessage(client, "You have to submit a channel name in a private message!", message.User, message.Source);
                }
            }
            else
            {
                if (!message.Message.StartsWith("#"))
                {
                    QIRC.SendMessage(client, "Invalid channel name!", message.User, message.Source);
                }
                else
                {
                    QIRC.SendMessage(client, "I will leave the channel " + message.Message + " now.", message.User, message.Source);
                    QIRC.LeaveChannel(message.Message);
                }
            }
        }
    }
}
