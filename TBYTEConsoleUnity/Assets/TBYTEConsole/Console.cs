using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using TBYTEConsole.Utilities;

namespace TBYTEConsole
{
    public abstract class Console
    {
        public string consoleOutput;

        public abstract string ProcessConsoleInput(string command);
    }

    public class StandardConsole : Console
    {
        private struct ConsoleExpression
        {
            public readonly string token;
            public readonly string[] arguments;

            public ConsoleExpression(string token, string[] arguments)
            {
                this.token = token;
                this.arguments = arguments;
            }
        }

        public override string ProcessConsoleInput(string command)
        {
            // remove excess whitespace
            command.Trim();

            // exit if empty
            if (command == string.Empty)
                return consoleOutput;

            // echo command back to console
            consoleOutput += ">" + command + "\n";

            ConsoleExpression evaluation = DecomposeInput(command);

            string executionResult = ValidateCommand(evaluation) ? ProcessCommand(evaluation) :
                                     ValidateCVar(evaluation)    ? ProcessCvar(evaluation)    :
                                     evaluation.token + " is not a valid token\n";

            consoleOutput += executionResult;
            
            return consoleOutput;
        }

        private ConsoleExpression DecomposeInput(string command)
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

            return new ConsoleExpression(cmd, args);
        }
        private bool ValidateCommand(ConsoleExpression command)
        {
            return ConsoleLocator.cmdRegistry.ContainsCmd(command.token);
        }     
        private string ProcessCommand(ConsoleExpression command)
        {
            if (ConsoleLocator.cmdRegistry.ContainsCmd(command.token))
                return ConsoleLocator.cmdRegistry.Execute(command.token, command.arguments);

            return string.Format("{0} is not a valid command", command.token);
        }

        private bool ValidateCVar(ConsoleExpression cvarCommand)
        {
            return ConsoleLocator.cvarRegistry.ContainsCVar(cvarCommand.token);
        }
        private string ProcessCvar(ConsoleExpression cvarCommand)
        {
            if (cvarCommand.arguments.Length == 0)
                return cvarCommand.token + " = " + ConsoleLocator.cvarRegistry.LookUp(cvarCommand.token).ToString() + "\n";
            else
            {
                string reassembledArgs = string.Empty;

                for (int i = 0; i < cvarCommand.arguments.Length - 1; ++i)
                {
                    reassembledArgs += cvarCommand.arguments[i] + " ";
                }
                reassembledArgs += cvarCommand.arguments[cvarCommand.arguments.Length - 1];

                try
                {
                    ConsoleLocator.cvarRegistry.WriteTo(cvarCommand.token, reassembledArgs);
                }
                catch (Exception ex)
                {
                    if (ex is CVarRegistryException)
                    {
                        return "Failed to assign to " + cvarCommand.token + "\n";
                    }
                }
                return string.Empty;
            }
        }
    }
}