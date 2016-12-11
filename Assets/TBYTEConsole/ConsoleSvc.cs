using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TBYTEConsole.Utilities;

namespace TBYTEConsole
{
    public static class ConsoleSvc
    {
        public static CVarRegistry cvarRegistry { get; private set; }
        public static CmdRegistry  cmdRegistry { get; private set; }

        static ConsoleSvc()
        {
            cvarRegistry = new StandardCVarRegistry();
            cmdRegistry = new StandardCmdRegistry();
        }
    }
}