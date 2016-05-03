/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using ChatSharp;
using ChatSharp.Events;
using QIRC.IRC;
using QIRC.Plugins;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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
        public const String regex = @"(?:(\S+)[:,]\s+)?s/((?:\\/|[^/])+)/((?:\\/|[^/])*)(?:/(\S+))?";

        /// <summary>
        /// This gets invoked when someone wrote something in a channel
        /// </summary>
        public override void OnChannelMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            // Create a new message 
            ProtoIrcMessage message = new ProtoIrcMessage(e);

            // If there is no match, we have nothing to do
            if (!Regex.IsMatch(message.Message, regex, RegexOptions.IgnoreCase))
                return;

            // Get the values from the message
            Match match = Regex.Match(message.Message, regex, RegexOptions.IgnoreCase);
            String nick = match.Groups[1].Success ? match.Groups[1].Value : message.User;
            Console.WriteLine(nick);
            if (!client.Users.Contains(nick))
                return;
            String find = match.Groups[2].Value.Replace(@"\/", "/");
            String repl = match.Groups[3].Value.Replace(@"\/", "/");

            // Find the message to edit
            ProtoIrcMessage new_msg = QIRC.messages.LastOrDefault(m => m.User == nick && Regex.IsMatch(m.Message, find, RegexOptions.IgnoreCase));
            Char[] flags = match.Groups[4].Success ? match.Groups[4].Value.ToCharArray() : new Char[0];
            Console.WriteLine(new string(flags));
            if (new_msg == null)
                return;
            if (new_msg.Message.StartsWith("ACTION"))
                new_msg.Message = "/me" + new_msg.Message.Remove(0, "ACTION".Length - 1);

            // Regex options
            RegexOptions options = flags.Contains('i') || flags.Contains('I') ? RegexOptions.IgnoreCase : RegexOptions.None;
            Regex rFind = new Regex(find, options);

            // Replace stuff
            new_msg.Message = flags.Contains('g') || flags.Contains('G') ? Regex.Replace(new_msg.Message, find, repl, options) : rFind.Replace(new_msg.Message, repl, 1);

            // Send the message back
            if (nick == message.User)
                QIRC.SendMessage(client, nick + " [b]meant[/b] to say: " + new_msg.Message, message.User, message.Source, true);
            else if (!client.Channels[message.Source].Users.Contains("Kountdown"))
                QIRC.SendMessage(client, message.User + " thinks " + nick + " [b]meant[/b] to say: " + new_msg.Message, message.User, message.Source, true);
        }
    }
}
