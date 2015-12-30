/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// IRC
using ChatSharp;
using ChatSharp.Events;

/// QIRC
using QIRC.Constants;

/// System
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

/// <summary>
/// The namespace where everything Plugin related is stored.
/// </summary>
namespace QIRC.Plugins
{
    /// <summary>
    /// This class loads and manages plugins for QIRC
    /// </summary>
    public class PluginManager
    {
        /// <summary>
        /// All loaded <see cref="IrcPlugin"/>
        /// </summary>
        public static HashSet<IrcPlugin> plugins { get; set; }

        /// <summary>
        /// All loaded <see cref="IrcCommand"/>
        /// </summary>
        public static HashSet<IrcCommand> commands { get; set; }

        /// <summary>
        /// All loaded <see cref="Assembly"/>
        /// </summary>
        public static HashSet<Assembly> assemblies { get; set; }

        /// <summary>
        /// Loads the <see cref="Assembly"/> at the given Path
        /// </summary>
        public static Assembly LoadAssembly(Path path)
        {
            if (!path.Exists())
                throw new ArgumentException("The path \"" + path + "\" doesn't exist!", nameof(path));
            Byte[] buffer = File.ReadAllBytes(path);
            Assembly assembly = AppDomain.CurrentDomain.Load(buffer);
            assemblies.Add(assembly);
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(IrcPlugin)))
                {
                    IrcPlugin plugin = (IrcPlugin)Activator.CreateInstance(type);
                    plugins.Add(plugin);
                }
                else if (type.IsSubclassOf(typeof(IrcCommand)))
                {
                    IrcCommand command = (IrcCommand)Activator.CreateInstance(type);
                    commands.Add(command);
                }
            }
            return assembly;
        }

        /// <summary>
        /// Loads the .dll files from the Plugins directory
        /// </summary>
        public static void Load()
        {
            foreach (String file in Directory.GetFiles(Paths.plugins, "*.dll"))
                LoadAssembly(file);
        }

        /// <summary>
        /// Calls OnChannelListReceived in the IrcPlugins
        /// </summary>
        public static void Invoke(String name, params Object[] arguments)
        {
            MethodInfo info = typeof(IrcPlugin).GetMethod("On" + name);
            foreach (IrcPlugin plugin in plugins)
                info.Invoke(plugin, arguments);
        }

        /// <summary>
        /// Initialization
        /// </summary>
        static PluginManager()
        {
            assemblies = new HashSet<Assembly>();
            plugins = new HashSet<IrcPlugin>();
            commands = new HashSet<IrcCommand>();
            Directory.CreateDirectory(Paths.plugins);
        }
    }
}
