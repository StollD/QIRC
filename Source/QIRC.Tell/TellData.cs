/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;
using System.IO.IsolatedStorage;
using QIRC.Serialization;
using SQLite;

namespace QIRC.Commands
{
    /// <summary>
    /// An object that represents a message that was left in the bot
    /// </summary>
    public class TellData : Storage<TellData>
    {
        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public Int32 Index { get; set; }
        public String to { get; set; }
        public String source { get; set; }
        public String user { get; set; }
        public DateTime time { get; set; }
        public String message { get; set; }
        public Boolean pm { get; set; }
        public Boolean channel { get; set; }
        public String channelName { get; set; }

        public TellData() { }
    }
}