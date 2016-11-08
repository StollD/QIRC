/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
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
            String[] arguments = new String[GetParameters().Length];
            Int32 i = 0;
            foreach (String s in message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!s.StartsWith("-"))
                    break;
                arguments[i] = s.Remove(0, 1).Split(':', '=')[0];
                i++;
            }
            return arguments.Contains(param);
        }

        /// <summary>
        /// Strips the parameter from the message.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected String StripParam(String param, ref String message)
        {
            String msg = String.Copy(message);
            Regex regex = new Regex(@"-([^-\s:=]+)([:=])?([^\s]+)?");
            while (regex.IsMatch(msg))
            {
                Match match = regex.Match(msg);
                msg = msg.Remove(msg.IndexOf(match.ToString()), match.ToString().Length);
                if (!String.Equals(match.Groups[1].Value, param, StringComparison.InvariantCultureIgnoreCase)) continue;
                String val = "";
                String replace = "-" + param;
                if (match.Groups[2].Success && match.Groups[3].Success)
                {
                    replace += match.Groups[2].Value + match.Groups[3].Value;
                    val = match.Groups[3].Value;
                }
                message = message.Replace(replace + " ", "").Trim();
                return val;
            }
            return "";
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

