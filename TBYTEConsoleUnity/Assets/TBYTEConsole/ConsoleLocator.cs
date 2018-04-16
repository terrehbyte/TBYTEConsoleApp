using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TBYTEConsole.Utilities;

namespace TBYTEConsole
{
    public static class ConsoleLocator
    {
        public static CVarRegistry cvarRegistry { get; private set; }
        public static CmdRegistry  cmdRegistry { get; private set; }
        public static Console      console { get; private set; }

        public static void Initialize()
        {
            cvarRegistry = new StandardCVarRegistry();
            cmdRegistry = new StandardCmdRegistry();
            console = new StandardConsole();
        }

        static ConsoleLocator()
        {
            // make this editor only?
            Initialize();
        }
    }
}