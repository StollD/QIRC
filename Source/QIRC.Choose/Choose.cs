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
using System.Linq;

namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the help command. It displays every available command
    /// and provides short descriptions. It also explains parameters
    /// </summary>
    public class Choose : IrcCommand
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
            return "choose";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Picks one option from 2 or more different things.";
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
            return Settings.Read<String>("control") + GetName() + " coffee|tea";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (String.IsNullOrWhiteSpace(message.Message))
            {
                BotController.SendMessage(client, "You have to submit at least two options!", message.User, message.Source);
            }
            else
            {
                String[] options = message.Message.Split('|', '/', '\\', ';', ',').Select(s => s.Trim()).ToArray();
                BotController.SendMessage(client, "Your options are: " + String.Join(", ", options) + ". My choice: " + options[new Random(options.GetHashCode()).Next(0, options.Length)], message.User, message.Source);
            }
        }
    }
}
