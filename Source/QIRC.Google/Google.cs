/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using ChatSharp;
using Newtonsoft.Json;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using System;
using System.Collections.Generic;
using System.Net;

namespace QIRC.Commands
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
            if (StartsWithParam("results", message.Message))
            {
                String query = message.Message;
                Int32 results = Int32.Parse(StripParam("results", ref query));
                query = query.Trim();
                GoogleResults result = JsonConvert.DeserializeObject<GoogleResults>(new WebClient().DownloadString("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&rsz=8&q=" + query + "&hl=en"));
                if (result.responseData.results.Count == 0)
                {
                    QIRC.SendMessage(client, "No results found for \"" + query + "\"", message.User, message.Source);
                    return;
                }
                Int32 start = 7;
                while (result.responseData.results.Count < results)
                {
                    result.responseData.results.AddRange(JsonConvert.DeserializeObject<GoogleResults>(new WebClient().DownloadString("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&rsz=8&start=" + start + "&q=" + query + "&hl=en")).responseData.results);
                    start += 8;
                }
                if (message.IsChannelMessage) QIRC.SendMessage(client, "Writing you a PM with the first " + results + " search results for \"" + query + "\"", message.User, message.Source);
                for (Int32 i = 0; i < results; i++)
                {
                    QIRC.SendMessage(client, Uri.UnescapeDataString(result.responseData.results[i].url) + " [" + result.responseData.results[i].titleNoFormatting + "]", message.User, message.User, true);
                }
            }
            else
            {
                String query = message.Message;
                GoogleResults result = JsonConvert.DeserializeObject<GoogleResults>(new WebClient().DownloadString("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&rsz=8&q=" + query + "&hl=en"));
                if (result.responseData.results.Count == 0)
                {
                    QIRC.SendMessage(client, "No results found for \"" + query + "\"", message.User, message.Source);
                    return;
                }
                QIRC.SendMessage(client, Uri.UnescapeDataString(result.responseData.results[0].url) + " [" + result.responseData.results[0].titleNoFormatting + "] (" + result.responseData.cursor.resultCount + " results found, took " + result.responseData.cursor.searchResultTime + "s)", message.User, message.Source, !message.IsChannelMessage);
            }
        }

        /// Google API Classes
        public struct GoogleResults
        {
            public ResponseData responseData;
            public String responseDetails;
            public Int32 responseStatus;
        }
        public struct ResponseData
        {
            public List<Result> results;
            public Cursor cursor;
        }
        public struct Result
        {
            public String GsearchResultClass;
            public String unescapedUrl;
            public String url;
            public String visibleUrl;
            public String cacheUrl;
            public String title;
            public String titleNoFormatting;
            public String content;
        }
        public struct Cursor
        {
            public String resultCount;
            public List<Page> pages;
            public String estimatedResultCount;
            public Int32 currentPageIndex;
            public String moreResultsUrl;
            public String searchResultTime;
        }
        public struct Page
        {
            public String start;
            public Int32 label;
        }
    }
}
