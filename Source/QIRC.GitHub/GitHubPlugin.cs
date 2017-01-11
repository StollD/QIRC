/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ChatSharp;
using ChatSharp.Events;
using Newtonsoft.Json.Linq;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;

namespace QIRC.Commands
{
    /// <summary>
    /// The IrcPlugin Implementation
    /// </summary>
    public class GitHubPlugin : IrcPlugin
    {
        public const String issueURL = @"(?:https?:\/\/(?:www\.)?github.com\/)?(?:([A-z0-9\-]+(\/)?[A-z0-9\-]+)?(\/)?)?(issues\/|pull\/|#)([\d]+)";
        public const String sha1URL = @"(?:https?:\/\/(?:www\.)?github.com\/)?(?:([A-z0-9\-]+(?:\/)?[A-z0-9\-]+)?(\/)?)?(commit\/|@)([a-z0-9A-Z]{5})";

        public override void OnChannelMessageRecieved(IrcClient client, PrivateMessageEventArgs e)
        {
            if (GitHubRepo.repos == null)
                GitHubRepo.repos = new SerializeableList<KeyValuePair<String, String>>("repos");
            if (GitHubAlias.alias == null)
                GitHubAlias.alias = new SerializeableList<KeyValuePair<String, String>>("repoalias");
            ProtoIrcMessage message = new ProtoIrcMessage(e);

            if (Regex.IsMatch(message.Message, issueURL))
            {
                foreach (Match match in Regex.Matches(message.Message, issueURL, RegexOptions.IgnoreCase))
                {
                    String id = match.Groups[5].Value;
                    if (match.Groups[1].Success)
                    {
                        String repo = GitHubAlias.alias.Count(r => r.Key == match.Groups[1].Value) > 0 ? GitHubAlias.alias.First(r => r.Key == match.Groups[1].Value).Value : match.Groups[1].Value;
                        String info = GetInfoIssue(repo, id);
                        if (!String.IsNullOrWhiteSpace(info))
                            BotController.SendMessage(client, info, message.User, message.Source, true);
                    }
                    else
                    {
                        String info = GetInfoIssue(GitHubRepo.repos.FirstOrDefault(r => r.Key == message.Source).Value, id);
                        if (!String.IsNullOrWhiteSpace(info))
                            BotController.SendMessage(client, info, message.User, message.Source, true);
                    }
                }
            }
            else if (Regex.IsMatch(message.Message, sha1URL))
            {
                foreach (Match match in Regex.Matches(message.Message, sha1URL, RegexOptions.IgnoreCase))
                {
                    String id = match.Groups[4].Value;
                    Console.WriteLine(id);
                    if (match.Groups[1].Success)
                    {
                        String repo = GitHubAlias.alias.Count(r => r.Key == match.Groups[1].Value) > 0 ? GitHubAlias.alias.First(r => r.Key == match.Groups[1].Value).Value : match.Groups[1].Value;
                        String info = GetInfoCommit(repo, id);
                        if (!String.IsNullOrWhiteSpace(info))
                            BotController.SendMessage(client, info, message.User, message.Source, true);
                    }
                    else
                    {
                        String info = GetInfoCommit(GitHubRepo.repos.FirstOrDefault(r => r.Key == message.Source).Value, id);
                        Console.WriteLine(info);
                        if (!String.IsNullOrWhiteSpace(info))
                            BotController.SendMessage(client, info, message.User, message.Source, true);
                    }
                }
            }
        }

        public String GetInfoIssue(String repository, String id)
        {
            try
            {
                HttpWebRequest web = WebRequest.Create("https://api.github.com/repos/" + repository + "/issues/" + id) as HttpWebRequest;
                web.UserAgent = "QIRC";
                String json = new System.IO.StreamReader(web.GetResponse().GetResponseStream()).ReadToEnd();
                JObject data = JObject.Parse(json);
                String body = "";
                String[] split = data["body"].ToString().Split('\n', '\r');
                if (split.Length > 1)
                    body = split[0] + "...";
                else if (split[0].Length > 200)
                    body = split[0].Substring(0, 200) + "...";
                else
                    body = split[0];
                return "[#" + id + "] [b]title:[/b] " + data["title"] + " [b]|[/b] " + body + " [b]|[/b] " + "https://github.com/" + repository + "/issues/" + id;
            }
            catch
            {
                return "";
            }
        }

        public String GetInfoCommit(String repository, String id)
        {
            try
            {
                HttpWebRequest web = WebRequest.Create("https://api.github.com/repos/" + repository + "/commits/" + id) as HttpWebRequest;
                web.UserAgent = "QIRC";
                String json = new System.IO.StreamReader(web.GetResponse().GetResponseStream()).ReadToEnd();
                JObject data = JObject.Parse(json);
                String body = "";
                if (data["commit"]["message"].ToString().Split('\n').Length > 1)
                    body = data["commit"]["message"].ToString().Split('\n')[0] + "...";
                else if (data["commit"]["message"].ToString().Split('\n')[0].Length > 200)
                    body = data["commit"]["message"].ToString().Split('\n')[0].Substring(0, 200) + "...";
                else
                    body = data["commit"]["message"].ToString().Split('\n')[0];
                return "[" + ((String)data["sha"]).Substring(0, 5) + "] [b]title:[/b] " + body + " by " + data["commit"]["author"]["name"] + " [b]|[/b] Additions: [color=LightGreen]" + data["stats"]["additions"] + "[/color] [b]|[/b] Deletions: [color=Red]" + data["stats"]["deletions"] + "[/color] [b]|[/b] " + data["html_url"];
            }
            catch (Exception exception)
            {
                Console.Write(exception.Message);
                return "";
            }
        }
    }
}