/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// IRC
using ChatSharp;

/// QIRC
using QIRC.IRC;

/// System
using System;

/// <summary>
/// The namespace where everything Plugin related is stored.
/// </summary>
namespace QIRC.Plugins
{
    /// <summary>
    /// This is an abstract base for a QIRC Command
    /// </summary>
    public abstract class IrcCommand
    {
        public virtual String GetName() { return ""; }
        public virtual String GetDescription() { return ""; }
        public virtual Boolean IsSerious() { return false; }
        public virtual AccessLevel GetAccessLevel() { return AccessLevel.NORMAL; }
        public virtual void RunCommand(IrcClient client, ProtoIrcMessage message) { }
        public virtual String[] GetParameters() { return new String[0]; }
        public virtual String GetExample() { return ""; }
        protected Boolean StartsWithParam(String param, String message) { return message.StartsWith("-" + param + "=") || message.StartsWith("-" + param + ":"); }
        protected String StripParam(String param, String message) { return message.Replace("-" + param + "=", "").Replace("-" + param + ":", "").Replace("-" + param, ""); }
    }
}

