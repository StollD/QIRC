/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// System
using System;
using System.IO;
using PathIO = System.IO.Path;

/// <summary>
/// In this namespace, everything is hardcoded.
/// Here are paths and internal definitions stored.
/// </summary>
namespace QIRC.Constants
{
    /// <summary>
    /// Constant definitions for paths used by the bot
    /// </summary>
    public class Paths
    {
        /// <summary>
        /// The directory where the settings files are stored.
        /// This is relative to the location of the main executable.
        /// </summary>
        public static readonly Path settings = "settings/";

        /// <summary>
        /// The directory where the logfiles are stored.
        /// This is relative to the location of the main executable.
        /// </summary>
        public static readonly Path logs = "logs/";

        /// <summary>
        /// The directory where the plugins are stored.
        /// This is relative to the location of the main executable.
        /// </summary>
        public static readonly Path plugins = "plugins/";
    }
}
