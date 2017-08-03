/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using System;
using System.Linq;
using ChatSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;

namespace QIRC.Modules
{
    /// <summary>
    /// This is the implementation for the modules command. The bot will load / unload
    /// the given command into it's runtime
    /// </summary>
    public class Modules : IrcCommand
    {
        /// <summary>
        /// The Access Level that is needed to execute the command
        /// </summary>
        public override AccessLevel GetAccessLevel()
        {
            return AccessLevel.ROOT;
        }

        /// <summary>
        /// The name of the command
        /// </summary>
        public override String GetName()
        {
            return "modules";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Loads or unloads Commands.";
        }

        /// <summary>
        /// The Parameters of the Command
        /// </summary>
        public override String[] GetParameters()
        {
            return new String[]
            {
                "load", "Loads the given Command into the bots runtime.",
                "unload", "Unloads the given Command into the bots runtime."
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
            return Settings.Read<String>("control") + GetName() + " -unload:modules";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            // Load a module
            if (StartsWithParam("load", message.Message))
            {
                String text = message.Message;
                String module = StripParam("load", ref text);
                if (PluginManager.commands.Count(c => c.GetType().Name == module) > 0 || PluginManager.plugins.Count(p => p.GetType().Name == module) > 0)
                {
                    BotController.SendMessage(client, "This module is already loaded.", message.User, message.Source);
                    return;
                }
                Type[] types = PluginManager.assemblies.SelectMany(a => a.GetTypes()).ToArray();
                if (types.Count(t => t.Name == module) == 0)
                {
                    BotController.SendMessage(client, "This module doesn't exist.", message.User, message.Source);
                    return;
                }
                Type type = types.First(t => t.Name == module);
                if (type.IsSubclassOf(typeof(IrcPlugin)))
                {
                    IrcPlugin plugin = (IrcPlugin)Activator.CreateInstance(type);
                    PluginManager.plugins.Add(plugin);
                    plugin.OnAwake();
                    plugin.OnLoad();
                }
                if (type.IsSubclassOf(typeof(IrcCommand)))
                {
                    IrcCommand command = (IrcCommand)Activator.CreateInstance(type);
                    PluginManager.commands.Add(command);
                }
                BotController.SendMessage(client, "Loaded the module \"" + module + "\"", message.User, message.Source);
            }

            // Unload a module
            if (StartsWithParam("unload", message.Message))
            {
                String text = message.Message;
                String module = StripParam("unload", ref text);
                if (PluginManager.commands.Count(c => c.GetType().Name == module) == 0 && PluginManager.plugins.Count(p => p.GetType().Name == module) == 0)
                {
                    BotController.SendMessage(client, "This module is already unloaded.", message.User, message.Source);
                    return;
                }
                Type[] types = PluginManager.assemblies.SelectMany(a => a.GetTypes()).ToArray();
                if (types.Count(t => t.Name == module) == 0)
                {
                    BotController.SendMessage(client, "This module doesn't exist.", message.User, message.Source);
                    return;
                }
                Type type = types.First(t => t.Name == module);
                if (type.IsSubclassOf(typeof(IrcPlugin)))
                {
                    PluginManager.plugins.RemoveWhere(p => p.GetType() == type);
                }
                if (type.IsSubclassOf(typeof(IrcCommand)))
                {
                    PluginManager.commands.RemoveWhere(p => p.GetType() == type);
                }
                BotController.SendMessage(client, "Unloaded the module \"" + module + "\"", message.User, message.Source);
            }
        }
    }
}
