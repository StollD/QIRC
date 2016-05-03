/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;
using System.Linq;
using System.Text;
using Mono.CSharp;
using QIRC.IRC;

namespace QIRC.Commands
{
    /// <summary>
    /// A dynamic ReportPrinter
    /// </summary>
    public class DelegateReportPrinter : ReportPrinter
    {
        /// <summary>
        /// The action that gets executed
        /// </summary>
        protected Action<String, ProtoIrcMessage> action;

        /// <summary>
        /// Print sth.
        /// </summary>
        public override void Print(AbstractMessage msg, Boolean showFullPath)
        {
            String output = "";
            base.Print(msg, showFullPath);
            StringBuilder stringBuilder = new StringBuilder();
            if (!msg.Location.IsNull)
            {
                stringBuilder.Append(!showFullPath ? msg.Location.ToString() : msg.Location.ToStringFullName());
                stringBuilder.Append(" ");
            }
            stringBuilder.AppendFormat("{0} CS{1:0000}: {2}", msg.MessageType, msg.Code, msg.Text);
            if (msg.IsWarning)
                output += stringBuilder.ToString();
            else
                output += FormatText(stringBuilder.ToString());
            if (msg.RelatedSymbols != null)
            {
                String[] relatedSymbols = msg.RelatedSymbols;
                output = relatedSymbols.Aggregate(output, (current, t) => current + String.Concat(t, msg.MessageType, ")"));
            }
            action(output, CSharp.lastMsg);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DelegateReportPrinter(Action<String, ProtoIrcMessage> action)
        {
            this.action = action;
        }
    }
}