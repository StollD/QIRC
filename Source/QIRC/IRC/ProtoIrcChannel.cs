/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;

namespace QIRC.IRC
{
    /// <summary>
    /// The loadable definitions for an IRC channel
    /// </summary>
    public class ProtoIrcChannel
    {
        /// <summary>
        /// The name of the IRC channel. Must be prefixed with #
        /// </summary>
        public String name { get; set; }

        /// <summary>
        /// The password of the IRC channel. If there's no password, this is ""
        /// </summary>
        public String password { get; set; } = "";

        /// <summary>
        /// Whether this channel is a serious one
        /// </summary>
        public Boolean serious { get; set; }
    }
}
