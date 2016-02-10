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
using System.CodeDom.Compiler;

/// Microsoft
using Microsoft.CSharp;

/// <summary>
/// Here's everything that is an IrcCommand
/// </summary>
namespace QIRC.Commands
{
    /// <summary>
    /// This is the implementation for the csharp command. It will take C# code, compile
    /// and execute it in a Sandbox
    /// </summary>
    public class Choose : IrcCommand
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
        /// An example for using the command.
        /// </summary>
        /// <returns></returns>
        public override String GetExample()
        {
            return Settings.Read<String>("control") + GetName() + " Math.PI";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            String code = @"using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class Eval
{
    public static Object Evaluate()
    {
        return " + message.Message.Trim() + @";
    }
}";
            /// If we have no args
            /// Get a CSharp Code Compiler
            CSharpCodeProvider csharp = new CSharpCodeProvider();

            /// Configure it
            CompilerParameters param = new CompilerParameters(new[] { "System.dll", "System.Core.dll" });
            CompilerResults result = csharp.CompileAssemblyFromSource(param, code
                .Replace("System.IO", "")
                .Replace("System.Xml", "")
                .Replace("System.Diagnostics.Process", "")
                .Replace("while(true)", "")
                .Replace("while (true)", "")
                .Replace("Console.ReadKey()", "")
                .Replace("Console.Read()", "")
                .Replace("Console.ReadLine()", "")
            );

            /// Execute it
            foreach (Object s in result.Errors) Logging.Log(s, Logging.Level.ERROR);
            Object value = result.CompiledAssembly.GetType("Eval").GetMethod("Evaluate").Invoke(null, null);
            QIRC.SendMessage(client, value.ToString(), message.User, message.Source);
        }
    }
}
