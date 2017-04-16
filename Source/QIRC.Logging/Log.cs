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
using System.Reflection;
using log4net;

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
        /// Logging
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initial Logging
        /// </summary>
        public override void OnAwake()
        {
            log.Info(".NET Bot for Internet Relay Chat (IRC) - 2016");
        }

        /// <summary>
        /// Logs when the connection to IRC is getting created
        /// </summary>
        public override void OnConnect(String host, Int32 port, String nick, Boolean useSSL)
        {
            log.Info($"Connecting to IRC: {host}:{port} as user {nick}. {(useSSL ? "Using SSL." : "")}");
        }

        /// <summary>
        /// Logs when the Connection is established
        /// </summary>
        public override void OnConnectionComplete(IrcClient client)
        {
            log.Info($"Connection established! Network name: {client.ServerInfo.NetworkName}");
        }

        /// <summary>
        /// Logs when a message was sent in a channel
        /// </summary>
        public override void OnChannelMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            ProtoIrcMessage msg = new ProtoIrcMessage(e);
            log.Info($"[{msg.Source}] <{msg.User}> {msg.Message}");
            ProtoIrcMessage.Query.Connection.Insert(msg);
        }

        /// <summary>
        /// Logs when the Bot has got a private message 
        /// </summary>
        public override void OnUserMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            ProtoIrcMessage msg = new ProtoIrcMessage(e);
            log.Info(String.Format("[{1}] <{0}> {2}", msg.User, "Private", msg.Message));
            ProtoIrcMessage.Query.Connection.Insert(msg);
        }

        /// <summary>
        /// Logs when the Bot has recieved a Notice
        /// </summary>
        public override void OnNoticeRecieved(IrcClient client, IrcNoticeEventArgs e)
        {
            ProtoIrcMessage msg = new ProtoIrcMessage(e);
            log.Info($"Notice from {msg.User.Split('!')[0]}: {msg.Message}");
            ProtoIrcMessage.Query.Connection.Insert(msg);
        }

        /// <summary>
        /// Logs when someone changes his nick
        /// </summary>
        public override void OnNickChanged(IrcClient client, NickChangedEventArgs e)
        {
            log.Info($"{e.OldNick} has changed his Nick to {e.NewNick}");
        }

        /// <summary>
        /// Logs when the Message of the Day was recieved
        /// </summary>
        public override void OnMOTDRecieved(IrcClient client, ServerMOTDEventArgs e)
        {
            log.Info("Recieved Message of the Day");
        }

        /// <summary>
        /// Logs when the Network errors out
        /// </summary>
        public override void OnNetworkError(IrcClient client, SocketErrorEventArgs e)
        {
            log.Info($"Network Error! Reason: {e.SocketError}");
        }

        /// <summary>
        /// Logs when a nick is in use
        /// </summary>
        public override void OnNickInUse(IrcClient client, ErronousNickEventArgs e)
        {
            log.Info($"Someone has tried to change his nick to {e.InvalidNick}! This is invalid. New Nick: {e.NewNick}");
        }

        /// <summary>
        /// Logs when a user has joined a channel
        /// </summary>
        public override void OnUserJoinedChannel(IrcClient client, ChannelUserEventArgs e)
        {
            log.Info($"{e.User.Nick} has joined {e.Channel.Name}");
        }

        /// <summary>
        /// Logs when a User got kicked
        /// </summary>
        public override void OnUserKicked(IrcClient client, KickEventArgs e)
        {
            log.Info($"{e.Kicked.Nick} got kicked from {e.Channel.Name} by {e.Kicker.Nick}. Reason: {e.Reason}");
        }

        /// <summary>
        /// Logs when a user parted
        /// </summary>
        public override void OnUserPartedChannel(IrcClient client, ChannelUserEventArgs e)
        {
            log.Info($"{e.User.Nick} has parted {e.Channel.Name}");
        }

        /// <summary>
        /// Logs when a user quit from the Network
        /// </summary>
        public override void OnUserQuit(IrcClient client, UserEventArgs e)
        {
            log.Info($"{e.User.Nick} has quit.");
        }

        /// <summary>
        /// Logs when a message was sent
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public override void OnMessageSent(IrcClient client, ProtoIrcMessage message)
        {
            log.Info($"[{(message.IsChannelMessage ? message.Source : "Private")}] <{message.User}> {message.Message}");
            ProtoIrcMessage.Query.Connection.Insert(message);
        }
    }
}
