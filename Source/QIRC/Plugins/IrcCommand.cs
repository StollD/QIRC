/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2016
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// IRC
using ChatSharp;

/// QIRC
using QIRC.IRC;

/// System
using System;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// The namespace where everything Plugin related is stored.
/// </summary>
namespace QIRC.Plugins
{
    /// <summary>
    /// This is an abstract base for a QIRC Command
    /// </summary>
    public abstract class IrcCommand
    {
        public virtual String GetName() { return ""; }
        public virtual String GetDescription() { return ""; }
        public virtual Boolean IsSerious() { return false; }
        public virtual AccessLevel GetAccessLevel() { return AccessLevel.NORMAL; }
        public virtual void RunCommand(IrcClient client, ProtoIrcMessage message) { }
        public virtual String[] GetParameters() { return new String[0]; }
        public virtual String GetExample() { return ""; }
        protected Boolean StartsWithParam(String param, String message)
        {
            String[] arguments = new String[GetParameters().Length];
            Int32 i = 0;
            foreach (String s in message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!s.StartsWith("-"))
                    break;
                arguments[i] = s.Remove(0, 1).Split(':', '=')[0];
            }
            return arguments.Contains(param);
        }
        protected String StripParam(String param, ref String message)
        {
            Regex regex = new Regex(@"-([^-\s:=]+)([:=])?([^-\s]+)?");
            while (regex.IsMatch(message))
            {
                Match match = regex.Match(message);
                message = message.Remove(message.IndexOf(match.ToString()), match.ToString().Length);
                if (!String.Equals(match.Groups[1].Value, param, StringComparison.InvariantCultureIgnoreCase)) continue;
                String val = "";
                String replace = "-" + param;
                if (match.Groups[2].Success && match.Groups[3].Success)
                {
                    replace += match.Groups[2].Value + match.Groups[3].Value;
                    val = match.Groups[3].Value;
                }
                return val;
            }
            return "";
        }
    }
}

