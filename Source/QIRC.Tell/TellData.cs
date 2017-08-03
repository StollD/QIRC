/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using System;
using QIRC.Serialization;
using SQLite;

namespace QIRC.Tell
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