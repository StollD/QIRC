/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using QIRC.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;

namespace QIRC.Plugins
{
    /// <summary>
    /// This class loads and manages plugins for QIRC
    /// </summary>
    public class PluginManager
    {
        /// <summary>
        /// Logging
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            Assembly assembly = Assembly.LoadFrom(path);
            assemblies.Add(assembly);
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(IrcPlugin)) && !type.IsSubclassOf(typeof(IrcCommand)))
                {
                    IrcPlugin plugin = (IrcPlugin)Activator.CreateInstance(type);
                    plugins.Add(plugin);
                }
                if (type.IsSubclassOf(typeof(IrcCommand)))
                {
                    IrcCommand command = (IrcCommand)Activator.CreateInstance(type);
                    plugins.Add(command);
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
        /// Calls a method in the IrcPlugins
        /// </summary>
        public static void Invoke(String name, params Object[] arguments)
        {
            try
            {
                MethodInfo info = typeof(IrcPlugin).GetMethod("On" + name);
                foreach (IrcPlugin plugin in plugins)
                    info.Invoke(plugin, arguments);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
            }
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
