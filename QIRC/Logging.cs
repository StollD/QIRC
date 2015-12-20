/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// QIRC
using QIRC.Constants;

/// System
using System;
using System.IO;

/// <summary>
/// The main namespace. Here's everything that executes actively.
/// </summary>
namespace QIRC
{
    /// <summary>
    /// Class to support logging to disk and CLI
    /// </summary>
    public class Logging
    {
        /// <summary>
        /// The logging levels
        /// </summary>
        public enum Level
        {
            DEBUG,
            INFO,
            WARNING,
            ERROR,
            SPECIAL,
            VERYSPECIAL
        }

        /// <summary>
        /// StreamWriter that writes into the log file
        /// </summary>
        protected static StreamWriter writer { get; set; }

        /// <summary>
        /// Gets the logging prefix based on the current time and the logging level
        /// </summary>
        /// <param name="level">The current logging level.</param>
        /// <returns>A string, that shows the logging level and the current time</returns>
        protected static String GetPrefix(Level level)
        {
            String levelName = Enum.GetName(typeof(Level), level);
            String date = DateTime.UtcNow.ToLongTimeString();
            return "[" + levelName + " " + date + "] ";
        }

        /// <summary>
        /// Returns the respective color for each logging Level
        /// </summary>
        protected static ConsoleColor GetColor(Level level)
        {
            if (level == Level.DEBUG) return ConsoleColor.DarkGreen;
            if (level == Level.ERROR) return ConsoleColor.DarkRed;
            if (level == Level.INFO) return ConsoleColor.Gray;
            if (level == Level.SPECIAL) return ConsoleColor.DarkCyan;
            if (level == Level.WARNING) return ConsoleColor.DarkYellow;
            return ConsoleColor.DarkMagenta;
        }

        /// <summary>
        /// Writes a text into the main logfile and into the Command Line Interface
        /// </summary>
        /// <param name="message">The message that should be logged.</param>
        /// <param name="level">The logging level.</param>
        public static void Log(Object message, Level level)
        {
            /// Get the logging prefix
            String prefix = GetPrefix(level);

            /// Write to disk
            writer.WriteLine(prefix + message);
            writer.Flush();

            /// Write to CLI
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = GetColor(level);
            Console.WriteLine(prefix + message);
            Console.ForegroundColor = old;
        }

        /// <summary>
        /// Initialise everything
        /// </summary>
        static Logging()
        {
            /// Get the logging path
            Directory.CreateDirectory(Paths.logs);

            /// Create the writer
            writer = new StreamWriter(Paths.logs + "latest.log");
        }
    }
}