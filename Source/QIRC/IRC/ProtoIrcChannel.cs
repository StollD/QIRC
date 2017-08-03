/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
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

        /// <summary>
        /// Whether this channel is private
        /// </summary>
        public Boolean secret { get; set; }

        public override Boolean Equals(Object obj)
        {
            if (!(obj is ProtoIrcChannel))
                return false;
            return String.Equals(name, ((ProtoIrcChannel) obj).name, StringComparison.InvariantCultureIgnoreCase);
        }

        public override Int32 GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}
