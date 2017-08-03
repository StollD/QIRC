/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using System;
using QIRC.Configuration;
using QIRC.Serialization;
using SQLite;

namespace QIRC.Commands
{
    /// <summary>
    /// A C# Expression
    /// </summary>
    public class IgnoreData : Storage<IgnoreData>
    {
        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public Int32 Index { get; set; }

        [NotNull]
        public String Host { get; set; }

        public IgnoreData() { }

        public IgnoreData(String host)
        {
            Host = host;
        }
    }
}