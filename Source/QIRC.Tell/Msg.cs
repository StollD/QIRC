/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;

namespace QIRC.Commands
{
    /// <summary>
    /// An object that represents a message that was left in the bot
    /// </summary>
    public struct Msg
    {
        public String to;
        public String source;
        public String user;
        public DateTime time;
        public String message;
        public Boolean pm;
        public Boolean channel;
        public String channelName;
    }
}