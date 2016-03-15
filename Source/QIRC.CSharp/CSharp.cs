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

/// System
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Text;

/// Mono
using Mono.CSharp;

/// <summary>
/// Here's everything that is an IrcCommand
/// </summary>
namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the csharp command. It will take C# code, compile
    /// and execute it in a Sandbox
    /// </summary>
    public class CSharp : IrcCommand
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
            return "csharp";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Evaluates C# code and executes it in a sandbox.";
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
                "reset", "Clears the state of the C# shell.",
                "persistent", "Saves an expression into the class body.",
                "state", "Debugs the state of the evaluator"
            };
        }

        /// <summary>
        /// An example for using the command.
        /// </summary>
        /// <returns></returns>
        public override String GetExample()
        {
            return Settings.Read<String>("control") + GetName() + " Math.PI";
        }

        /// <summary>
        /// The C# Evaluator
        /// </summary>
        protected static Evaluator evaluator { get; set; }

        /// <summary>
        /// All persistent expressions
        /// </summary>
        protected static SerializeableList<String> persistent { get; set; }

        /// <summary>
        /// The last message we got
        /// </summary>
        internal static ProtoIrcMessage lastMsg { get; set; }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            /// Update the message 
            lastMsg = message;

            /// Load the list
            if (persistent == null)
                persistent = new SerializeableList<String>("csharp_persistent");

            /// Reset the evaluator
            if (StartsWithParam("reset", message.Message))
            {
                evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), new DelegateReportPrinter((state, msg) => { foreach (String s in state.Split('\n')) QIRC.SendMessage(QIRC.client, s, msg.User, msg.Source, true); })));
                Evaluate(client, "using System; using System.Linq; using System.Collections.Generic; using System.Collections;", message.User, message.Source, true);
                foreach (String s in persistent)
                    Evaluate(client, s, message.User, message.Source, QIRC.CheckPermission(AccessLevel.ADMIN, message.level), true);
                QIRC.SendMessage(client, "Cleared the C# Evaluator.", message.User, message.Source);
                return;
            }

            /// Debugs the evaluator state
            if (StartsWithParam("state", message.Message))
            {
                QIRC.SendMessage(client, "Using: " + evaluator.GetUsing().Replace("\n", ""), message.User, message.User, true);
                QIRC.SendMessage(client, "Variables: " + evaluator.GetVars().Replace('\n', ';'), message.User, message.User, true);
                if (message.IsChannelMessage) QIRC.SendMessage(client, "I sent you the current state of the evaluator.", message.User, message.Source);
                return;
            }

            /// Saves an expression
            if (StartsWithParam("persistent", message.Message))
            {
                String text = message.Message;
                StripParam("persistent", ref text);
                persistent.Add(text.Trim());
                message.Message = text.Trim();
            }

            /// Create the Evaluator
            if (evaluator == null)
            {
                evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), new DelegateReportPrinter((state, msg) => { foreach (String s in state.Split('\n')) QIRC.SendMessage(QIRC.client, s, msg.User, msg.Source, true); })));
                Evaluate(client, "using System; using System.Linq; using System.Collections.Generic; using System.Collections;", message.User, message.Source, QIRC.CheckPermission(AccessLevel.ADMIN, message.level), true);
                foreach (String s in persistent)
                    Evaluate(client, s, message.User, message.Source, true);
            }

            /// Evaluate!
            Evaluate(client, message.Message, message.User, message.Source, QIRC.CheckPermission(AccessLevel.ADMIN, message.level));
        }

        /// <summary>
        /// Evaluates a C# expression. Ported from Mono REPL
        /// </summary>
        protected string Evaluate(IrcClient client, String input, String user, String source, Boolean admin, Boolean quite = false)
        {
            bool result_set;
            object result;
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            try
            {
                CompiledMethod method = evaluator.Compile(input);
                if (method == null)
                    return "";
                SDILReader.MethodBodyReader reader = new SDILReader.MethodBodyReader(method.Method);
                String il = reader.GetBodyCode();
                if ((il.Contains("System.IO") ||
                    il.Contains("System.Xml") ||
                    il.Contains("System.Runtime.InteropServices") ||
                    il.Contains("System.Diagnostics.Process") ||
                    il.Contains("System.Console") ||
                    il.Contains("System.Threading") ||
                    il.Contains("System.Environment::Exit") ||
                    il.Contains("System.Environment::FailFast") ||
                    il.Contains("System.Environment::SetEnvironmentVariable")) && !admin)
                {
                    QIRC.SendMessage(client, "You tried to use a forbidden type, method or namespace!", user, source);
                    return "";
                }
                input = evaluator.Evaluate(input, out result, out result_set);
                if (result_set && !quite)
                    QIRC.SendMessage(client, PrettyPrint(result), user, source, true);
            }
            catch (Exception e)
            {
                if (!quite)
                    QIRC.SendMessage(client, e.ToString().Replace('\n', ' '), user, source, true);
                return null;
            }
            return input;
        }

        /// <summary>
        /// Ported from Mono REPL
        /// </summary>
        protected static String EscapeString(String s)
        {
            String output = "";
            foreach (Char c in s)
            {
                if (c > 32)
                {
                    output += c;
                    continue;
                }
                switch (c)
                {
                    case '\"':
                        output += "\\\""; break;
                    case '\a':
                        output += "\\a"; break;
                    case '\b':
                        output += "\\b"; break;
                    case '\n':
                        output += "\\n";
                        break;

                    case '\v':
                        output += "\\v";
                        break;

                    case '\r':
                        output += "\\r";
                        break;

                    case '\f':
                        output += "\\f";
                        break;

                    case '\t':
                        output += "\\t";
                        break;

                    default:
                        output += String.Format("\\x{0:x}", (Int32)c);
                        break;
                }
            }
            return output;
        }

        /// <summary>
        /// Escapes a char. Ported from Mono REPL
        /// </summary>
        protected static String EscapeChar(char c)
        {
            String output = "";
            if (c == '\'')
            {
                output += "'\\''";
                return output;
            }
            if (c > 32)
            {
                output += String.Format("'{0}'", c);
                return output;
            }
            switch (c)
            {
                case '\a':
                    output += "'\\a'";
                    break;

                case '\b':
                    output += "'\\b'";
                    break;

                case '\n':
                    output += "'\\n'";
                    break;

                case '\v':
                    output += "'\\v'";
                    break;

                case '\r':
                    output += "'\\r'";
                    break;

                case '\f':
                    output += "'\\f'";
                    break;

                case '\t':
                    output += "'\\t";
                    break;

                default:
                    output += String.Format("'\\x{0:x}", (int)c);
                    break;
            }
            return output;
        }

        // Some types (System.Json.JsonPrimitive) implement
        // IEnumerator and yet, throw an exception when we
        // try to use them, helper function to check for that
        // condition
        protected static bool WorksAsEnumerable(Object obj)
        {
            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable != null)
            {
                try
                {
                    enumerable.GetEnumerator();
                    return true;
                }
                catch
                {
                    // nothing, we return false below
                }
            }
            return false;
        }

        /// <summary>
        /// Pretty prints an object. Ported from Mono REPL
        /// </summary>
        protected static String PrettyPrint(object result)
        {
            String output = "";
            if (result == null)
            {
                output += "null";
                return output; ;
            }

            if (result is Array)
            {
                Array a = (Array)result;

                output += "{ ";
                int top = a.GetUpperBound(0);
                for (int i = a.GetLowerBound(0); i <= top; i++)
                {
                    output += PrettyPrint(a.GetValue(i));
                    if (i != top)
                        output += ", ";
                }
                output += " }";
            }
            else if (result is bool)
            {
                if ((bool)result)
                    output += "true";
                else
                    output += "false";
            }
            else if (result is string)
            {
                output += result.ToString().Replace("\n", " ");
            }
            else if (result is IDictionary)
            {
                IDictionary dict = (IDictionary)result;
                int top = dict.Count, count = 0;

                output += "{";
                foreach (DictionaryEntry entry in dict)
                {
                    count++;
                    output += "{ ";
                    output += PrettyPrint(entry.Key);
                    output += ", ";
                    output += PrettyPrint(entry.Value);
                    if (count != top)
                        output += " }, ";
                    else
                        output += " }";
                }
                output += "}";
            }
            else if (WorksAsEnumerable(result))
            {
                int i = 0;
                output += "{ ";
                foreach (object item in (IEnumerable)result)
                {
                    if (i++ != 0)
                        output += ", ";

                    output += PrettyPrint(item);
                }
                output += " }";
            }
            else if (result is char)
            {
                output += EscapeChar((char)result);
            }
            else {
                output += result.ToString();
            }
            return output;
        }
    }

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
                if (!showFullPath)
                    stringBuilder.Append(msg.Location.ToString());
                else
                    stringBuilder.Append(msg.Location.ToStringFullName());
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
                for (Int32 i = 0; i < relatedSymbols.Length; i++)
                    output += String.Concat(relatedSymbols[i], msg.MessageType, ")");
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

    /// <summary>
    /// An alias of CSharp to the shorter form c
    /// </summary>
    public class C : CSharp
    {
        public override String GetName()
        {
            return "c";
        }
    }
}
