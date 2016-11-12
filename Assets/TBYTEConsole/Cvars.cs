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

        // Adds an entry to the CVarRegistry by name and sets an initial value
        static public CVar<T> Register<T>(string cvarName, T initialValue) where T : IConvertible
        {
            if (ContainsCVar(cvarName)) { return null; }

            CVarData babyCVar;
            try
            {
                babyCVar = new CVarData(typeof(T), initialValue.ToString());
            }
            catch (NullReferenceException ex)
            {
                // perform special logic for strings
                //   - `default(String)` yields `null` since it's a ref type
                if (typeof(T) == typeof(String))
                {
                    babyCVar = new CVarData(typeof(T), string.Empty);
                }
                else
                {
                    throw new CVarRegistryException("Failed to create internal data store for CVar.", ex);
                }
            }

            registry[cvarName] = babyCVar;

            return new CVar<T>(cvarName, true);
        }

        // Adds an entry to the CVarRegistry by name
        static public CVar<T> Register<T>(string cvarName) where T : IConvertible
        {
            return Register(cvarName, default(T));
        }

        // Adds an entry to the CVarRegistry by existing CVar
        static public CVar<T> Register<T>(CVar<T> newCvar) where T : IConvertible
        {
            return Register<T>(newCvar.name);
        }

        // Returns an object for a given key, as the type given
        // - Asserts if the given key does not have a value
        static public T LookUp<T>(string cvarName) where T : IConvertible
        {
            return (T)Convert.ChangeType(registry[cvarName].value, typeof(T));
        }

        // Returns the string-backed data store for a given CVar
        static public string LookUp(string cvarName)
        {
            return registry[cvarName].value;
        }

        // Assigns a value to the specified CVar entry
        // - Raises an exception if the value could not be converted into a string
        static public void WriteTo<T>(string cvarName, T value) where T : IConvertible
        {
            try
            {
                registry[cvarName].value = Convert.ToString(value);
            }
            catch (Exception ex)
            {
                throw new CVarRegistryException("Failed to convert value to string for storage.", ex);
            }
        }

        // Assigns a string value to the specified CVar entry
        // - Raises an exception if the string could not be converted into CVar type
        static public void WriteTo(string cvarName, string value)
        {
            // perform runtime check to see if string value is valid for this type
            try
            {
                var data = Convert.ChangeType(value, registry[cvarName].type);
            }
            catch (Exception ex)
            {
                throw new CVarRegistryException("Failed to convert value to CVar data type.", ex);
            }

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
                CVarRegistry.Register("version", "0.1");
                CVarRegistry.Register("cl_playerName", "PlayerName");
            }
        }
    }

    // Type-safe accessor for registered CVar
    // - Bypassing the registration check will allow the creation of accessors for non-existant CVars
    public class CVar<T> where T : IConvertible
    {
        // The name of the CVar accessed by this object.
        public readonly string name;

        // The type of the CVar accessed by this object.
        public readonly Type type;

        // The value of the CVar as its type.
        public T value
        {
            get
            {
                return CVarRegistry.LookUp<T>(name);
            }
        }

        // The string-backed value of the CVar.
        public string stringValue
        {
            get
            {
                return CVarRegistry.LookUp(name);
            }
        }

        // Creates an accessor to a registered CVar
        // - Set `isLazy` to true to avoid checking if the CVar exists at creation
        public CVar(string cvarName, bool isLazy = false)
        {
            name = cvarName;
            type = typeof(T);

            if(!CVarRegistry.ContainsCVar(name) && !isLazy)
            {
                throw new CVarRegistryException("Requested CVar does not exist.");
            }
        }

        public static implicit operator T(CVar<T> cvar)
        {
            return cvar.value;
        }
        public static implicit operator string(CVar<T> cvar)
        {
            return cvar.stringValue;
        }

        // Writes to the CVar
        public void Write(T value)
        {
            CVarRegistry.WriteTo(name, value);
        }
    }
}