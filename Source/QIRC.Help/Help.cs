/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using System;
using System.Collections.Generic;
using System.Linq;
using ChatSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;

namespace QIRC.Help
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
        /// An example for using the command.
        /// </summary>
        /// <returns></returns>
        public override String GetExample()
        {
            return Settings.Read<String>("control") + GetName() + " <command>";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (String.IsNullOrWhiteSpace(message.Message))
            {
                List<String> commands = PluginManager.commands.Select(c => c.GetName()).ToList();
                commands.AddRange(PluginManager.commands.SelectMany(s => s.GetAlternativeNames()));
                BotController.SendMessage(client, "Commands I recognize: " + String.Join(", ", commands.OrderBy(x => x)), message.User, message.User, true);
                BotController.SendMessage(client, "For additional help type \"" + Settings.Read<String>("control") + "help <command>\" where <command> is the name of the command you want help for.", message.User, message.User, true);
                if (message.IsChannelMessage)
                    BotController.SendMessage(client, "I sent you a private message with information about all my commands!", message.User, message.Source);
            }
            else
            {
                String name = message.Message;
                IrcCommand command = PluginManager.commands.FirstOrDefault(c => c.IsNamed(name));
                if (command != null)
                {
                    String description = command.GetDescription();
                    BotController.SendMessage(client, name + ": " + (String.IsNullOrWhiteSpace(description) ? "No description available." : description), message.User, message.Source, true);
                    String[] parameters = new String[command.GetParameters().Length / 2];
                    for (Int32 i = 0; i < parameters.Length * 2; i += 2)
                    {
                        String param = command.GetParameters()[i];
                        String descr = command.GetParameters()[i + 1];
                        parameters[i == 0 ? 0 : i / 2] += "-" + param + " (" + descr + ")";
                    }
                    if (parameters.Length != 0)
                        BotController.SendMessage(client, "parameters: " + String.Join(", ", parameters), message.User, message.Source, true);
                    if (!String.IsNullOrWhiteSpace(command.GetExample()))
                        BotController.SendMessage(client, "example: " + command.GetExample(), message.User, message.Source, true);
                }
            }
        }
    }
}
