/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;
using System.Net;
using System.Text;
using ChatSharp;
using ChatSharp.Events;
using Newtonsoft.Json.Linq;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;

namespace QIRC.Acronyms
{
    /// <summary>
    /// This is the implementation for the acronym command. It grabs questions from the
    /// chat and searches it's database for a matching explanation
    /// </summary>
    public class Acronym : IrcCommand
    {
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
        /// Alternative names
        /// </summary>
        public override String[] GetAlternativeNames()
        {
            return new[] {"acronym"};
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
            return new []
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
            // If we have no additional commands
            if (message.Message.Length == 0)
            {
                // Response
                BotController.SendMessage(client, "I have " + AcronymData.Query.Count() + " explanations for acronyms stored", message.User, message.Source);
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
                        BotController.SendMessage(client, "Invalid key!", message.User, message.Source);
                        return;
                    }

                    // Get the text
                    message.Message = message.Message.Remove(0, ("[" + ident + "]").Length).Trim();

                    // Add it
                    if (AcronymData.Query.Count(t => t.Short == ident) != 0)
                    {
                        BotController.SendMessage(client, "I already know an explanation for " + ident + "! (Update it with " + Settings.Read<String>("control") + GetName() + " -update:" + ident + " " + text + ")", message.User, message.Source);
                        return;
                    }
                    AcronymData.Query.Insert(ident, text.Trim());
                    BotController.SendMessage(client, "I added the explanation for this acronym.", message.User, message.Source);
                }
                else if (StartsWithParam("remove", message.Message))
                {
                    // remove it!
                    String text = message.Message;
                    String ident = StripParam("remove", ref text);

                    // If we don't know this
                    if (AcronymData.Query.Count(t => t.Short == ident) == 0)
                    {
                        BotController.SendMessage(client, "This key is not registered!", message.User, message.Source);
                        return;
                    }
                    AcronymData.Query.Delete(t => t.Short == ident);
                    BotController.SendMessage(client, "I removed the explanation for " + ident, message.User, message.Source);
                }
                else if (StartsWithParam("update", message.Message))
                {
                    // update it!
                    String text = message.Message;
                    String ident = StripParam("update", ref text);

                    // If we don't know this
                    if (AcronymData.Query.Count(t => t.Short == ident) == 0)
                    {
                        BotController.SendMessage(client, "This key is not registered!", message.User, message.Source);
                        return;
                    }
                    AcronymData.Query.Delete(t => t.Short == ident);
                    AcronymData.Query.Insert(ident, text.Trim());
                    BotController.SendMessage(client, "I updated the explanation for " + ident, message.User, message.Source);
                }
                else if (StartsWithParam("list", message.Message))
                {
                    // Announce it
                    if (message.IsChannelMessage) BotController.SendMessage(client, "I will send you a list of my acronyms!", message.User, message.Source);
                    String content = "";
                    foreach (AcronymData data in AcronymData.Query)
                    {
                        content += "[" + data.Short + "] => " + data.Explanation + "\n";
                    }
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        String response = JObject.Parse(wc.UploadString("https://hastebin.com/documents", "POST", content))["key"].ToString();
                        BotController.SendMessage(client, "Here is a list of all my acronyms:  https://hastebin.com/" + response, message.User, message.User, true);
                    }
                }
                else
                {
                    // If we don't know this
                    String temp = message.Message.Trim();
                    if (AcronymData.Query.Count(t => t.Short == temp) == 0)
                    {
                        BotController.SendMessage(client, "This key is not registered!", message.User, message.Source);
                        return;
                    }
                    BotController.SendMessage(client, "[" + message.Message.Trim() + "] => " + AcronymData.Query.First(t => t.Short == temp).Explanation, message.User, message.Source);
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
                String message = e.PrivateMessage.Message.Remove(e.PrivateMessage.Message.Length - 1).Trim();
                if (AcronymData.Query.Count(t => t.Short == message) > 0)
                    RunCommand(client, new ProtoIrcMessage(e) {Message = message});
            }
        }
    }
}
