/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */
 
using ChatSharp;
using ChatSharp.Events;
using Newtonsoft.Json;
using System;
using QIRC.Serialization;
using SQLite;

namespace QIRC.IRC
{
    /// <summary>
    /// The saveable definitions for an IRC message
    /// </summary>
    public class ProtoIrcMessage : Storage<ProtoIrcMessage>
    {
        /// <summary>
        /// The index of the message
        /// </summary>
        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public Int32 Index { get; set; }

        /// <summary>
        /// Whether the Message was sent in a channel or via. /query
        /// </summary>
        public Boolean IsChannelMessage { get; set; }

        /// <summary>
        /// The actual message
        /// </summary>
        public String Message { get; set; }

        /// <summary>
        /// The Source of the Message
        /// </summary>
        public String Source { get; set; }

        /// <summary>
        /// The User who has sent the message
        /// </summary>
        public String User { get; set; }

        /// <summary>
        /// The time when the message was sent.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// The access level of the user who sent the message
        /// </summary>
        public AccessLevel level { get; set; }

        /// <summary>
        /// Create the ProtoMessage
        /// </summary>
        public ProtoIrcMessage(PrivateMessage message)
        {
            IsChannelMessage = message.IsChannelMessage;
            Message = message.Message;
            Source = message.Source;
            User = message.User.Nick;
            Time = DateTime.UtcNow;
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
            IsChannelMessage = false;
            Message = e.Notice;
            Source = User = e.Source;
            Time = DateTime.UtcNow;
        }

        /// <summary>
        /// JsonConstructor
        /// </summary>
        [JsonConstructor]
        public ProtoIrcMessage()
        {
            IsChannelMessage = false;
            Message = Source = User = "";
            Time = DateTime.UtcNow;
        }
    }
}
