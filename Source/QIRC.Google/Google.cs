/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;
using System.Collections.Generic;
using System.Net;
using ChatSharp;
using Newtonsoft.Json;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;

namespace QIRC.Google
{
    /// <summary>
    /// This is the implementation for the google command. It takes a search parameter and queries
    /// Googles API for it. Optionally, it PM's the user a defineable amount of search results. Standard is one.
    /// </summary>
    public class Google : IrcCommand
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
            return "google";
        }

        /// <summary>
        /// Alternative names
        /// </summary>
        public override String[] GetAlternativeNames()
        {
            return new[] { "g" };
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Queries the Google API for a search parameter and returns the results.";
        }

        /// <summary>
        /// Whether the command can be used in serious channels.
        /// </summary>
        public override Boolean IsSerious()
        {
            return true;
        }

        /// <summary>
        /// The Parameters of the Command
        /// </summary>
        public override String[] GetParameters()
        {
            return new String[]
            {
                "results", "Displays the given amount of results in a private message."
            };
        }

        /// <summary>
        /// An example for using the command.
        /// </summary>
        /// <returns></returns>
        public override String GetExample()
        {
            return Settings.Read<String>("control") + GetName() + " Internet Relay Chat";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            // Get the config
            GoogleConfig config = Settings.Read<GoogleConfig>("Google");
            
            if (StartsWithParam("results", message.Message))
            {
                String query = message.Message;
                Int32 results = Int32.Parse(StripParam("results", ref query));
                query = query.Trim();
                GoogleResults result = JsonConvert.DeserializeObject<GoogleResults>(new WebClient().DownloadString("https://www.googleapis.com/customsearch/v1?num=8&q=" + query + "&hl=en&cx=" + config.searchEngine + "&key=" + config.apiKey));
                if (result.searchInformation.totalResults == "0")
                {
                    BotController.SendMessage(client, "No results found for \"" + query + "\"", message.User, message.Source);
                    return;
                }
                Int32 start = 7;
                while (Int32.Parse(result.searchInformation.totalResults) < results)
                {
                    result.items.AddRange(JsonConvert.DeserializeObject<GoogleResults>(new WebClient().DownloadString("https://www.googleapis.com/customsearch/v1?num=8&start=" + start + "&q=" + query + "&hl=en&cx=" + config.searchEngine + "&key=" + config.apiKey)).items);
                    start += 8;
                }
                if (message.IsChannelMessage) BotController.SendMessage(client, "Writing you a PM with the first " + results + " search results for \"" + query + "\"", message.User, message.Source);
                for (Int32 i = 0; i < results; i++)
                {
                    BotController.SendMessage(client, result.items[i].link + " [" + result.items[i].title + "]", message.User, message.User, true);
                }
            }
            else
            {
                String query = message.Message;
                GoogleResults result = JsonConvert.DeserializeObject<GoogleResults>(new WebClient().DownloadString("https://www.googleapis.com/customsearch/v1?num=8&q=" + query + "&hl=en&cx=" + config.searchEngine + "&key=" + config.apiKey));
                if (result.searchInformation.totalResults == "0") // Why Google, why...
                {
                    BotController.SendMessage(client, "No results found for \"" + query + "\"", message.User, message.Source);
                    return;
                }
                BotController.SendMessage(client, result.items[0].link + " [" + result.items[0].title + "] (" + result.searchInformation.totalResults + " results found, took " + result.searchInformation.formattedSearchTime + "s)", message.User, message.Source, !message.IsChannelMessage);
            }
        }

        /// <summary>
        /// Adds the Google Settings to the config
        /// </summary>
        /// IrcClient Functions
        public override void OnLoad()
        {
            SettingsFile file = null;
            Settings.GetFile("settings", ref file);
            file.Add("Google", new GoogleConfig());
        }
        
        // Config Class
        public class GoogleConfig
        {
            public String searchEngine = "";
            public String apiKey = "";
        }

        // Google API Classes
        public struct GoogleResults
        {
            public GoogleSearchInformation searchInformation;
            [JsonProperty(Required = Required.Default)]
            public List<GoogleItem> items;
        }
        public struct GoogleSearchInformation
        {
            public Double searchTime;
            public String formattedSearchTime;
            public String totalResults;
            public String formattedTotalResults;
        }
        public struct GoogleItem
        {
            public String title;
            public String htmlTitle;
            public String link;
            public String displayLink;
            public String snippet;
            public String htmlSnippet;
            public String cacheId;
            public String formattedUrl;
            public String htmlFormattedUrl;
        }
    }
}
