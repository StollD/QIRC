/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// System
using System;

/// <summary>
/// The namespace where everything Plugin related is stored.
/// </summary>
namespace QIRC.Plugins
{
    /// <summary>
    /// This is an abstract base for a QIRC Plugin
    /// </summary>
    public abstract class IrcPlugin
    {
        public abstract String GetName();
        public abstract String GetInfo();
        public abstract String GetVersion();
    }
}

