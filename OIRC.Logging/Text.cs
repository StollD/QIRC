/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// QIRC
using QIRC.Configuration;
using QIRC.Plugins;

/// System
using System;

/// <summary>
/// Here's everything that logs to disk and CMD
/// </summary>
namespace QIRC.Logging
{
    /// <summary>
    /// This is the main logging module. In QIRC, almost nothing is hardcoded, 
    /// wich means that the used messages are loaded from a settings file. 
    /// The initialization for that is here.
    /// </summary>
    public class Log : IrcPlugin
    {
        /// <summary>
        /// Creates the prefab for the loaded texts
        /// </summary>
        public override void OnLoad()
        {
            SettingsFile file = new SettingsFile("texts");
            file.Add("logging__sayHello", ".NET Bot for Internet Relay Chat (IRC) - 2015");
            file.Add("logging__onConnect", "Connecting to IRC: {0}:{1} as user {2}. {4}");

            /// Inject the file
            Settings.AddFile(file);
        }

        /// <summary>
        /// Initial Logging
        /// </summary>
        public override void OnAwake()
        {
            Logging.Log(Settings.Read<String>("logging__sayHello"), Logging.Level.SPECIAL);
        }

        /// <summary>
        /// Logs when the connection to IRC is getting created
        /// </summary>
        public override void OnConnect(String host, Int32 port, String nick, Boolean useSSL)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__onConnect"), host, port, nick, useSSL ? "Using SSL." : ""), Logging.Level.INFO);
        }
    }
}
