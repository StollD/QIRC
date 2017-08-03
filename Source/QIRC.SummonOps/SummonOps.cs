/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using ChatSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QIRC.SummonOps
{
    /// <summary>
    /// A command that pings all the operators in the current channel, if two persons call it.
    /// </summary>
    public class SummonOps : IrcCommand
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
            return "ops";
        }

        /// <summary>
        /// Alternative names
        /// </summary>
        public override String[] GetAlternativeNames()
        {
            return new[] { "summonops" };
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Summons all channel ops by spitting out a line with their names in it. THIS COMMAND SHOULD ONLY BE USED IN EMERGENCIES IF YOU DON'T WANT TO BE SMOTE FOR SPAMMING.";
        }

        /// <summary>
        /// The Parameters of the Command
        /// </summary>
        public override String[] GetParameters()
        {
            return new String[0];
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
            return Settings.Read<String>("control") + GetName();
        }

        /// <summary>
        /// The memory where we store the channel name and the time when the command was entered
        /// </summary>
        public static Dictionary<String, Tuple<String, DateTime>> memory = new Dictionary<String, Tuple<String, DateTime>>();

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            // Is this a channel message?
            if (!message.IsChannelMessage)
            {
                BotController.SendMessage(client, "This command is pointless in private.", message.User, message.Source);
                return;
            }

            // Do we know this channel?
            if (!memory.ContainsKey(message.Source))
            {
                memory.Add(message.Source, new Tuple<String, DateTime>(message.User, new DateTime(0)));
            }
            if ((DateTime.UtcNow - memory[message.Source].Item2).TotalSeconds >= Settings.Read<Int32>("summonopsApprovalTime"))
            {
                BotController.SendMessage(client, "Are you sure? If this truly is an emergency, then another person on the channel must type this command within the next " + Settings.Read<Int32>("summonopsApprovalTime") + " seconds.", message.User, message.Source);
                memory[message.Source] = new Tuple<String, DateTime>(message.User, DateTime.UtcNow);
            } 
            else
            {
                Tuple<String, DateTime> previous = memory[message.Source];
                if ((DateTime.UtcNow - previous.Item2).TotalSeconds < Settings.Read<Int32>("summonopsApprovalTime"))
                {
                    if (previous.Item1 == message.User)
                    {
                        BotController.SendMessage(client, "An other person must confirm this, not you.", message.User, message.Source);
                        return;
                    }
                    else
                    {
                        // This is an emergency
                        List<String> ops = new List<String>();
                        foreach (IrcUser user in client.Channels[message.Source].Users)
                        {
                            IrcChannel channel = client.Channels[message.Source];
                            if (user.ChannelModes[channel] == 'o' || user.ChannelModes[channel] == 'O')
                            {
                                ops.Add(user.Nick);
                            }
                        }
                        BotController.SendMessage(client, "Hailing all ops! " + String.Join(", ", ops) + "! " + previous.Item1 + " and " + message.User + " request your attention.", message.User, message.Source, true);
                        memory[message.Source] = new Tuple<String, DateTime>(message.User, new DateTime(0));
                    }
                }
            }
        }

        /// <summary>
        /// Adds the Settings to the config
        /// </summary>
        public override void OnLoad()
        {
            SettingsFile file = null;
            Settings.GetFile("settings", ref file);
            file.Add("summonopsApprovalTime", 15);
        }
    }
}
