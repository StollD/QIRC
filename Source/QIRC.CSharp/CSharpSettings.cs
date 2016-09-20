/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;

namespace QIRC.Commands
{
    /// <summary>
    /// A class that stores settings for the C# Evaluator
    /// </summary>
    public class CSharpSettings
    {
        /// <summary>
        /// Instead of sending it via IRC, the bot puts the state on hastebin and sends that
        /// </summary>
        public Boolean useHastebin = false;
    }
}