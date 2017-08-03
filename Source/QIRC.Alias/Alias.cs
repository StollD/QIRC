/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ChatSharp;
using Microsoft.CSharp;
using QIRC.Configuration;
using QIRC.IRC;
using QIRC.Plugins;
using QIRC.Serialization;

namespace QIRC.Alias
{
    /// <summary>
    /// This command creates an alias command for another command
    /// </summary>
    public class Alias : IrcCommand
    {
        /// <summary>
        /// The Access Level that is needed to execute the command
        /// </summary>
        public override AccessLevel GetAccessLevel()
        {
            return AccessLevel.ADMIN;
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
            return new []
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
            // Remove an alias
            if (StartsWithParam("remove", message.Message))
            {
                String text = message.Message;
                String id = StripParam("remove", ref text).ToLower();
                if (AliasData.Query.Count(s => s.Name == id) == 1)
                {
                    AliasData a = AliasData.Query.First(s => s.Name == id);
                    if (!BotController.CheckPermission(a.Level, message.level))
                    {
                        BotController.SendMessage(client, "You don't have the permission to remove this alias! Only " + a.Level + " can remove this alias! You are " + message.level + ".", message.User, message.Source);
                        return;
                    }
                    AliasData.Query.Delete(s => s.Name == id);
                    PluginManager.commands.RemoveWhere(i => i.GetName() == id);
                    BotController.SendMessage(client, "Removed the alias \"" + id + "\"", message.User, message.Source);
                }
                else
                    BotController.SendMessage(client, "The alias \"" + id + "\" does not exist!", message.User, message.Source);
                return;
            }

            // Add one
            if (StartsWithParam("create", message.Message))
            {
                String text = message.Message;
                String name = StripParam("create", ref text).ToLower();
                if (AliasData.Query.Count(a => a.Name == name) != 0)
                {
                    BotController.SendMessage(client, "This alias does already exist!", message.User, message.Source);
                    return;
                }
                if (!StartsWithParam("structure", text))
                {
                    IrcCommand command = CreateAlias(AliasData.Query.Insert(name, text.Trim(), null, false));
                    PluginManager.commands.Add(command);
                }
                else
                {
                    String regex = StripParam("structure", ref text, true);
                    Boolean escape = StartsWithParam("escape", text);
                    if (escape)
                        StripParam("escape", ref text);
                    IrcCommand command = CreateAlias(AliasData.Query.Insert(name, text.Trim(), regex, escape));
                    PluginManager.commands.Add(command);
                }
                BotController.SendMessage(client, "Aliased \"" + text.Trim() + "\" to \"" + Settings.Read<String>("control") + name + "\"", message.User, message.Source);
                return;
            }

            // Edit description
            if (StartsWithParam("description", message.Message))
            {
                String text = message.Message;
                String id = StripParam("description", ref text);
                if (AliasData.Query.Count(a => a.Name == id) == 0)
                {
                    BotController.SendMessage(client, "This alias doesn't exist! You can add it using the -create attribute.", message.User, message.Source);
                    return;
                }
                AliasData al = AliasData.Query.First(a => a.Name == id);
                if (!BotController.CheckPermission(al.Level, message.level))
                {
                    BotController.SendMessage(client, "You don't have the permission to edit this alias! Only " + al.Level + " can edit this alias! You are " + message.level + ".", message.User, message.Source);
                    return;
                }
                al.Description = text.Trim();
                BotController.Database.Update(al);
                PluginManager.commands.RemoveWhere(a => a.GetName() == id);
                PluginManager.commands.Add(CreateAlias(al));
                BotController.SendMessage(client, "Updated the description for \"" + Settings.Read<String>("control") + al.Name + "\"", message.User, message.Source);
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
                    BotController.SendMessage(client, "Please enter a valid AccessLevel!", message.User, message.Source);
                    return;
                }
                if (AliasData.Query.Count(a => a.Name == id) == 0)
                {
                    BotController.SendMessage(client, "This alias doesn't exist! You can add it using the -create attribute.", message.User, message.Source);
                    return;
                }
                AliasData al = AliasData.Query.First(a => a.Name == id);
                if (!BotController.CheckPermission(al.Level, message.level))
                {
                    BotController.SendMessage(client, "You don't have the permission to edit this alias! Only " + al.Level + " can edit this alias! You are " + message.level + ".", message.User, message.Source);
                    return;
                }
                al.Level = level;
                BotController.Database.Update(al);
                PluginManager.commands.RemoveWhere(a => a.GetName() == id);
                PluginManager.commands.Add(CreateAlias(al));
                BotController.SendMessage(client, "Updated the access level for \"" + Settings.Read<String>("control") + al.Name + "\"", message.User, message.Source);
                return;
            }

            // Edit example
            if (StartsWithParam("example", message.Message))
            {
                String text = message.Message;
                String id = StripParam("example", ref text);
                if (AliasData.Query.Count(a => a.Name == id) == 0)
                {
                    BotController.SendMessage(client, "This alias doesn't exist! You can add it using the -create attribute.", message.User, message.Source);
                    return;
                }
                AliasData al = AliasData.Query.First(a => a.Name == id);
                if (!BotController.CheckPermission(al.Level, message.level))
                {
                    BotController.SendMessage(client, "You don't have the permission to edit this alias! Only " + al.Level + " can edit this alias! You are " + message.level + ".", message.User, message.Source);
                    return;
                }
                al.Example = Settings.Read<String>("control") + al.Name + " " + text.Trim();
                BotController.Database.Update(al);
                PluginManager.commands.RemoveWhere(a => a.GetName() == id);
                PluginManager.commands.Add(CreateAlias(al));
                BotController.SendMessage(client, "Updated the example for \"" + Settings.Read<String>("control") + al.Name + "\"", message.User, message.Source);
                return;
            }
        }

        /// <summary>
        /// Transforms an Alias into a C# class using Reflection.Emit
        /// </summary>
        /// Editors note: I never want to see the IL of this method.
        public static IrcCommand CreateAlias(AliasData alias)
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
            parameters.ReferencedAssemblies.Add(typeof(BotController).Assembly.Location);
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
            return AccessLevel." + Enum.GetName(typeof(AccessLevel), alias.Level) + @";
        }

        public override String GetName()
        {
            return " + "\"" + alias.Name + "\"" + @";
        }

        public override String GetDescription()
        {
            return " + "\"" + alias.Description + "\"" + @";
        }

        public override Boolean IsSerious()
        {
            return " + alias.Serious.ToString().ToLower() + @";
        }

        public override String GetExample()
        {
            return " + "\"" + alias.Example + "\"" + @";
        }

        public override void RunCommand(IrcClient client, ProtoIrcMessage message)
        {
            String regex = @" + "\"" + alias.Regex + "\"" + @";
            String command = " + "\"" + alias.Command.Replace(@"\", @"\\").Replace("\"", @"\" + "\"") + "\"" + @";
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
                    message.Message = String.Format(command, groups.Where(g => g.Success && groups.IndexOf(g) != 0).Select(g => g.Value.Trim()" + (alias.Escape ? ".Replace(\"\\\"\", \"\\\\\\\"\"" + ")" : "") + @").ToArray());
                }
                catch
                {
                    return;
                }
            }
            BotController.HandleCommand(message, client, false);
        }
    }
}
");
            Type type = result.CompiledAssembly.GetType("QIRC.Commands." + id);
            return (IrcCommand)Activator.CreateInstance(type);
        }

        /// <summary>
        /// Load the definitions
        /// </summary>
        public override void OnLoad()
        {
            // Create them
            foreach (AliasData alias in AliasData.Query.ToList())
            {
                IrcCommand command = CreateAlias(alias);
                PluginManager.commands.Add(command);
            }
        }
    }
}
