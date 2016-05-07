﻿/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;

namespace QIRC.Commands
{
    /// <summary>
    /// Alias for the google command.
    /// </summary>
    public class GoogleShort : Google
    {
        public override String GetName()
        {
            return "g";
        }
        public override void OnLoad() { }
    }
}