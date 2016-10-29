using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace TBYTEConsole
{
    public delegate string CommandCallback(int argc, string[] argv);

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
        public string Execute(int argc, string[] argv)
        {
            return Callback(argc, argv);
        }
    }

    public static class Console
    {
        static Console()
        {
            Register(new CCommand("help", ConsoleDefaultCommands.HelpCommand));
            Register(new CCommand("clear", ConsoleDefaultCommands.ClearCommand));
            Register(new CCommand("echo", ConsoleDefaultCommands.EchoCommand));
        }

        private static List<CCommand> commands = new List<CCommand>();
        private static string consoleOutput;

        public static void Register(CCommand newCommand)
        {
            commands.Add(newCommand);
        }

        private static string ProcessCommand(string command, int argc, string[] argv)
        {
            foreach(var cmd in commands)
            {
                if (cmd.Token == command)
                {
                    return cmd.Callback(argc, argv);
                }
            }

            return string.Format("{0} is not a valid command", command);
        }
        public static string ProcessConsoleInput(string command)
        {
            // blank? send it back
            if (string.IsNullOrEmpty(command))
                return consoleOutput;

            command.Trim();

            // split into command and args
            string[] input = command.Split(' ');

            string    cmd = input[0];
            string[] args = command.Substring(cmd.Length).Trim().Split(' ');

            // reject if empty command
            if (string.IsNullOrEmpty(cmd))
                return consoleOutput;

            // echo command back to console
            consoleOutput += "\n>" + command;

            // HACK: why does this work
            // if I modify consoleHistory while its += is being evaluated, it gets written back
            string result = ProcessCommand(cmd, args.Length, args);
            if (!string.IsNullOrEmpty(result))
            {
                consoleOutput += "\n" + result;
            }

            return consoleOutput;
        }

        private static string ClearHistory()
        {
            consoleOutput = string.Empty;
            return consoleOutput;
        }

        private static class ConsoleDefaultCommands
        {
            static public string HelpCommand(int argc, string[] argv)
            {
                string output = string.Empty;

                foreach (var command in commands)
                {
                    output += command.Token + "\n";
                }

                return output;
            }
            static public string ClearCommand(int argc, string[] argv)
            {
                ClearHistory();
                return string.Empty;
            }
            static public string EchoCommand(int argc, string[] argv)
            {
                StringBuilder bldr = new StringBuilder();

                foreach (var arg in argv)
                {
                    bldr.Append(arg);
                    bldr.Append(' ');
                }

                return bldr.ToString();
            }
        }
    }
}