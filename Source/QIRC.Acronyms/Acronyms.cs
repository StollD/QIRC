/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */
 
using ChatSharp;
using ChatSharp.Events;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;
using System;
using System.Linq;

namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the acronym command. It grabs questions from the
    /// chat and searches it's database for a matching explanation
    /// </summary>
    public class Acronym : IrcCommand
    {
        /// <summary>
        /// The persistent List of Acronyms
        /// </summary>
        public SerializeableList<Tuple<String, String>> acronyms { get; set; }

        /// <summary>
        /// The Access Level that is needed to execute the command
        /// </summary>
        public override AccessLevel GetAccessLevel()
        {
            return AccessLevel.NORMAL;
        }

        /// <summary>
        /// The name of the command
        /// </summary>
        public override String GetName()
        {
            return "acr";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Expands an acronym to a text that is easily understandable.";
        }

        /// <summary>
        /// The Parameters of the Command
        /// </summary>
        public override String[] GetParameters()
        {
            return new String[]
            {
                "add", "Adds an explanation",
                "remove", "Removes an explanation",
                "update", "Updates an explanation",
                "list", "Returns a list with all stored acronyms"
            };
        }

        /// <summary>
        /// Whether the command can be used in serious channels.
        /// </summary>
        public override Boolean IsSerious()
        {
            return true;
        }

        /// <summary>
        /// An example for using the command.
        /// </summary>
        /// <returns></returns>
        public override String GetExample()
        {
            return Settings.Read<String>("control") + GetName() + " IRC";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            // Null Check
            if (acronyms == null)
                acronyms = new SerializeableList<Tuple<String, String>>("acronyms");

            // If we have no additional commands
            if (message.Message.Length == 0)
            {
                // Response
                QIRC.SendMessage(client, "I have " + acronyms.Count + " explanations for acronyms stored", message.User, message.Source);
            }
            else
            {
                // If we should add an acronym
                if (StartsWithParam("add", message.Message))
                {
                    // Get the identifier
                    String text = message.Message;
                    String ident = StripParam("add", ref text);

                    // Error out
                    if (String.IsNullOrWhiteSpace(ident))
                    {
                        QIRC.SendMessage(client, "Invalid key!", message.User, message.Source);
                        return;
                    }

                    // Get the text
                    message.Message = message.Message.Remove(0, ("[" + ident + "]").Length).Trim();

                    /// Add it
                    if (acronyms.Count(t => t.Item1 == ident) != 0)
                    {
                        QIRC.SendMessage(client, "I already know an explanation for " + ident + "! (Update it with " + Settings.Read<String>("control") + GetName() + " -update:" + ident + " " + text + ")", message.User, message.Source);
                        return;
                    }
                    acronyms.Add(new Tuple<String, String>(ident, text.Trim()));
                    QIRC.SendMessage(client, "I added the explanation for this acronym.", message.User, message.Source);
                }
                else if (StartsWithParam("remove", message.Message))
                {
                    // remove it!
                    String text = message.Message;
                    String ident = StripParam("remove", ref text);

                    // If we don't know this
                    if (acronyms.Count(t => t.Item1 == ident) == 0)
                    {
                        QIRC.SendMessage(client, "This key is not registered!", message.User, message.Source);
                        return;
                    }
                    acronyms.RemoveAll(t => t.Item1 == ident);
                    QIRC.SendMessage(client, "I removed the explanation for " + ident, message.User, message.Source);
                }
                else if (StartsWithParam("update", message.Message))
                {
                    // update it!
                    String text = message.Message;
                    String ident = StripParam("update", ref text);

                    // If we don't know this
                    if (acronyms.Count(t => t.Item1 == ident) == 0)
                    {
                        QIRC.SendMessage(client, "This key is not registered!", message.User, message.Source);
                        return;
                    }
                    acronyms.RemoveAll(t => t.Item1 == ident);
                    acronyms.Add(new Tuple<String, String>(ident, text));
                    QIRC.SendMessage(client, "I updated the explanation for " + ident, message.User, message.Source);
                }
                else if (StartsWithParam("list", message.Message))
                {
                    // Announce it
                    if (message.IsChannelMessage) QIRC.SendMessage(client, "I will send you a list of my acronyms!", message.User, message.Source);
                    QIRC.SendMessage(client, "Here is a list of all my acronyms: " + String.Join(", ", acronyms.Select(t => t.Item1)), message.User, message.User, true);
                }
                else
                {
                    // If we don't know this
                    if (acronyms.Count(t => t.Item1 == message.Message.Trim()) == 0)
                    {
                        QIRC.SendMessage(client, "This key is not registered!", message.User, message.Source);
                        return;
                    }
                    QIRC.SendMessage(client, "[" + message.Message.Trim() + "] => " + acronyms.First(t => t.Item1 == message.Message.Trim()).Item2, message.User, message.Source);
                }
            }
        }

        /// <summary>
        /// Reply to messages who match a special scheme
        /// </summary>
        public override void OnPrivateMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            if (e.PrivateMessage.Message.EndsWith("?", StringComparison.InvariantCultureIgnoreCase))
            {
                String message = e.PrivateMessage.Message.Remove(e.PrivateMessage.Message.Length - 1);
                if (acronyms.Count(t => t.Item1 == message) > 0)
                    RunCommand(client, new ProtoIrcMessage(e) {Message = message});
            }
        }
    }

    /// <summary>
    /// Aliased version of the Acronym command
    /// </summary>
    public class AcronymLong : Acronym
    {
        public override String GetName()
        {
            return "acronym";
        }
        public override void OnPrivateMessageRecieved(IrcClient client, PrivateMessageEventArgs e) {}
    }
}
