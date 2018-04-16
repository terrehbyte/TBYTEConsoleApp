using System;
using TBYTEConsole;

namespace TBYTEConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleLocator.Initialize();
            var result = ConsoleLocator.console.ProcessConsoleInput("echo test");

            System.Console.WriteLine(result);
        }
    }
}
