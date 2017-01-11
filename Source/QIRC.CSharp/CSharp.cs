/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using ChatSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Globalization;
using System.Net;
using Mono.CSharp;
using Newtonsoft.Json.Linq;

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
        /// Alternative names
        /// </summary>
        public override String[] GetAlternativeNames()
        {
            return new[] { "c" };
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
            return new []
            {
                "reset", "Clears the state of the C# shell.",
                "persistent", "Saves an expression into the class body.",
                "state", "Debugs the state of the evaluator",
                "remove", "Removes a persistend expression.",
                "stop", "Stops the current evaluation"
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
        /// The last message we got
        /// </summary>
        internal static ProtoIrcMessage lastMsg { get; set; }

        /// <summary>
        /// The thread where the C# gets evaluated
        /// </summary>
        private static BackgroundWorker worker { get; set; }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            // Update the message 
            lastMsg = message;

            // Create the Backgroundworker
            if (worker == null)
                worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;

            // Reset the evaluator
            if (StartsWithParam("reset", message.Message))
            {
                evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), new DelegateReportPrinter((state, msg) => { foreach (String s in state.Split('\n')) BotController.SendMessage(BotController.client, s, msg.User, msg.Source, true); })));
                Evaluate(client, "using System; using System.Linq; using System.Collections.Generic; using System.Collections;", message.User, message.Source, true);
                foreach (String s in CSharpData.Query.Select(c => c.Expression))
                    Evaluate(client, s, message.User, message.Source, BotController.CheckPermission(AccessLevel.ADMIN, message.level), true);
                BotController.SendMessage(client, "Cleared the C# Evaluator.", message.User, message.Source);
                return;
            }

            // Debugs the evaluator state
            if (StartsWithParam("state", message.Message))
            {
                if (Settings.Read<CSharpSettings>("csharp").useHastebin)
                {
                    String content = "Using: \r\n" + evaluator.GetUsing().Replace("\r\n", " \r\n") + "\n";
                    content += "Variables: \r\n" + evaluator.GetVars().Replace("\r\n", " \r\n") + "\n\n";
                    for (Int32 i = 0; i < CSharpData.Query.Count(); i++)
                        content += "[" + i + "] " + CSharpData.Query.ElementAt(i).Expression + "\n";
                    String response = JObject.Parse(new WebClient().UploadString("http://hastebin.com/documents", content))["key"].ToString();
                    BotController.SendMessage(client, "State: http://hastebin.com/" + response, message.User, message.User, true);
                }
                else
                {
                    BotController.SendMessage(client, "Using: " + evaluator.GetUsing().Replace("\r\n", " "), message.User, message.User, true);
                    BotController.SendMessage(client, "Variables: " + evaluator.GetVars().Replace("\r\n", ";"), message.User, message.User, true);
                    for (Int32 i = 0; i < CSharpData.Query.Count(); i++)
                        BotController.SendMessage(client, "[" + i + "] " + CSharpData.Query.ElementAt(i).Expression, message.User, message.User, true);
                }
                if (message.IsChannelMessage) BotController.SendMessage(client, "I sent you the current state of the evaluator.", message.User, message.Source);
                return;
            }

            // Removes an expression
            if (StartsWithParam("remove", message.Message))
            {
                String text = message.Message;
                String nr = StripParam("remove", ref text);
                Int32 index = 0;
                if (!Int32.TryParse(nr, out index))
                    BotController.SendMessage(client, "Please enter a valid index!", message.User, message.Source);
                if (!(CSharpData.Query.Count() > index))
                    BotController.SendMessage(client, "Please enter a valid index!", message.User, message.Source);
                CSharpData.Query.Delete(e => e.Index == index);
                return;
            }

            // Saves an expression
            if (StartsWithParam("persistent", message.Message))
            {
                String text = message.Message;
                StripParam("persistent", ref text);
                CSharpData.Query.Insert(text.Trim());
                message.Message = text.Trim();
            }

            // Stops the current evaluation
            if (StartsWithParam("stop", message.Message))
            {
                if (worker == null || !worker.IsBusy)
                {
                    BotController.SendMessage(client, "No evaluation running!", message.User, message.Source);
                    return;
                }
                worker.Dispose();
                worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                BotController.SendMessage(client, "Aborted the evaluation that was currently running.", message.User, message.Source);
                return;
            }

            // Create the Evaluator
            if (evaluator == null)
            {
                evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), new DelegateReportPrinter((state, msg) => { foreach (String s in state.Split('\n')) BotController.SendMessage(BotController.client, s, msg.User, msg.Source, true); })));
                Evaluate(client, "using System; using System.Linq; using System.Collections.Generic; using System.Collections;", message.User, message.Source, BotController.CheckPermission(AccessLevel.ADMIN, message.level), true);
                foreach (String s in CSharpData.Query.Select(e => e.Expression))
                    Evaluate(client, s, message.User, message.Source, true);
            }

            // Evaluate!
            if (!worker.IsBusy)
            {
                worker.Dispose();
                worker = new BackgroundWorker();
                worker.DoWork += delegate(Object sender, DoWorkEventArgs e)
                {
                    Evaluate(client, message.Message, message.User, message.Source,
                        BotController.CheckPermission(AccessLevel.ADMIN, message.level));
                };
                worker.RunWorkerAsync();
            }
            else
                BotController.SendMessage(client, "There is already an evaluation going on. Please wait until it terminates.", message.User, message.Source);
        }


        /// <summary>
        /// Adds the C# Settings to the config
        /// </summary>
        /// IrcClient Functions
        public override void OnLoad()
        {
            SettingsFile file = null;
            Settings.GetFile("settings", ref file);
            file.Add("csharp", new CSharpSettings());
        }

        /// <summary>
        /// Evaluates a C# expression. Ported from Mono REPL
        /// </summary>
        protected String Evaluate(IrcClient client, String input, String user, String source, Boolean admin, Boolean quite = false)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            try
            {
                Boolean result_set;
                Object result;
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
                    BotController.SendMessage(client, "You tried to use a forbidden type, method or namespace!", user, source);
                    return "";
                }
                input = evaluator.Evaluate(input, out result, out result_set);
                if (result_set && !quite)
                    BotController.SendMessage(client, PrettyPrint(result), user, source, true);
            }
            catch (Exception e)
            {
                if (!quite)
                    BotController.SendMessage(client, e.ToString().Replace('\n', ' '), user, source, true);
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
                        output += $"\\x{(Int32) c:x}";
                        break;
                }
            }
            return output;
        }

        /// <summary>
        /// Escapes a char. Ported from Mono REPL
        /// </summary>
        protected static String EscapeChar(Char c)
        {
            String output = "";
            if (c == '\'')
            {
                output += "'\\''";
                return output;
            }
            if (c > 32)
            {
                output += $"'{c}'";
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
                    output += $"'\\x{(Int32) c:x}";
                    break;
            }
            return output;
        }

        // Some types (System.Json.JsonPrimitive) implement
        // IEnumerator and yet, throw an exception when we
        // try to use them, helper function to check for that
        // condition
        protected static Boolean WorksAsEnumerable(Object obj)
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
        protected static String PrettyPrint(Object result)
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
                Int32 top = a.GetUpperBound(0);
                for (Int32 i = a.GetLowerBound(0); i <= top; i++)
                {
                    output += PrettyPrint(a.GetValue(i));
                    if (i != top)
                        output += ", ";
                }
                output += " }";
            }
            else if (result is Boolean)
            {
                if ((Boolean)result)
                    output += "true";
                else
                    output += "false";
            }
            else if (result is String)
            {
                output += result.ToString().Replace("\n", " ");
            }
            else if (result is IDictionary)
            {
                IDictionary dict = (IDictionary)result;
                Int32 top = dict.Count, count = 0;

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
                Int32 i = 0;
                output += "{ ";
                foreach (Object item in (IEnumerable)result)
                {
                    if (i++ != 0)
                        output += ", ";

                    output += PrettyPrint(item);
                }
                output += " }";
            }
            else if (result is Char)
            {
                output += EscapeChar((Char)result);
            }
            else {
                output += result.ToString();
            }
            return output;
        }
    }
}
