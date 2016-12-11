﻿using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using TBYTEConsole.Utilities;

namespace TBYTEConsole
{
    public struct CCommand
    {
        public readonly string token;
        public readonly Func<string[], string> callback;

        public CCommand(string commandName, Func<string[], string> callback)
        {
            commandName.ThrowIfNullOrEmpty("commandName");
            callback.ThrowIfNull("callback");

            this.token = commandName;
            this.callback = callback;
        }

        public string Execute(string[] argv)
        {
            return callback(argv);
        }
    }

    public class CmdRegistryException : Exception
    {
        public CmdRegistryException()
        {
        }
        public CmdRegistryException(string message)
            : base(message)
        {
        }
        public CmdRegistryException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public abstract class CmdRegistry
    {
        public abstract bool Register(CCommand newCommand);
        public abstract bool ContainsCmd(string cmdName);
        public abstract string Execute(string cmdName, string[] argv);
    }

    public class StandardCmdRegistry : CmdRegistry
    {
        private static Dictionary<string, CCommand> commands = new Dictionary<string, CCommand>();

        public StandardCmdRegistry()
        {
            // register default commands
            Register(new CCommand("help", ConsoleCoreCommands.HelpCommand));
            Register(new CCommand("clear", ConsoleCoreCommands.ClearCommand));
            Register(new CCommand("echo", ConsoleCoreCommands.EchoCommand));
            Register(new CCommand("list", ConsoleCoreCommands.ListCommand));
        }

        public override bool Register(CCommand newCommand)
        {
            if (commands.ContainsKey(newCommand.token))
            {
                return false;
            }

            commands[newCommand.token] = newCommand;

            return true;
        }
        public override bool ContainsCmd(string cmdName)
        {
            return commands.ContainsKey(cmdName);
        }
        public override string Execute(string cmdName, string[] argv)
        {
            return commands[cmdName].Execute(argv);
        }
        private static CCommand[] GatherCommands()
        {
            throw new System.NotImplementedException();
        }

        private static class ConsoleCoreCommands
        {
            static public string HelpCommand(string[] Arguments)
            {
                string output = string.Empty;

                foreach (var command in commands.Keys)
                {
                    output += command + "\n";
                }

                return output;
            }
            static public string ClearCommand(string[] Arguments)
            {
                Console.consoleOutput = string.Empty;
                return Console.consoleOutput;
            }
            static public string EchoCommand(string[] Arguments)
            {
                StringBuilder bldr = new StringBuilder();

                foreach (var arg in Arguments)
                {
                    bldr.Append(arg);
                    bldr.Append(' ');
                }

                return bldr.ToString() + "\n";
            }
            static public string ListCommand(string[] Arguments)
            {
                string output = string.Empty;

                var keyArray = ConsoleSvc.cvarRegistry.GetCVarNames();

                foreach (var key in keyArray)
                {
                    output += key + "\n";
                }

                return output;
            }
        }
    }
}