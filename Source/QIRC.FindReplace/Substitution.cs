/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;
using System.Linq;
using System.Text.RegularExpressions;
using ChatSharp;
using ChatSharp.Events;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;

namespace QIRC.Substitute
{
    /// <summary>
    /// This is the implementation for the s/text/replacement module. Search the messages of the user
    /// for the given text (regex) and replace it with the given replacement.
    /// </summary>
    public class Substitution : IrcPlugin
    {
        /// <summary>
        /// The regular expression that is used to detect the syntax (Copyright goes to the Willie/Sopel Devs)
        /// </summary>
        public const String regex = @"^(?:(\S+)[:,]\s+)?s/((?:\\/|[^/])+)/((?:\\/|[^/])*)(?:/(\S+)?)?$";

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
            if (!client.Users.Contains(nick))
                return;
            Regex find = new Regex(match.Groups[2].Value.Replace(@"\/", "/"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            String repl = match.Groups[3].Value.Replace(@"\/", "/");

            // Find the message to edit
            ProtoIrcMessage[] messages = ProtoIrcMessage.Query.OrderByDescending(m => m.Time).Take(Settings.Read<Int32>("messageQueryLimit")).ToArray();
            ProtoIrcMessage new_msg = null;
            for (Int32 i = 0; i < messages.Length; i++)
            {
                ProtoIrcMessage m = messages[i];
                if (m.User == nick && find.IsMatch(m.Message) && !Regex.IsMatch(m.Message, regex, RegexOptions.IgnoreCase))
                {
                    new_msg = m;
                    break;
                }
            }
            Char[] flags = match.Groups[4].Success ? match.Groups[4].Value.ToCharArray() : new Char[0];
            if (new_msg == null)
                return;
            if (new_msg.Message.StartsWith("\x01" + "ACTION"))
                new_msg.Message = new_msg.Message.Replace("\x01", "").Replace("ACTION", "/me");

            // Regex options
            RegexOptions options = RegexOptions.IgnoreCase;

            // Replace stuff
            new_msg.Message = flags.Contains('g') || flags.Contains('G') ? find.Replace(new_msg.Message, repl) : find.Replace(new_msg.Message, repl, 1);

            // Send the message back
            if (nick == message.User)
                BotController.SendMessage(client, nick + " [b]meant[/b] to say: " + new_msg.Message, message.User, message.Source, true);
            else if (!client.Channels[message.Source].Users.Contains("Kountdown"))
                BotController.SendMessage(client, message.User + " thinks " + nick + " [b]meant[/b] to say: " + new_msg.Message, message.User, message.Source, true);
        }
    }
}
