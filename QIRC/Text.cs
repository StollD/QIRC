/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// QIRC
using QIRC.Configuration;

/// System
using System;

/// <summary>
/// The main namespace. Here's everything that executes actively.
/// </summary>
namespace QIRC
{
    /// <summary>
    /// This is the main text storage. In QIRC, almost nothing is hardcoded, 
    /// wich means that the used messages are loaded from a settings file. 
    /// The initialization for that is here.
    /// </summary>
    public class Text
    {
        /// <summary>
        /// Creates the prefab for the loaded texts
        /// </summary>
        public static void Load()
        {
            SettingsFile file = new SettingsFile("texts");
            file.Add("logging__sayHello", ".NET Bot for Internet Relay Chat (IRC) - 2015");

            /// Inject the file
            Settings.AddFile(file);
        }
    }
}
