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
    /// An alias for a github repository
    /// </summary>
    public class RepoAlias : Storage<RepoAlias>
    {
        [PrimaryKey, NotNull]
        public String Alias { get; set; }

        [NotNull]
        public String Repository { get; set; }

        public RepoAlias() { }

        public RepoAlias(String alias, String repo)
        {
            Alias = alias;
            Repository = repo;
        }
    }
}