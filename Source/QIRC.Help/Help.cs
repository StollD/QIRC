﻿/// --------------------------------------
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
    /// This is the implementation for the help command. It displays every available command
    /// and provides short descriptions. It also explains parameters
    /// </summary>
    public class Help : IrcCommand
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
            return "help";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Provides a list of all available commands plus short descriptions for them.";
        }

        /// <summary>
        /// Whether the command can be used in serious channels.
        /// </summary>
        public override Boolean IsSerious()
        {
            return true;
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (String.IsNullOrWhiteSpace(message.Message))
            {
                QIRC.SendMessage(client, "Commands I recognize: " + String.Join(", ", PluginManager.commands.Select(c => c.GetName()).OrderBy(x => x)), message.User, message.User, true);
                QIRC.SendMessage(client, "For additional help type \"" + Settings.Read<String>("control") + "help <command>\" where <command> is the name of the command you want help for.", message.User, message.User, true);
                if (message.IsChannelMessage)
                    QIRC.SendMessage(client, "I sent you a private message with information about all my commands!", message.User, message.Source);
            }
            else
            {
                String name = message.Message;
                IrcCommand command = PluginManager.commands.FirstOrDefault(c => c.GetName() == name);
                if (command != null)
                {
                    String description = command.GetDescription();
                    QIRC.SendMessage(client, name + ": " + (String.IsNullOrWhiteSpace(description) ? "No description available." : description), message.User, message.Source, true);
                    String[] parameters = new String[command.GetParameters().Length / 2];
                    for (Int32 i = 0; i < parameters.Length * 2; i += 2)
                    {
                        String param = command.GetParameters()[i];
                        String descr = command.GetParameters()[i + 1];
                        parameters[i == 0 ? 0 : i / 2] += "-" + param + " (" + descr + ")";
                    }
                    if (parameters.Length != 0)
                        QIRC.SendMessage(client, "parameters: " + String.Join(", ", parameters), message.User, message.Source, true);
                }
            }
        }
    }
}