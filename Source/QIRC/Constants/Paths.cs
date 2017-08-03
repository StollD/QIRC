/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */
 
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

        /// <summary>
        /// The Directory where the persistent data is stored.
        /// This is relative to the location of the main executable.
        /// </summary>
        public static readonly Path data = "storage.db";
    }
}
