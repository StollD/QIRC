/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// IRC
using ChatSharp;
using ChatSharp.Events;

/// QIRC
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;

/// System
using System;

/// <summary>
/// Here's everything that logs to disk and CMD
/// </summary>
namespace QIRC.Logging
{
    /// <summary>
    /// This is the main logging module. In QIRC, almost nothing is hardcoded, 
    /// wich means that the used messages are loaded from a settings file. 
    /// The initialization for that is here.
    /// </summary>
    public class Log : IrcPlugin
    {        
        /// <summary>
        /// Creates the prefab for the loaded texts
        /// </summary>
        public override void OnLoad()
        {
            SettingsFile file = new SettingsFile("texts");
            file.Add("logging__sayHello", ".NET Bot for Internet Relay Chat (IRC) - 2015");
            file.Add("logging__onConnect", "Connecting to IRC: {0}:{1} as user {2}. {3}");
            file.Add("logging__modtRecieved", "Recieved Message of the Day");
            file.Add("logging__onConnected", "Connection established! Network name: {0}");
            file.Add("logging__messageRecieved", "[{1}] <{0}> {2}");
            file.Add("logging__noticeRecieved", "Notice from {0}: {1}");
            file.Add("logging__nickChanged", "{0} has changed his Nick to {1}");
            file.Add("logging__networkError", "Network Error! Reason: {0}");
            file.Add("logging__invalidNick", "Someone has tried to change his nick to {0}! This is invalid. New Nick: {1}");
            file.Add("logging__userJoined", "{0} has joined {1}");
            file.Add("logging__userKicked", "{0} got kicked from {1} by {2}. Reason: {3}");
            file.Add("logging__userParted", "{0} has parted {1}");
            file.Add("logging__userQuit", "{0} has quit.");

            /// Inject the file
            Settings.AddFile(file);
        }

        /// <summary>
        /// Initial Logging
        /// </summary>
        public override void OnAwake()
        {
            Logging.Log(Settings.Read<String>("logging__sayHello"), Logging.Level.SPECIAL);
        }

        /// <summary>
        /// Logs when the connection to IRC is getting created
        /// </summary>
        public override void OnConnect(String host, Int32 port, String nick, Boolean useSSL)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__onConnect"), host, port, nick, useSSL ? "Using SSL." : ""), Logging.Level.INFO);
        }

        /// <summary>
        /// Logs when the Connection is established
        /// </summary>
        public override void OnConnectionComplete(IrcClient client)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__onConnected"), client.ServerInfo.NetworkName), Logging.Level.SPECIAL);
        }

        /// <summary>
        /// Logs when a message was sent in a channel
        /// </summary>
        public override void OnChannelMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            ProtoIrcMessage msg = new ProtoIrcMessage(e);
            Logging.Log(String.Format(Settings.Read<String>("logging__messageRecieved"), msg.User, msg.Source, msg.Message), Logging.Level.INFO);
            QIRC.messages.Add(msg);
        }

        /// <summary>
        /// Logs when the Bot has got a private message 
        /// </summary>
        public override void OnUserMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            ProtoIrcMessage msg = new ProtoIrcMessage(e);
            Logging.Log(String.Format(Settings.Read<String>("logging__messageRecieved"), msg.User, "Private", msg.Message), Logging.Level.INFO);
            QIRC.messages.Add(msg);
        }

        /// <summary>
        /// Logs when the Bot has recieved a Notice
        /// </summary>
        public override void OnNoticeRecieved(IrcClient client, IrcNoticeEventArgs e)
        {
            ProtoIrcMessage msg = new ProtoIrcMessage(e);
            Logging.Log(String.Format(Settings.Read<String>("logging__noticeRecieved"), msg.User.Split('!')[0], msg.Message), Logging.Level.WARNING);
            QIRC.messages.Add(msg);
        }

        /// <summary>
        /// Logs when someone changes his nick
        /// </summary>
        public override void OnNickChanged(IrcClient client, NickChangedEventArgs e)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__nickChanged"), e.OldNick, e.NewNick), Logging.Level.INFO);
        }

        /// <summary>
        /// Logs when the Message of the Day was recieved
        /// </summary>
        public override void OnMOTDRecieved(IrcClient client, ServerMOTDEventArgs e)
        {
            Logging.Log(Settings.Read<String>("logging__modtRecieved"), Logging.Level.INFO);
        }

        /// <summary>
        /// Logs when the Network errors out
        /// </summary>
        public override void OnNetworkError(IrcClient client, SocketErrorEventArgs e)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__networkError"), e.SocketError), Logging.Level.ERROR);
        }

        /// <summary>
        /// Logs when a nick is in use
        /// </summary>
        public override void OnNickInUse(IrcClient client, ErronousNickEventArgs e)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__invalidNick"), e.InvalidNick, e.NewNick), Logging.Level.WARNING);
        }

        /// <summary>
        /// Logs when a user has joined a channel
        /// </summary>
        public override void OnUserJoinedChannel(IrcClient client, ChannelUserEventArgs e)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__userJoined"), e.User.Nick, e.Channel.Name), Logging.Level.WARNING);
        }

        /// <summary>
        /// Logs when a User got kicked
        /// </summary>
        public override void OnUserKicked(IrcClient client, KickEventArgs e)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__userKicked"), e.Kicked.Nick, e.Channel.Name, e.Kicker.Nick, e.Reason), Logging.Level.WARNING);
        }

        /// <summary>
        /// Logs when a user parted
        /// </summary>
        public override void OnUserPartedChannel(IrcClient client, ChannelUserEventArgs e)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__userParted"), e.User.Nick, e.Channel.Name), Logging.Level.WARNING);
        }

        /// <summary>
        /// Logs when a user quit from the Network
        /// </summary>
        public override void OnUserQuit(IrcClient client, UserEventArgs e)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__userQuit"), e.User.Nick), Logging.Level.ERROR);
        }

        /// <summary>
        /// Logs when a message was sent
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public override void OnMessageSent(IrcClient client, ProtoIrcMessage message)
        {
            Logging.Log(String.Format(Settings.Read<String>("logging__messageRecieved"), message.User, message.IsChannelMessage ? message.Source : "Private", message.Message), Logging.Level.INFO);
            QIRC.messages.Add(message);
        }
    }
}
