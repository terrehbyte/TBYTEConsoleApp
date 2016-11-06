using UnityEngine;
using System;
using System.Collections.Generic;

namespace TBYTEConsole
{
    public class CVarRegistryException : Exception
    {
        public CVarRegistryException()
        {
        }

        public CVarRegistryException(string message)
            : base (message)
        {
        }

        public CVarRegistryException(string message, Exception inner)
            : base (message, inner)
        {
        }
    }

    public static class CVarRegistry
    {
        private class CVarData
        {
            public CVarData(Type dataType, string initialValue)
            {
                type = dataType;
                value = initialValue;
            }

            public Type type;
            public string value;
        }

        static CVarRegistry()
        {
            CVarDefaults.Register();
        }

        static Dictionary<string, CVarData> registry = new Dictionary<string, CVarData>();

        // Adds an entry to the CVarRegistry
        static public void Register<T>(CVar<T> newCvar) where T : IConvertible
        {
            if(registry.ContainsKey(newCvar.name))
            {
                return;
                //throw new CVarRegistryException(string.Format("CVar Registry already contains an entry for {0}", newCvar.name));
            }

            CVarData babyCVar;
            try
            {
                babyCVar = new CVarData(typeof(T), default(T).ToString());
            }
            catch (NullReferenceException)
            {
                // special logic for Strings
                if(typeof(T) == typeof(String))
                {
                    babyCVar = new CVarData(typeof(T), string.Empty);
                }
                else
                {
                    throw;
                }
            }
            registry[newCvar.name] = babyCVar;
        }

        // Returns an object for a given key, as the type given
        // - Asserts if the given key does not have a value
        static public T LookUp<T>(string cvarName) where T : IConvertible
        {
            return (T)Convert.ChangeType(registry[cvarName].value, typeof(T));
        }

        static public string LookUp(string cvarName)
        {
            return registry[cvarName].value;
        }

        static public void WriteTo<T>(string cvarName, T value) where T : IConvertible
        {
            registry[cvarName].value = Convert.ToString(value);
        }

        static public void WriteTo(string cvarName, string value)
        {
            var data = Convert.ChangeType(value, registry[cvarName].type);

            registry[cvarName].value = value;
        }

        // Returns true if the CVar is registered with this registry
        static public bool ContainsCVar(string cvarName)
        {
            return registry.ContainsKey(cvarName);
        }

        public static class CVarDefaults
        {
            public static void Register()
            {
                new CVar<float>("version", 0.01f);
                new CVar<string>("cl_playerName", "terrehbyte");
            }
        }
    }



    // Type-safe accessor for a particular CVar
    public class CVar<T> where T : IConvertible
    {
        public readonly string name;

        public CVar(string cvarName)
        {
            name = cvarName;

            CVarRegistry.Register(this);
        }

        public CVar(string cvarName, T initialValue) : this(cvarName)
        {
            CVarRegistry.WriteTo(cvarName, initialValue);
        }

        public static implicit operator T(CVar<T> cvar)
        {
            return CVarRegistry.LookUp<T>(cvar.name);
        }
    }
}