/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using ChatSharp;
using ChatSharp.Events;
using QIRC.IRC;
using QIRC.Plugins;
using System;

namespace QIRC.Logger
{
    /// <summary>
    /// This is the main logging module. In QIRC, almost nothing is hardcoded, 
    /// wich means that the used messages are loaded from a settings file. 
    /// The initialization for that is here.
    /// </summary>
    public class Log : IrcPlugin
    {        
        /// <summary>
        /// Initial Logging
        /// </summary>
        public override void OnAwake()
        {
            Logging.Log(".NET Bot for Internet Relay Chat (IRC) - 2016", Logging.Level.SPECIAL);
        }

        /// <summary>
        /// Logs when the connection to IRC is getting created
        /// </summary>
        public override void OnConnect(String host, Int32 port, String nick, Boolean useSSL)
        {
            Logging.Log($"Connecting to IRC: {host}:{port} as user {nick}. {(useSSL ? "Using SSL." : "")}", Logging.Level.INFO);
        }

        /// <summary>
        /// Logs when the Connection is established
        /// </summary>
        public override void OnConnectionComplete(IrcClient client)
        {
            Logging.Log($"Connection established! Network name: {client.ServerInfo.NetworkName}", Logging.Level.SPECIAL);
        }

        /// <summary>
        /// Logs when a message was sent in a channel
        /// </summary>
        public override void OnChannelMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            ProtoIrcMessage msg = new ProtoIrcMessage(e);
            Logging.Log(String.Format("[{1}] <{0}> {2}", msg.User, msg.Source, msg.Message), Logging.Level.INFO);
            QIRC.messages.Add(msg);
        }

        /// <summary>
        /// Logs when the Bot has got a private message 
        /// </summary>
        public override void OnUserMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            ProtoIrcMessage msg = new ProtoIrcMessage(e);
            Logging.Log(String.Format("[{1}] <{0}> {2}", msg.User, "Private", msg.Message), Logging.Level.INFO);
            QIRC.messages.Add(msg);
        }

        /// <summary>
        /// Logs when the Bot has recieved a Notice
        /// </summary>
        public override void OnNoticeRecieved(IrcClient client, IrcNoticeEventArgs e)
        {
            ProtoIrcMessage msg = new ProtoIrcMessage(e);
            Logging.Log($"Notice from {msg.User.Split('!')[0]}: {msg.Message}", Logging.Level.WARNING);
            QIRC.messages.Add(msg);
        }

        /// <summary>
        /// Logs when someone changes his nick
        /// </summary>
        public override void OnNickChanged(IrcClient client, NickChangedEventArgs e)
        {
            Logging.Log($"{e.OldNick} has changed his Nick to {e.NewNick}", Logging.Level.INFO);
        }

        /// <summary>
        /// Logs when the Message of the Day was recieved
        /// </summary>
        public override void OnMOTDRecieved(IrcClient client, ServerMOTDEventArgs e)
        {
            Logging.Log("Recieved Message of the Day", Logging.Level.INFO);
        }

        /// <summary>
        /// Logs when the Network errors out
        /// </summary>
        public override void OnNetworkError(IrcClient client, SocketErrorEventArgs e)
        {
            Logging.Log($"Network Error! Reason: {e.SocketError}", Logging.Level.ERROR);
        }

        /// <summary>
        /// Logs when a nick is in use
        /// </summary>
        public override void OnNickInUse(IrcClient client, ErronousNickEventArgs e)
        {
            Logging.Log($"Someone has tried to change his nick to {e.InvalidNick}! This is invalid. New Nick: {e.NewNick}", Logging.Level.WARNING);
        }

        /// <summary>
        /// Logs when a user has joined a channel
        /// </summary>
        public override void OnUserJoinedChannel(IrcClient client, ChannelUserEventArgs e)
        {
            Logging.Log($"{e.User.Nick} has joined {e.Channel.Name}", Logging.Level.WARNING);
        }

        /// <summary>
        /// Logs when a User got kicked
        /// </summary>
        public override void OnUserKicked(IrcClient client, KickEventArgs e)
        {
            Logging.Log($"{e.Kicked.Nick} got kicked from {e.Channel.Name} by {e.Kicker.Nick}. Reason: {e.Reason}", Logging.Level.WARNING);
        }

        /// <summary>
        /// Logs when a user parted
        /// </summary>
        public override void OnUserPartedChannel(IrcClient client, ChannelUserEventArgs e)
        {
            Logging.Log($"{e.User.Nick} has parted {e.Channel.Name}", Logging.Level.WARNING);
        }

        /// <summary>
        /// Logs when a user quit from the Network
        /// </summary>
        public override void OnUserQuit(IrcClient client, UserEventArgs e)
        {
            Logging.Log($"{e.User.Nick} has quit.", Logging.Level.ERROR);
        }

        /// <summary>
        /// Logs when a message was sent
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public override void OnMessageSent(IrcClient client, ProtoIrcMessage message)
        {
            Logging.Log(String.Format("[{1}] <{0}> {2}", message.User, message.IsChannelMessage ? message.Source : "Private", message.Message), Logging.Level.INFO);
            QIRC.messages.Add(message);
        }
    }
}
