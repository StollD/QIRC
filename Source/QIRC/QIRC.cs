/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2016
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// The main namespace. Here's everything that executes actively.
/// </summary>
namespace QIRC
{
    /// <summary>
    /// This is the main Bot Controller. It loads the Bot and creates the <see cref="IrcClient"/>.
    /// After this is done, it is responsible for Comandline Access. 
    /// </summary>
    public class QIRC
    {
        /// <summary>
        /// The connection to the IRC Server. It handles the protocol
        /// implementation for us. Also, it gives us delegates for events.
        /// </summary>
        public static IrcClient client { get; set; }

        /// <summary>
        /// Whether there's an active IRC connection at the moment.
        /// </summary>
        public static Boolean isConnected { get; set; }

        /// <summary>
        /// Whether the Bot is still alive.
        /// </summary>
        public static Boolean isAlive { get; set; }

        /// <summary>
        /// The thread that manages the IRC Connection
        /// </summary>
        public static Thread ircThread { get; protected set; }
        /// <summary>
        /// All the messages that were recieved on IRC, stored as BSON
        /// </summary>
        public static SerializeableList<ProtoIrcMessage> messages = new SerializeableList<ProtoIrcMessage>("messages");

        /// <summary>
        /// This function is executed when the Program starts.
        /// Here we load everything we need and create the <see cref="IrcClient"/>
        /// </summary>
        /// <param name="args">Commandline arguments</param>
        public static void Main(String[] args)
        {
            /// We are alive
            isAlive = true;

            /// Load the settings of the Bot
            PluginManager.Load();
            PluginManager.Invoke("Load");
            Settings.Load();

            /// Call OnAwake
            PluginManager.Invoke("Awake");

            /// Connect to the IRC
            ircThread = new Thread(Connect);
            ircThread.Start();

            /// Command line handler goes here
            while (isAlive)
            {
                Console.Write("> ");
                String input = Console.ReadLine();
                HandleCommand(new ProtoIrcMessage()
                {
                    IsChannelMessage = false,
                    Message = input,
                    Source = Settings.Read<String>("name"),
                    User = Settings.Read<String>("name")
                }, client, true);
            }            
        }

        /// <summary>
        /// Creates the connection to the server and saves it into <see cref="client"/>
        /// It also sets various flags that are needed for the bot to function properly.
        /// </summary>
        public static void Connect()
        {
            /// Identification
            IrcUser user = new IrcUser(Settings.Read<String>("name"), Settings.Read<String>("name"), Settings.Read<String>("password"), "QIRC - A friendly IRC bot!");

            /// Grab settings
            String host = Settings.Read<String>("host");
            Int32 port = Settings.Read<Int32>("port");
            Boolean useSSL = Settings.Read<Boolean>("useSSL");

            /// Create the IrcClient
            client = new IrcClient(String.Join(":", host, port), user, useSSL);
            client.Encoding = Encoding.UTF8;
            client.Settings.ModeOnJoin = true;

            /// Add delegates
            client.ChannelListRecieved += ChannelListRecieved;
            client.ChannelMessageRecieved += ChannelMessageRecieved;
            client.ChannelTopicReceived += ChannelTopicReceived;
            client.ConnectionComplete += ConnectionComplete;
            client.ModeChanged += ModeChanged;
            client.MOTDPartRecieved += MOTDPartRecieved;
            client.MOTDRecieved += MOTDRecieved;
            client.NetworkError += NetworkError;
            client.NickChanged += NickChanged;
            client.NickInUse += NickInUse;
            client.NoticeRecieved += NoticeRecieved;
            client.PrivateMessageRecieved += PrivateMessageRecieved;
            client.RawMessageRecieved += RawMessageRecieved;
            client.RawMessageSent += RawMessageSent;
            client.ServerInfoRecieved += ServerInfoRecieved;
            client.UserJoinedChannel += UserJoinedChannel;
            client.UserKicked += UserKicked;
            client.UserMessageRecieved += UserMessageRecieved;
            client.UserPartedChannel += UserPartedChannel;
            client.UserQuit += UserQuit;
            client.WhoIsReceived += WhoIsReceived;

            /// Connect to IRC
            client.ConnectAsync();
            PluginManager.Invoke("Connect", host, port, client.User.Nick, useSSL);

            /// Set isConnected to false, which is funny since the function is named Connect, but hey!
            isConnected = false;
        }

        /// <summary>
        /// Channel List Event
        /// </summary>
        private static void ChannelListRecieved(Object sender, ChannelEventArgs e)
        {
            PluginManager.Invoke("ChannelListRecieved", client, e);
        }

        /// <summary>
        /// Channel Message Event
        /// </summary>
        private static void ChannelMessageRecieved(Object sender, PrivateMessageEventArgs e)
        {
            PluginManager.Invoke("ChannelMessageRecieved", client, e);
        }

        /// <summary>
        /// Channel Topic Event
        /// </summary>
        private static void ChannelTopicReceived(Object sender, ChannelTopicEventArgs e)
        {
            PluginManager.Invoke("ChannelTopicReceived", client, e);
        }

        /// <summary>
        /// Connection Complete Event
        /// </summary>
        private static void ConnectionComplete(Object sender, EventArgs e)
        {
            isConnected = true;
            PluginManager.Invoke("ConnectionComplete", client);

            /// Join Channels
            foreach (ProtoIrcChannel channel in Settings.Read<List<ProtoIrcChannel>>("channels"))
                JoinChannel(channel, false);
        }
        
        /// <summary>
        /// Mode Changed Event
        /// </summary>
        private static void ModeChanged(Object sender, ModeChangeEventArgs e)
        {
            PluginManager.Invoke("ModeChanged", client, e);
        }

        /// <summary>
        /// MOTD Part Recieved Event
        /// </summary>
        private static void MOTDPartRecieved(Object sender, ServerMOTDEventArgs e)
        {
            PluginManager.Invoke("MOTDPartRecieved", client, e);
        }

        /// <summary>
        /// MODT Recieved Event
        /// </summary>
        private static void MOTDRecieved(Object sender, ServerMOTDEventArgs e)
        {
            PluginManager.Invoke("MOTDRecieved", client, e);
        }

        /// <summary>
        /// Network Error Event
        /// </summary>
        private static void NetworkError(Object sender, SocketErrorEventArgs e)
        {
            PluginManager.Invoke("NetworkError", client, e);
        }

        /// <summary>
        /// Nick Changed Event
        /// </summary>
        private static void NickChanged(Object sender, NickChangedEventArgs e)
        {
            PluginManager.Invoke("NickChanged", client, e);
        }

        /// <summary>
        /// Nick in Use Event
        /// </summary>
        private static void NickInUse(Object sender, ErronousNickEventArgs e)
        {
            PluginManager.Invoke("NickInUse", client, e);
        }

        /// <summary>
        /// Notice Recieved Event
        /// </summary>
        private static void NoticeRecieved(Object sender, IrcNoticeEventArgs e)
        {
            PluginManager.Invoke("NoticeRecieved", client, e);
        }

        /// <summary>
        /// Private Message Event
        /// </summary>
        private static void PrivateMessageRecieved(Object sender, PrivateMessageEventArgs e)
        {
            /// Commands
            String control = Settings.Read<String>("control");
            ProtoIrcMessage msg = new ProtoIrcMessage(e);
            if (msg.Message.StartsWith(control))
            {
                HandleCommand(msg, client);
            }

            PluginManager.Invoke("PrivateMessageRecieved", client, e);
        }

        /// <summary>
        /// Raw Message Recieved Event
        /// </summary>
        private static void RawMessageRecieved(Object sender, RawMessageEventArgs e)
        {
            PluginManager.Invoke("RawMessageRecieved", client, e);
        }

        /// <summary>
        /// Raw Message Sent Event
        /// </summary>
        private static void RawMessageSent(Object sender, RawMessageEventArgs e)
        {
            PluginManager.Invoke("RawMessageSent", client, e);
        }

        /// <summary>
        /// Server Info Recieved Event
        /// </summary>
        private static void ServerInfoRecieved(Object sender, SupportsEventArgs e)
        {
            PluginManager.Invoke("ServerInfoRecieved", client, e);
        }

        /// <summary>
        /// User Joined Channel Event
        /// </summary>
        private static void UserJoinedChannel(Object sender, ChannelUserEventArgs e)
        {
            PluginManager.Invoke("UserJoinedChannel", client, e);
        }

        /// <summary>
        /// User Kicked Event
        /// </summary>
        private static void UserKicked(Object sender, KickEventArgs e)
        {
            if (e.Kicked.Nick == Settings.Read<String>("name"))
                LeaveChannel(e.Channel.Name);
            PluginManager.Invoke("UserKicked", client, e);
        }

        /// <summary>
        /// User Message Recieved Event
        /// </summary>
        private static void UserMessageRecieved(Object sender, PrivateMessageEventArgs e)
        {
            PluginManager.Invoke("UserMessageRecieved", client, e);
        }
        
        /// <summary>
        /// User Parted Channel Event
        /// </summary>
        private static void UserPartedChannel(Object sender, ChannelUserEventArgs e)
        {
            PluginManager.Invoke("UserPartedChannel", client, e);
        }

        /// <summary>
        /// User Quit Event
        /// </summary>
        private static void UserQuit(Object sender, UserEventArgs e)
        {
            PluginManager.Invoke("UserQuit", client, e);
        }

        /// <summary>
        /// WhoIs Recieved Event
        /// </summary>
        private static void WhoIsReceived(Object sender, WhoIsReceivedEventArgs e)
        {
            PluginManager.Invoke("WhoIsReceived", client, e);
        }

        /// <summary>
        /// Joins a channel on the IRC
        /// </summary>
        public static void JoinChannel(ProtoIrcChannel channel, Boolean addToCFG = true)
        {
            /// If we aren't connected, we cant join. Same if the channel is null
            if (!isConnected || channel == null)
                return;

            /// We don't need empty junk
            if (String.IsNullOrWhiteSpace(channel.name))
                return;
            
            /// Join
            String name = channel.name;
            String password = channel.password;
            if (String.IsNullOrWhiteSpace(password))
                client.JoinChannel(name);
            else
                client.JoinChannel(name + ":" + password);

            /// Add it to the cfg
            if (addToCFG)
            {
                List<ProtoIrcChannel> list = Settings.Read<List<ProtoIrcChannel>>("channels");
                list.Add(channel);
                Settings.Write("channels", list);
            }
        }

        /// <summary>
        /// Leaves a channel on the IRC
        /// </summary>
        public static void LeaveChannel(String channel, String reason = "")
        {
            /// If we aren't connected, we cant join.
            if (!isConnected)
                return;

            /// We don't need empty junk
            if (String.IsNullOrWhiteSpace(channel))
                return;

            /// Leave
            try
            {
                if (String.IsNullOrWhiteSpace(reason))
                    client.PartChannel(channel);
                else
                    client.PartChannel(channel, reason);
            }
            catch
            {

            }
            
            /// Edit the cfg
            List<ProtoIrcChannel> list = Settings.Read<List<ProtoIrcChannel>>("channels");
            list.RemoveAll(c => String.Equals(c.name, channel, StringComparison.InvariantCultureIgnoreCase));
            Settings.Write("channels", list);
        }

        /// <summary>
        /// Handles an incoming command
        /// </summary>
        public static void HandleCommand(ProtoIrcMessage message, IrcClient client, Boolean commandLine = false)
        {
            String control = Settings.Read<String>("control");
            message.Message = message.Message.Remove(0, control.Length);
            IrcUser user = client.Users[message.User];

            client.WhoIs(user.Nick, (WhoIs whoIs) =>
            {
                try
                {
                    foreach (IrcCommand command in PluginManager.commands)
                    {
                        String cmd = message.Message.Split(' ')[0];
                        if (String.Equals(command.GetName(), cmd, StringComparison.InvariantCultureIgnoreCase))
                        {
                            message.Message = message.Message.Remove(0, cmd.Length).Trim();
                            AccessLevel level = AccessLevel.NORMAL;
                            if (message.IsChannelMessage)
                            {
                                try
                                {
                                    IrcChannel channel = client.Channels[message.Source];
                                    if (user.ChannelModes[channel] == 'o' || user.ChannelModes[channel] == 'O')
                                        level = AccessLevel.OPERATOR;
                                    else if (user.ChannelModes[channel] == 'v' || user.ChannelModes[channel] == 'V')
                                        level = AccessLevel.VOICE;
                                }
                                catch (Exception e)
                                {

                                }
                            }
                            List<ProtoIrcAdmin> admins = Settings.Read<List<ProtoIrcAdmin>>("admins");
                            if (admins.Count(a => String.Equals(a.name, whoIs.LoggedInAs, StringComparison.InvariantCultureIgnoreCase)) == 1)
                            {
                                ProtoIrcAdmin admin = admins.FirstOrDefault(a => String.Equals(a.name, whoIs.LoggedInAs, StringComparison.InvariantCultureIgnoreCase));
                                if (admin.root)
                                    level = AccessLevel.ROOT;
                                else
                                    level = AccessLevel.ADMIN;
                            }
                            message.level = level;
                            if (CheckPermission(command.GetAccessLevel(), level) || commandLine)
                            {
                                if (message.IsChannelMessage)
                                {
                                    List<ProtoIrcChannel> channels = Settings.Read<List<ProtoIrcChannel>>("channels");
                                    ProtoIrcChannel channel = channels.FirstOrDefault(c => String.Equals(c.name, message.Source));
                                    if (channel.serious && !command.IsSerious()) return;
                                }
                                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                                try
                                {
                                    command.RunCommand(client, message);
                                }
                                catch (Exception e)
                                {
                                    SendMessage(client, e.Message, message.User, message.Source);
                                }
                            }
                            else
                                SendMessage(client, "You don't have the permission to use this command! Only " + command.GetAccessLevel() + " can use this command! You are " + level + ".", message.User, message.Source);
                            break;
                        }
                    }
                }
                catch
                {
                    SendMessage(client, "ChatSharp broke. Please contact your local doctor.", message.User, message.Source);
                }
            });            
        }     

        /// <summary>
        /// Sends a message to the IRC
        /// </summary>
        public static ProtoIrcMessage SendMessage(IrcClient client, string message, string from, string to, bool noname = false)
        {
            if (String.IsNullOrWhiteSpace(message))
                return new ProtoIrcMessage();
            message = Formatter.Format(message);
            String[] splits = SplitInParts(message, 400).ToArray();

            if (!to.StartsWith("#")) to = from;
            for (Int32 j = 0; j < splits.Length; j++)
            {
                try
                {
                    if (j == 0 && !noname)
                        client.SendMessage(from + ": " + splits[j], to);
                    else
                        client.SendMessage(splits[j], to);
                }
                catch (Exception e)
                {
                    client.SendMessage(from + ": " + e.Message, to);
                }
            }
            ProtoIrcMessage proto = new ProtoIrcMessage()
            {
                IsChannelMessage = to.StartsWith("#"),
                Message = message,
                Source = to.StartsWith("#") ? to : client.User.Nick,
                User = client.User.Nick
            };
            PluginManager.Invoke("MessageSent", client, proto);
            return proto;
        }

        /// <summary>
        /// Splits a string into multiple chunks
        /// </summary>
        protected static IEnumerable<String> SplitInParts(String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (Int32 i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        /// <summary>
        /// Sends an action to the IRC
        /// </summary>
        public static ProtoIrcMessage SendAction(IrcClient client, string message, string to)
        {
            message = Formatter.Format(message);
            client.SendAction(message, to);
            ProtoIrcMessage proto = new ProtoIrcMessage()
            {
                IsChannelMessage = to.StartsWith("#"),
                Message = "ACTION" + message,
                Source = to.StartsWith("#") ? to : client.User.Nick,
                User = client.User.Nick
            };
            PluginManager.Invoke("MessageSent", client, proto);
            return proto;
        }

        /// <summary>
        /// Checks if two Access Levels are compatible
        /// </summary>
        public static bool CheckPermission(AccessLevel required, AccessLevel given)
        {
            if (required == AccessLevel.NORMAL)
                return true;
            else if (required == AccessLevel.VOICE)
                return given == AccessLevel.VOICE || given == AccessLevel.OPERATOR || given == AccessLevel.ADMIN || given == AccessLevel.ROOT;
            else if (required == AccessLevel.OPERATOR)
                return given == AccessLevel.OPERATOR || given == AccessLevel.ADMIN || given == AccessLevel.ROOT;
            else if (required == AccessLevel.ADMIN)
                return given == AccessLevel.ADMIN || given == AccessLevel.ROOT;
            else if (required == AccessLevel.ROOT)
                return given == AccessLevel.ROOT;
            return false;
        }
    }

    /// <summary>
    /// The various access levels
    /// </summary>
    public enum AccessLevel
    {
        NORMAL,
        VOICE,
        OPERATOR,
        ADMIN,
        ROOT
    }
}
