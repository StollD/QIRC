/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using System;
using QIRC.Serialization;
using SQLite;

namespace QIRC.GitHub
{
    /// <summary>
    /// An alias for a github repository
    /// </summary>
    public class ChannelRepo : Storage<ChannelRepo>
    {
        [PrimaryKey, NotNull]
        public String Channel { get; set; }

        [NotNull]
        public String Repository { get; set; }

        public ChannelRepo() { }

        public ChannelRepo(String chan, String repo)
        {
            Channel = chan;
            Repository = repo;
        }
    }
}