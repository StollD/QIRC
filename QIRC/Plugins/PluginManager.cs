/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

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
        /// Calls OnLoad in the IrcPlugins
        /// </summary>
        public static void OnLoad()
        {
            foreach (IrcPlugin plugin in plugins)
                plugin.OnLoad();
        }

        /// <summary>
        /// Calls OnAwake in the IrcPlugins
        /// </summary>
        public static void OnAwake()
        {
            foreach (IrcPlugin plugin in plugins)
                plugin.OnAwake();
        }

        /// <summary>
        /// Calls OnConnect in the IrcPlugins
        /// </summary>
        public static void OnConnect(String host, Int32 port, String nick, Boolean useSSL)
        {
            foreach (IrcPlugin plugin in plugins)
                plugin.OnConnect(host, port, nick, useSSL);
        }

        /// <summary>
        /// Initialization
        /// </summary>
        static PluginManager()
        {
            assemblies = new HashSet<Assembly>();
            plugins = new HashSet<IrcPlugin>();
            Directory.CreateDirectory(Paths.plugins);
        }
    }
}
