using UnityEngine;
using System;
using System.Collections.Generic;

namespace TBYTEConsole
{
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
}