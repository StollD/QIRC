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

/// System
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

/// <summary>
/// Here's everything that is an IrcCommand
/// </summary>
namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the wolfram command. It takes a search parameter and queries
    /// WolframAlphas API for it. 
    /// </summary>
    public class WolframAlpha : IrcCommand
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
            return "wa";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Queries the Wolfram Alpha API for a search parameter and returns the results.";
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
            return Settings.Read<String>("control") + GetName() + " What is IRC?";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            WolframConfig config = Settings.Read<WolframConfig>("WolframAlpha");
            XmlDocument document = new XmlDocument();
            String wa = new WebClient()
                { Encoding = System.Text.Encoding.UTF8 }
                .DownloadString("http://api.wolframalpha.com/v2/query?appid=" + config.appid + "&input=" + Uri.EscapeDataString(message.Message) + "&format=plaintext&podindex=1&podindex=2&scantimeout=7&podtimeout=7&formattimeout=7&parsetimeout=7&units=" + config.units);
            document.LoadXml(wa);
            String success = document.DocumentElement.Attributes["success"].Value;
            if (success != "true")
            {
                QIRC.SendMessage(client, "Seems that Wolfram is unable to understand that.", message.User, message.Source);
                return;
            }
            String wolframOutput = "";
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                if (node.Name != "pod")
                    continue;
                String subpod = node.FirstChild.FirstChild.InnerText;

                /// Fix Wolframs Formatting
                subpod = Regex.Replace(subpod, "\n", ", ");
                subpod = Regex.Replace(subpod, "[ ]{2,}", " ");

                if (node.Attributes["title"].Value == "Input interpretation")
                    wolframOutput += subpod + ": ";
                else if (node.Attributes["title"].Value == "Input")
                    wolframOutput += subpod + " = ";
                else
                    wolframOutput += subpod;
            }
            QIRC.SendMessage(client, wolframOutput, message.User, message.Source);
        }
    }

    /// <summary>
    /// Alias for the google command.
    /// </summary>
    public class Wolfram_L : WolframAlpha
    {
        public override String GetName()
        {
            return "wolfram";
        }
    }

    /// <summary>
    /// An IRC plugin that adds the Wolfram config options to the system
    /// </summary>
    public class WolframConfigAdder : IrcPlugin
    {
        public override void OnLoad()
        {
            SettingsFile file = null;
            Settings.GetFile("settings", ref file);
            file.Add("WolframAlpha", new WolframConfig());
        }
    }

    /// <summary>
    /// The config for the Wolfram Query
    /// </summary>
    public class WolframConfig
    {
        public String units = "metric";
        public String appid = "";
    }
}
