/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// IRC
using IrcDotNet;

/// QIRC
using QIRC.Configuration;
using QIRC.Plugins;

/// System
using System;
using System.Threading;

/// <summary>
/// The main namespace. Here's everything that executes actively.
/// </summary>
namespace QIRC
{
    /// <summary>
    /// This is the main Bot Controller. It loads the Bot and creates the <see cref="IrcClient"/>.
    /// After this is done, it is responsible for Comandline Access. 
    /// The <see cref="IrcClient"/> is managed in a different <see cref="Thread"/>.
    /// </summary>
    public class QIRC
    {
        /// <summary>
        /// The connection to the IRC Server. It handles the protocol
        /// implementation for us. Also, it gives us delegates for events.
        /// </summary>
        public static StandardIrcClient client { get; set; }

        /// <summary>
        /// Whether there's an active IRC connection at the moment.
        /// </summary>
        public static Boolean isConnected { get; set; }

        /// <summary>
        /// Whether the Bot is still alive.
        /// </summary>
        public static Boolean isAlive { get; set; }

        /// <summary>
        /// This function is executed when the Program starts.
        /// Here we load everything we need and create the <see cref="IrcClient"/>
        /// </summary>
        /// <param name="args">Commandline arguments</param>
        public static void Main(String[] args)
        {
            /// Load the settings of the Bot
            PluginManager.Load();
            PluginManager.OnLoad();
            Settings.Load();

            /// Call OnAwake
            PluginManager.OnAwake();

            /// Connect to the IRC
            Connect();

            while (true) ;
        }

        /// <summary>
        /// Creates the connection to the server and saves it into <see cref="client"/>
        /// It also sets various flags that are needed for the bot to function properly.
        /// </summary>
        public static void Connect()
        {
            /// Create the client
            client = new StandardIrcClient();
            client.FloodPreventer = new IrcStandardFloodPreventer(Settings.Read<Int32>("messageBurst"), Settings.Read<Int64>("counterPeriod"));

            /// Identification
            IrcUserRegistrationInfo info = new IrcUserRegistrationInfo()
            {
                NickName = Settings.Read<String>("name"),
                Password = Settings.Read<String>("password"),
                RealName = "QIRC - A friendly IRC bot!",
                UserName = Settings.Read<String>("name")
            };

            /// Grab settings
            String host = Settings.Read<String>("host");
            Int32 port = Settings.Read<Int32>("port");
            Boolean useSSL = Settings.Read<Boolean>("useSSL");

            /// Add delegates
            client.ChannelListReceived += ChannelListReceived;
            client.ClientInfoReceived += ClientInfoReceived;
            client.Connected += Connected;
            client.ConnectFailed += ConnectFailed;
            client.Disconnected += Disconnected;
            client.Error += Error;
            client.ErrorMessageReceived += ErrorMessageReceived;
            client.MotdReceived += MotdReceived;
            client.NetworkInformationReceived += NetworkInformationReceived;
            client.PingReceived += PingReceived;
            client.PongReceived += PongReceived;
            client.ProtocolError += ProtocolError;
            client.RawMessageReceived += RawMessageReceived;
            client.RawMessageSent += RawMessageSent;
            client.Registered += Registered;
            client.ServerBounce += ServerBounce;
            client.ServerLinksListReceived += ServerLinksListReceived;
            client.ServerStatsReceived += ServerStatsReceived;
            client.ServerSupportedFeaturesReceived += ServerSupportedFeaturesReceived;
            client.ServerTimeReceived += ServerTimeReceived;
            client.ServerVersionInfoReceived += ServerVersionInfoReceived;
            client.ValidateSslCertificate += ValidateSslCertificate;
            client.WhoIsReplyReceived += WhoIsReplyReceived;
            client.WhoReplyReceived += WhoReplyReceived;
            client.WhoWasReplyReceived += WhoWasReplyReceived;

            /// Connect to IRC
            client.Connect(host, port, useSSL, info);
            PluginManager.OnConnect(host, port, info.NickName, useSSL);

            /// Set isConnected to false, which is funny since the function is named Connect, but hey!
            isConnected = false;
        }

        /// <summary>
        /// Channel List Received Event
        /// </summary>
        private static void ChannelListReceived(Object sender, IrcChannelListReceivedEventArgs e)
        {
            
        }

        private static void ClientInfoReceived(Object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Connected(Object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ConnectFailed(Object sender, IrcErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Disconnected(Object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Error(Object sender, IrcErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ErrorMessageReceived(Object sender, IrcErrorMessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void MotdReceived(Object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void NetworkInformationReceived(Object sender, IrcCommentEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void PingReceived(Object sender, IrcPingOrPongReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void PongReceived(Object sender, IrcPingOrPongReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ProtocolError(Object sender, IrcProtocolErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void RawMessageReceived(Object sender, IrcRawMessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void RawMessageSent(Object sender, IrcRawMessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Registered(Object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ServerBounce(Object sender, IrcServerInfoEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ServerLinksListReceived(Object sender, IrcServerLinksListReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ServerStatsReceived(Object sender, IrcServerStatsReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ServerSupportedFeaturesReceived(Object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ServerTimeReceived(Object sender, IrcServerTimeEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ServerVersionInfoReceived(Object sender, IrcServerVersionInfoEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ValidateSslCertificate(Object sender, IrcValidateSslCertificateEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void WhoIsReplyReceived(Object sender, IrcUserEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void WhoReplyReceived(Object sender, IrcNameEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void WhoWasReplyReceived(Object sender, IrcUserEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
