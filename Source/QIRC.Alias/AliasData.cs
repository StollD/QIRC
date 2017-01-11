/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;
using QIRC.Configuration;
using QIRC.Serialization;
using SQLite;

namespace QIRC.Commands
{
    /// <summary>
    /// Contains all data for one command alias
    /// </summary>
    public class AliasData : Storage<AliasData>
    {
        [PrimaryKey, Unique, NotNull]
        public String Name { get; set; }

        public String Description { get; set; }
        public AccessLevel Level { get; set; }
        public Boolean Serious { get; set; }
        public String Example { get; set; }

        [NotNull]
        public String Command { get; set; }

        public String Regex { get; set; }
        public Boolean Escape { get; set; }

        public AliasData() { }

        public AliasData(String name, String command, String regex, Boolean escape)
        {
            Name = name;
            Level = AccessLevel.NORMAL;
            Serious = true;
            Example = Settings.Read<String>("control") + name;
            Command = command;
            Regex = regex;
            Escape = escape;
        }
    }
}