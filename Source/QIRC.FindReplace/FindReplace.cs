/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2016
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// IRC
using ChatSharp;
using ChatSharp.Events;

/// QIRC
using QIRC;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;

/// System
using System;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Here's everything that logs to disk and CMD
/// </summary>
namespace QIRC.Addons
{
    /// <summary>
    /// This is the implementation for the s/text/replacement module. Search the messages of the user
    /// for the given text (regex) and replace it with the given replacement.
    /// </summary>
    public class FindReplace : IrcPlugin
    {
        /// <summary>
        /// The regular expression that is used to detect the syntax (Copyright goes to the Willie/Sopel Devs)
        /// </summary>
        public const String regex = @"^(?:(\S+)[:]\s+)?s/((?:[^/])+)/((?:[^/])*)(?:/)?$";

        /// <summary>
        /// This gets invoked when someone wrote something in a channel
        /// </summary>
        public override void OnChannelMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            ProtoIrcMessage message = new ProtoIrcMessage(e);
            if (!Regex.IsMatch(message.Message, regex, RegexOptions.IgnoreCase))
                return;
            Match match = Regex.Match(message.Message, regex, RegexOptions.IgnoreCase);
            String nick = match.Groups[1].Success ? match.Groups[1].Value : message.User;                
            if (!client.Users.Contains(nick))
                return;
            String find = match.Groups[2].Value.Replace(@"\/", "/");
            String repl = match.Groups[3].Value.Replace(@"\/", "/");
            ProtoIrcMessage new_msg = QIRC.messages.Where(m => m.User == nick && Regex.IsMatch(m.Message, find, RegexOptions.IgnoreCase)).LastOrDefault();
            if (new_msg == null)
                return;
            if (new_msg.Message.StartsWith("ACTION"))
                new_msg.Message = "/me" + new_msg.Message.Remove(0, "ACTION".Length - 1);
            new_msg.Message = Regex.Replace(new_msg.Message, find, repl, RegexOptions.IgnoreCase);
            if (nick == message.User)
                QIRC.SendMessage(client, nick + " [b]meant[/b] to say: " + new_msg.Message, message.User, message.Source, true);
            else
                QIRC.SendMessage(client, message.User + " thinks " + nick + " [b]meant[/b] to say: " + new_msg.Message, message.User, message.Source, true);
        }
    }
}
