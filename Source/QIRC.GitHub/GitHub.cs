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

/// Json
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// System
using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Here's everything that is an IrcCommand
/// </summary>
namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the github tool suite. The command can set the default repository
    /// for the channel and the IrcPlugin is resposible for posting Links to issues and so on
    /// </summary>
    public class GitHubRepo : IrcCommand
    {
        /// <summary>
        /// The Access Level that is needed to execute the command
        /// </summary>
        public override AccessLevel GetAccessLevel()
        {
            return AccessLevel.OPERATOR;
        }

        /// <summary>
        /// The name of the command
        /// </summary>
        public override String GetName()
        {
            return "setchannelrepo";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Sets the default GitHub repository for this channel.";
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
            return Settings.Read<String>("control") + GetName() + " ThomasKerman/QIRC";
        }

        public static SerializeableList<KeyValuePair<String, String>> repos { get; set; }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (repos == null)
                repos = new SerializeableList<KeyValuePair<String, String>>("repos");
            if (!message.IsChannelMessage)
                return;
            if (repos.Count(r => r.Key == message.Source) == 0)
                repos.Add(new KeyValuePair<String, String>(message.Source, message.Message.Trim()));
            else
                repos[repos.IndexOf(repos.First(r => r.Key == message.Source))] = new KeyValuePair<String, String>(message.Source, message.Message.Trim());
            QIRC.SendMessage(client, "Set default repository for " + message.Source + " to " + message.Message.Trim(), message.User, message.Source);
        }
    }

    /// <summary>
    /// This is the implementation for the github tool suite. The command can set the default repository
    /// for the channel and the IrcPlugin is resposible for posting Links to issues and so on
    /// </summary>
    public class GitHubAlias : IrcCommand
    {
        /// <summary>
        /// The Access Level that is needed to execute the command
        /// </summary>
        public override AccessLevel GetAccessLevel()
        {
            return AccessLevel.OPERATOR;
        }

        /// <summary>
        /// The name of the command
        /// </summary>
        public override String GetName()
        {
            return "setrepoalias";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Sets an alias for the given repository";
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
            return Settings.Read<String>("control") + GetName() + " QIRC ThomasKerman/QIRC";
        }

        public static SerializeableList<KeyValuePair<String, String>> alias { get; set; }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            if (alias == null)
                alias = new SerializeableList<KeyValuePair<String, String>>("repoalias");
            if (!message.IsChannelMessage)
                return;
            if (alias.Count(r => r.Key == message.Source) == 0)
                alias.Add(new KeyValuePair<String, String>(message.Message.Split(' ')[0].Trim(), message.Message.Split(' ')[1].Trim()));
            else
                alias[alias.IndexOf(alias.First(r => r.Key == message.Message.Split(' ')[0].Trim()))] = new KeyValuePair<String, String>(message.Message.Split(' ')[0].Trim(), message.Message.Split(' ')[1].Trim());
            QIRC.SendMessage(client, "Set alias for https://github.com/" + message.Message.Split(' ')[1].Trim() + "/ to " + message.Message.Split(' ')[0].Trim(), message.User, message.Source);
        }
    }

    /// <summary>
    /// The IrcPlugin Implementation
    /// </summary>
    public class GitHubPlugin : IrcPlugin
    {
        public const String issueURL = @"(?:https?:\/\/(?:www\.)?github.com\/)?(?:([A-z0-9\-]+(\/)?[A-z0-9\-]+)?(\/)?)?(issues\/|pull\/|#)([\d]+)";

        public override void OnChannelMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            if (GitHubRepo.repos == null)
                GitHubRepo.repos = new SerializeableList<KeyValuePair<String, String>>("repos");
            if (GitHubAlias.alias == null)
                GitHubAlias.alias = new SerializeableList<KeyValuePair<String, String>>("repoalias");
            ProtoIrcMessage message = new ProtoIrcMessage(e);
            if (!Regex.IsMatch(message.Message, issueURL))
                return;
            Match match = Regex.Match(message.Message, issueURL, RegexOptions.IgnoreCase);
            String id = match.Groups[5].Value;
            if (match.Groups[1].Success)
            {
                String repo = GitHubAlias.alias.Count(r => r.Key == match.Groups[1].Value) > 0 ? GitHubAlias.alias.First(r => r.Key == match.Groups[1].Value).Value : match.Groups[1].Value;
                String info = GetInfo(repo, id);
                if (!String.IsNullOrWhiteSpace(info))
                    QIRC.SendMessage(client, info, message.User, message.Source, true);
            }
            else
            {
                String info = GetInfo(GitHubRepo.repos.FirstOrDefault(r => r.Key == message.Source).Value, id);
                if (!String.IsNullOrWhiteSpace(info))
                    QIRC.SendMessage(client, info, message.User, message.Source, true);
            }
        }

        public String GetInfo(String repository, String id)
        {
            try
            {
                HttpWebRequest web = WebRequest.Create("https://api.github.com/repos/" + repository + "/issues/" + id) as HttpWebRequest;
                web.UserAgent = "QIRC";
                String json = new System.IO.StreamReader(web.GetResponse().GetResponseStream()).ReadToEnd();
                JObject data = JObject.Parse(json);
                String body = "";
                if (data["body"].ToString().Split('\n').Length > 1)
                    body = data["body"].ToString().Split('\n')[0] + "...";
                else
                    body = data["body"].ToString().Split('\n')[0];
                return "[#" + id + "] [b]title:[/b] " + data["title"] + " [b]|[/b] " + body;
            }
            catch
            {
                return "";
            }
        }
    }
}
