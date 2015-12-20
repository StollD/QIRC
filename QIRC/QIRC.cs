/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// IRC
using IrcDotNet;

/// QIRC
using QIRC.Configuration;

/// System
using System;
using System.Threading;

/// <summary>
/// The main namespace. Here's everything that executes actively.
/// </summary>
namespace QIRC
{
    /// <summary>
    /// This is the main Bot Controller. It loads the Bot and creates the <see cref="IrcClient"/>.
    /// After this is done, it is responsible for Comandline Access. 
    /// The <see cref="IrcClient"/> is managed in a different <see cref="Thread"/>.
    /// </summary>
    public class QIRC
    {
        /// <summary>
        /// This function is executed when the Program starts.
        /// Here we load everything we need and create the <see cref="IrcClient"/>
        /// </summary>
        /// <param name="args">Commandline arguments</param>
        public static void Main(String[] args)
        {
            
        }
    }
}
