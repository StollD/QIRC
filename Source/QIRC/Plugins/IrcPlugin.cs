/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using ChatSharp;
using ChatSharp.Events;
using QIRC.IRC;
using System;

namespace QIRC.Plugins
{
    /// <summary>
    /// This is an abstract base for a QIRC Plugin
    /// </summary>
    public abstract class IrcPlugin
    {
        /// IrcClient Functions
        public virtual void OnLoad() { }
        public virtual void OnAwake() { }
        public virtual void OnConnect(String host, Int32 port, String nick, Boolean useSSL) { }
        public virtual void OnChannelListRecieved(IrcClient client, ChannelEventArgs e) { }
        public virtual void OnChannelMessageRecieved(IrcClient client, PrivateMessageEventArgs e) { }
        public virtual void OnChannelTopicReceived(IrcClient client, ChannelTopicEventArgs e) { }
        public virtual void OnConnectionComplete(IrcClient client) { }
        public virtual void OnModeChanged(IrcClient client, ModeChangeEventArgs e) { }
        public virtual void OnMOTDPartRecieved(IrcClient client, ServerMOTDEventArgs e) { }
        public virtual void OnMOTDRecieved(IrcClient client, ServerMOTDEventArgs e) { }
        public virtual void OnNetworkError(IrcClient client, SocketErrorEventArgs e) { }
        public virtual void OnNickChanged(IrcClient client, NickChangedEventArgs e) { }
        public virtual void OnNickInUse(IrcClient client, ErronousNickEventArgs e) { }
        public virtual void OnNoticeRecieved(IrcClient client, IrcNoticeEventArgs e) { }
        public virtual void OnPrivateMessageRecieved(IrcClient client, PrivateMessageEventArgs e) { }
        public virtual void OnRawMessageRecieved(IrcClient client, RawMessageEventArgs e) { }
        public virtual void OnRawMessageSent(IrcClient client, RawMessageEventArgs e) { }
        public virtual void OnServerInfoRecieved(IrcClient client, SupportsEventArgs e) { }
        public virtual void OnUserJoinedChannel(IrcClient client, ChannelUserEventArgs e) { }
        public virtual void OnUserKicked(IrcClient client, KickEventArgs e) { }
        public virtual void OnUserMessageRecieved(IrcClient client, PrivateMessageEventArgs e) { }
        public virtual void OnUserPartedChannel(IrcClient client, ChannelUserEventArgs e) { }
        public virtual void OnUserQuit(IrcClient client, UserEventArgs e) { }
        public virtual void OnWhoIsReceived(IrcClient client, WhoIsReceivedEventArgs e) { }
        public virtual void OnMessageSent(IrcClient client, ProtoIrcMessage message) { }
    }
}

