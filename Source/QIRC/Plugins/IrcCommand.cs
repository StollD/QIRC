/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using ChatSharp;
using QIRC.IRC;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace QIRC.Plugins
{
    /// <summary>
    /// This is an abstract base for a QIRC Command
    /// </summary>
    public abstract class IrcCommand : IrcPlugin
    {
        public virtual String GetName() { return ""; }
        public virtual String[] GetAlternativeNames() { return new String[0]; }
        public virtual String GetDescription() { return ""; }
        public virtual Boolean IsSerious() { return false; }
        public virtual AccessLevel GetAccessLevel() { return AccessLevel.NORMAL; }
        public virtual void RunCommand(IrcClient client, ProtoIrcMessage message) { }
        public virtual String[] GetParameters() { return new String[0]; }
        public virtual String GetExample() { return ""; }

        /// <summary>
        /// A checkhandler that ensures that the user can run the command
        /// </summary>
        public static CheckHandler<Tuple<IrcUser, IrcCommand>> ExecuteCheck = new CheckHandler<Tuple<IrcUser, IrcCommand>>();

        /// <summary>
        /// Whether the message contains the given parameter
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected Boolean StartsWithParam(String param, String message)
        {
            String escaped = Regex.Replace(message, "\\\\(?<!\\\\\\\\)(-([^:= ]*)(?:[:=](?:([^ \"]+)|\"([^\"]*)\"))?)", "");
            MatchCollection matches = Regex.Matches(escaped, "-([^:= ]+)(?:[:=](?:([^ \"]+)|\"([^\"]*)\"))?");
            return matches.OfType<Match>().Any(m => String.Equals(m.Groups[1].Value.Trim(), param, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Strips the parameter from the message.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected String StripParam(String param, ref String message, Boolean escape = false)
        {
            String escaped = Regex.Replace(message, "\\\\(?<!\\\\\\\\)(-([^:= ]*)(?:[:=](?:([^ \"]+)|\"([^\"]*)\"))?)", "");
            MatchCollection matches = Regex.Matches(escaped, "-([^:= ]+)(?:[:=](?:([^ \"]+)|\"([^\"]*)\"))?");
            Match firstMatch = matches.OfType<Match>().FirstOrDefault(m => String.Equals(m.Groups[1].Value.Trim(), param, StringComparison.InvariantCultureIgnoreCase));
            if (firstMatch == null)
                return "";

            String value = "";
            if (firstMatch.Groups.Count == 4 && String.IsNullOrWhiteSpace(firstMatch.Groups[3].Value))
                value = firstMatch.Groups[2].Value;
            else if (firstMatch.Groups.Count == 4)
                value = firstMatch.Groups[3].Value;
            message = Regex.Replace(message, @"(" + (escape ? Regex.Escape(firstMatch.Value) : firstMatch.Value) + ")?", "");
            message = Regex.Replace(message, "\\\\(?<!\\\\\\\\)-", "-");
            message = message.Trim();
            return value.Trim();
        }

        /// <summary>
        /// Checks if the supplied name should trigger this command
        /// </summary>
        public Boolean IsNamed(String name)
        {
            if (String.Equals(GetName(), name, StringComparison.InvariantCultureIgnoreCase))
                return true;
            if (GetAlternativeNames().Select(s => s.ToLower()).Contains(name.ToLower()))
                return true;
            return false;
        }
    }
}

