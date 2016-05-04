/** 
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
    public class WolframLong : WolframAlpha
    {
        public override String GetName()
        {
            return "wolfram";
        }
    }
}