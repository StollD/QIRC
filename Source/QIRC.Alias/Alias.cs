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
using System.Linq;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Security.Cryptography;
using Microsoft.CSharp;

namespace QIRC.Commands
{
    /// <summary>
    /// This command creates an alias command for another command
    /// </summary>
    public class Alias : IrcCommand
    {
        /// <summary>
        /// A list that stores all aliases
        /// </summary>
        public static SerializeableList<_Alias> alias { get; set; }

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
            return "alias";
        }

        /// <summary>
        /// Returns a description of the command
        /// </summary>
        public override String GetDescription()
        {
            return "Creates an alias command for another command (sequence)";
        }

        /// <summary>
        /// The Parameters of the Command
        /// </summary>
        public override String[] GetParameters()
        {
            return new String[]
            {
                "create", "Creates an alias with the given name",
                "remove", "Removes the alias with the given name",
                "structure", "Defines the structure for a new alias",
                "escape", "Escapes the params before passing them to the alias command.",
                "description", "Sets the description for an alias",
                "level", "Sets the access level for an alias",
                "example", "Adds a proper example for the alias"
            };
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
            return Settings.Read<String>("control") + GetName() + " -create:roll30 -structure:^[0-9]+$ !roll {0}d30";
        }

        /// <summary>
        /// Here we run the command and evaluate the parameters
        /// </summary>
        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            // Create the list
            if (alias == null)
                alias = new SerializeableList<_Alias>("alias");

            // Remove an alias
            if (StartsWithParam("remove", message.Message))
            {
                String text = message.Message;
                String id = StripParam("remove", ref text).ToLower();
                if (alias.Count(s => s.name == id) == 1)
                {
                    _Alias a = alias.Find(s => s.name == id);
                    if (!QIRC.CheckPermission(a.level, message.level))
                    {
                        QIRC.SendMessage(client, "You don't have the permission to remove this alias! Only " + a.level + " can remove this alias! You are " + message.level + ".", message.User, message.Source);
                        return;
                    }
                    alias.RemoveAll(s => s.name == id);
                    PluginManager.commands.RemoveWhere(i => i.GetName() == id);
                    QIRC.SendMessage(client, "Removed the alias \"" + id + "\"", message.User, message.Source);
                }
                else
                    QIRC.SendMessage(client, "The alias \"" + id + "\" does not exist!", message.User, message.Source);
                return;
            }

            // Add one
            if (StartsWithParam("create", message.Message))
            {
                String text = message.Message;
                String name = StripParam("create", ref text).ToLower();
                if (alias.Count(a => a.name == name) != 0)
                {
                    QIRC.SendMessage(client, "This alias does already exist!", message.User, message.Source);
                    return;
                }
                if (!StartsWithParam("structure", text))
                {
                    _Alias alias_ = new _Alias { name = name, command = text.Trim(), level = AccessLevel.NORMAL, example = Settings.Read<String>("control") + name, serious = true };
                    IrcCommand command = CreateAlias(alias_);
                    Alias.alias.Add(alias_);
                    PluginManager.commands.Add(command);
                }
                else
                {
                    String regex = StripParam("structure", ref text);
                    Boolean escape = StartsWithParam("escape", text);
                    if (escape)
                        StripParam("escape", ref text);
                    _Alias alias_ = new _Alias { name = name, command = text.Trim(), level = AccessLevel.NORMAL, example = Settings.Read<String>("control") + name, serious = true, regex = regex, escape = escape };
                    IrcCommand command = CreateAlias(alias_);
                    Alias.alias.Add(alias_);
                    PluginManager.commands.Add(command);
                }
                QIRC.SendMessage(client, "Aliased \"" + text.Trim() + "\" to \"" + Settings.Read<String>("control") + name + "\"", message.User, message.Source);
                return;
            }

            // Edit description
            if (StartsWithParam("description", message.Message))
            {
                String text = message.Message;
                String id = StripParam("description", ref text);
                if (alias.Count(a => a.name == id) == 0)
                {
                    QIRC.SendMessage(client, "This alias doesn't exist! You can add it using the -create attribute.", message.User, message.Source);
                    return;
                }
                Int32 index = alias.IndexOf(alias.Find(a => a.name == id));
                _Alias al = alias[index];
                if (!QIRC.CheckPermission(al.level, message.level))
                {
                    QIRC.SendMessage(client, "You don't have the permission to edit this alias! Only " + al.level + " can edit this alias! You are " + message.level + ".", message.User, message.Source);
                    return;
                }
                al.description = text.Trim();
                alias[index] = al;
                PluginManager.commands.RemoveWhere(a => a.GetName() == id);
                PluginManager.commands.Add(CreateAlias(al));
                QIRC.SendMessage(client, "Updated the description for \"" + Settings.Read<String>("control") + alias[index].name + "\"", message.User, message.Source);
                return;
            }

            // Edit access
            if (StartsWithParam("level", message.Message))
            {
                String text = message.Message;
                String id = StripParam("level", ref text);
                AccessLevel level = AccessLevel.NORMAL;
                if (!Enum.TryParse(text.Trim(), out level))
                {
                    QIRC.SendMessage(client, "Please enter a valid AccessLevel!", message.User, message.Source);
                    return;
                }
                if (alias.Count(al => al.name == id) == 0)
                {
                    QIRC.SendMessage(client, "This alias doesn't exist! You can add it using the -create attribute.", message.User, message.Source);
                    return;
                }
                Int32 index = alias.IndexOf(alias.Find(al => al.name == id));
                _Alias a = alias[index];
                if (!QIRC.CheckPermission(a.level, message.level))
                {
                    QIRC.SendMessage(client, "You don't have the permission to edit this alias! Only " + a.level + " can edit this alias! You are " + message.level + ".", message.User, message.Source);
                    return;
                }
                a.level = level;
                alias[index] = a;
                PluginManager.commands.RemoveWhere(al => al.GetName() == id);
                PluginManager.commands.Add(CreateAlias(a));
                QIRC.SendMessage(client, "Updated the access level for \"" + Settings.Read<String>("control") + alias[index].name + "\"", message.User, message.Source);
                return;
            }

            // Edit example
            if (StartsWithParam("example", message.Message))
            {
                String text = message.Message;
                String id = StripParam("example", ref text);
                if (alias.Count(a => a.name == id) == 0)
                {
                    QIRC.SendMessage(client, "This alias doesn't exist! You can add it using the -create attribute.", message.User, message.Source);
                    return;
                }
                Int32 index = alias.IndexOf(alias.Find(a => a.name == id));
                _Alias al = alias[index];
                if (!QIRC.CheckPermission(al.level, message.level))
                {
                    QIRC.SendMessage(client, "You don't have the permission to edit this alias! Only " + al.level + " can edit this alias! You are " + message.level + ".", message.User, message.Source);
                    return;
                }
                al.example = Settings.Read<String>("control") + al.name + " " + text.Trim();
                alias[index] = al;
                PluginManager.commands.RemoveWhere(a => a.GetName() == id);
                PluginManager.commands.Add(CreateAlias(al));
                QIRC.SendMessage(client, "Updated the example for \"" + Settings.Read<String>("control") + alias[index].name + "\"", message.User, message.Source);
                return;
            }
        }

        /// <summary>
        /// Transforms an Alias into a C# class using Reflection.Emit
        /// </summary>
        /// Editors note: I never want to see the IL of this method.
        public static IrcCommand CreateAlias(_Alias alias)
        {
            String id = BitConverter.ToString(SHA1.Create().ComputeHash(BitConverter.GetBytes(alias.GetHashCode())));
            id = id.Replace("-", "")
                   .Replace("0", "A")
                   .Replace("1", "B")
                   .Replace("2", "C")
                   .Replace("3", "D")
                   .Replace("4", "E")
                   .Replace("5", "F")
                   .Replace("6", "G")
                   .Replace("7", "H")
                   .Replace("8", "I")
                   .Replace("9", "J");
            CSharpCodeProvider compiler = new CSharpCodeProvider(new Dictionary<String, String> { { "CompilerVersion", "v4.0" } });
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add(typeof(IrcClient).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(QIRC).Assembly.Location);
            parameters.TreatWarningsAsErrors = false;
            CompilerResults result = compiler.CompileAssemblyFromSource(parameters,
@"
using ChatSharp;
using ChatSharp.Events;

using QIRC;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.CodeDom.Compiler;

namespace QIRC.Commands
{
    public class " + id + @" : IrcCommand
    {
        public override AccessLevel GetAccessLevel()
        {
            return AccessLevel." + Enum.GetName(typeof(AccessLevel), alias.level) + @";
        }

        public override String GetName()
        {
            return " + "\"" + alias.name + "\"" + @";
        }

        public override String GetDescription()
        {
            return " + "\"" + alias.description + "\"" + @";
        }

        public override Boolean IsSerious()
        {
            return " + alias.serious.ToString().ToLower() + @";
        }

        public override String GetExample()
        {
            return " + "\"" + alias.example + "\"" + @";
        }

        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            String regex = @" + "\"" + alias.regex + "\"" + @";
            String command = " + "\"" + alias.command.Replace(@"\", @"\\").Replace("\"", @"\" + "\"") + "\"" + @";
            if (regex == " + "\"\"" + @")
                message.Message = command;
            else
            {
                try
                {
                    Regex regex_ = new Regex(regex);
                    if (!regex_.IsMatch(message.Message))
                        return;
                    Match match = regex_.Match(message.Message);
                    List<Group> groups = match.Groups.Cast<Group>().ToList();
                    message.Message = String.Format(command, groups.Where(g => g.Success && groups.IndexOf(g) != 0).Select(g => g.Value.Trim()" + (alias.escape ? ".Replace(\"\\\"\", \"\\\\\\\"\"" + ")" : "") + @").ToArray());
                }
                catch
                {
                    return;
                }
            }
            QIRC.HandleCommand(message, client, false);
        }
    }
}
");
            Type type = result.CompiledAssembly.GetType("QIRC.Commands." + id);
            return (IrcCommand)Activator.CreateInstance(type);
        }

        /// <summary>
        /// Internal representation of an alias
        /// </summary>
        public struct _Alias
        {
            public String name;
            public String description;
            public AccessLevel level;
            public Boolean serious;
            public String example;
            public String command;
            public String regex;
            public Boolean escape;
        }
    }

    /// <summary>
    /// Loads the Alias definitions
    /// </summary>
    public class AliasLoader : IrcPlugin
    {
        /// <summary>
        /// Load the definitions
        /// </summary>
        public override void OnLoad()
        {
            /// Create the list
            if (Alias.alias == null)
                Alias.alias = new SerializeableList<Alias._Alias>("alias");

            /// Create them
            foreach (Alias._Alias alias in Alias.alias)
            {
                IrcCommand command = Alias.CreateAlias(alias);
                PluginManager.commands.Add(command);
            }
        }
    }
}
