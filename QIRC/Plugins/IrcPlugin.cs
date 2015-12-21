/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// IRC
using IrcDotNet;

/// System
using System;

/// <summary>
/// The namespace where everything Plugin related is stored.
/// </summary>
namespace QIRC.Plugins
{
    /// <summary>
    /// This is an abstract base for a QIRC Plugin
    /// </summary>
    public abstract class IrcPlugin
    {
        /// <summary>
        /// The IRC client that manages our connection
        /// </summary>
        protected StandardIrcClient client
        {
            get { return QIRC.client; }
            set { QIRC.client = value; }
        }

        /// Virtual Functions
        public virtual void OnLoad() { }
        public virtual void OnAwake() { }
        public virtual void OnConnect(String host, Int32 port, String nick, Boolean useSSL) { }
        public virtual void OnChannelListReceived(IrcChannelListReceivedEventArgs e) { }
    }
}

