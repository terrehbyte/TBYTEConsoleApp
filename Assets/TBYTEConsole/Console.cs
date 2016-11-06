using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace TBYTEConsole
{
    public delegate string CommandCallback(string[] Arguments);

    public struct CCommand
    {
        public readonly string Token;
        public CommandCallback Callback;

        public CCommand(string command, CommandCallback callback)
        {
            if (callback == null)
            {
                throw new NullReferenceException("Callback can not be null!");
            }

            Token = command; 
            Callback = callback;
        }

        public string Execute(string[] argv)
        {
            return Callback(argv);
        }
    }
    
    internal struct Command
    {
        public readonly string Token;
        public readonly string[] Arguments;

        public Command(string token, string[] arguments)
        {
            Token = token;
            Arguments = arguments;
        }
    }

    public static class Console
    {
        static Console()
        {
            // register default commands
            Register(new CCommand("help", ConsoleDefaultCommands.HelpCommand));
            Register(new CCommand("clear", ConsoleDefaultCommands.ClearCommand));
            Register(new CCommand("echo", ConsoleDefaultCommands.EchoCommand));
        }

        private static Dictionary<string, CCommand> commands = new Dictionary<string, CCommand>();
        private static string consoleOutput;

        public static string ProcessConsoleInput(string command)
        {
            // remove excess whitespace
            command.Trim();

            // exit if empty
            if (command == string.Empty)
                return consoleOutput;

            // echo command back to console
            consoleOutput += ">" + command + "\n";

            Command evaluation = DecomposeInput(command);

            // try command
            if (ValidateCommand(evaluation))
            {
                // HACK: can't modify consoleHistory immediately after +=
                string result = ProcessCommand(evaluation);
                if (!string.IsNullOrEmpty(result))
                    consoleOutput += result;
            }
            // try cvar
            else if (ValidateCVar(evaluation))
            {
                consoleOutput += ProcessCvar(evaluation);
            }
            // inform user that command wasn't recognized
            else
            {
                consoleOutput += evaluation.Token + " is not a valid token";
            }
            
            return consoleOutput;
        }

        public static bool Register(CCommand newCommand)
        {
            if(commands.ContainsKey(newCommand.Token))
            {
                return false;
            }

            commands[newCommand.Token] = newCommand;

            return true;
        }

        private static Command DecomposeInput(string command)
        {
            command.Trim();

            // split into command and args
            string[] input = command.Split(' ');

            string cmd = input[0];
            string[] args = null;

            if (command.Length == cmd.Length)
                args = new string[0];
            else
                args = command.Substring(cmd.Length).Trim().Split(' ');

            return new Command(cmd, args);
        }
        private static bool ValidateCommand(Command command)
        {
            return commands.ContainsKey(command.Token);
        }     
        private static string ProcessCommand(Command command)
        {
            if (commands.ContainsKey(command.Token))
                return commands[command.Token].Execute(command.Arguments);

            return string.Format("{0} is not a valid command", command.Token);
        }

        private static bool ValidateCVar(Command cvarCommand)
        {
            return CVarRegistry.ContainsCVar(cvarCommand.Token);
        }
        private static string ProcessCvar(Command cvarCommand)
        {
            if (cvarCommand.Arguments.Length == 0)
                return cvarCommand.Token + " = " + CVarRegistry.LookUp(cvarCommand.Token).ToString() + "\n";
            else
                return "Sorry, assignment is unavailable at the moment.";
        }
        
        private static class ConsoleDefaultCommands
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
                consoleOutput = string.Empty;
                return consoleOutput;
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
        }
    }
}