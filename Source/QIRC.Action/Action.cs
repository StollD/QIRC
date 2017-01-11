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

namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the action command. The bot will grab the words
    /// behind the control sequence and output it as an action. Pretty basic
    /// </summary>
    public class Action : IrcCommand
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
            return "action";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Outputs the given text to the given channel as an Action.";
        }

        /// <summary>
        /// The Parameters of the Command
        /// </summary>
        public override String[] GetParameters()
        {
            return new []
            {
                "to", "The Channel where the bot should send the action to."
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
            return Settings.Read<String>("control") + GetName() + " runs away.";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (StartsWithParam("to", message.Message))
            {
                String text = message.Message;
                String target = StripParam("to", ref text);
                BotController.SendAction(client, text, target);
            }
            else
            {
                BotController.SendAction(client, message.Message, message.Source);
            }
        }
    }
}
