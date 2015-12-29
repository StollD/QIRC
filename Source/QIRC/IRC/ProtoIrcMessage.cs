/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// IRC
using ChatSharp;
using ChatSharp.Events;

/// JSON
using Newtonsoft.Json;

/// System
using System;

/// <summary>
/// In this namespace, everything related to loading IRC Stuff
/// </summary>
namespace QIRC.IRC
{
    /// <summary>
    /// The saveable definitions for an IRC message
    /// </summary>
    public class ProtoIrcMessage
    {
        /// <summary>
        /// Whether the Message was sent in a channel or via. /query
        /// </summary>
        public Boolean IsChannelMessage { get; }

        /// <summary>
        /// The actual message
        /// </summary>
        public String Message { get; }

        /// <summary>
        /// The Source of the Message
        /// </summary>
        public String Source { get; }

        /// <summary>
        /// The User who has sent the message
        /// </summary>
        public String User { get; }

        /// <summary>
        /// Create the ProtoMessage
        /// </summary>
        public ProtoIrcMessage(PrivateMessage message)
        {
            IsChannelMessage = message.IsChannelMessage;
            Message = message.Message;
            Source = message.Source;
            User = message.User.Nick;
        }

        /// <summary>
        /// Create the ProtoMessage
        /// </summary>
        public ProtoIrcMessage(PrivateMessageEventArgs e) : this(e.PrivateMessage) { }

        /// <summary>
        /// Create the ProtoMessage
        /// </summary>
        public ProtoIrcMessage(IrcNoticeEventArgs e)
        {
            IsChannelMessage = true;
            Message = e.Notice;
            Source = User = e.Source;
        }

        /// <summary>
        /// JsonConstructor
        /// </summary>
        [JsonConstructor]
        internal ProtoIrcMessage()
        {
            IsChannelMessage = false;
            Message = Source = User = "";
        }
    }
}
