﻿using UnityEngine;
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
        private interface ICVarReadWritable
        {
            Type type { get; }
            string stringValue { get; set; }
        }

        private class CVarProperty<T> : ICVarReadWritable
        {
            Func<string> getter;
            Action<string> setter;

            public CVarProperty(Type propType, Func<string> getter, Action<string> setter)
            {
                type = propType;

                this.getter = getter;
                this.setter = setter;
            }
            public CVarProperty(Func<string> getter, Action<string> setter) : this(typeof(T), getter, setter)
            {
            }

            public Type type { get; private set; }
            public string stringValue
            {
                get
                {
                    return getter();
                }
                set
                {
                    setter(value);
                }
            }
        }
        private class CVarData<T> : ICVarReadWritable
        {
            public CVarData(Type dataType, string initialValue)
            {
                type = dataType;
                stringValue = initialValue;
            }
            public CVarData(string initialValue) : this(typeof(T), initialValue)
            {
            }
            
            public Type type { get; private set; }
            public string stringValue { get; set; }
        }

        static CVarRegistry()
        {
            CVarDefaults.Register();
        }

        static Dictionary<string, ICVarReadWritable> registry = new Dictionary<string, ICVarReadWritable>();

        // Adds an dataentry to the CVarRegistry by name and sets an initial value
        static public CVar<T> Register<T>(string cvarName, T initialValue) where T : IConvertible
        {
            if (ContainsCVar(cvarName)) { return null; }

            CVarData<T> babyCVar;
            try
            {
                babyCVar = new CVarData<T>(initialValue.ToString());
            }
            catch (NullReferenceException ex)
            {
                // perform special logic for strings
                //   - `default(String)` yields `null` since it's a ref type
                if (typeof(T) == typeof(String))
                {
                    babyCVar = new CVarData<T>(string.Empty);
                }
                else
                {
                    throw new CVarRegistryException("Failed to create internal data store for CVar.", ex);
                }
            }

            registry[cvarName] = babyCVar;

            return new CVar<T>(cvarName, true);
        }

        // Adds an dataentry to the CVarRegistry by name
        static public CVar<T> Register<T>(string cvarName) where T : IConvertible
        {
            return Register(cvarName, default(T));
        }

        // Adds an dataentry to the CVarRegistry by existing CVar
        static public CVar<T> Register<T>(CVar<T> newCvar) where T : IConvertible
        {
            return Register<T>(newCvar.name);
        }

        // Adds a property-entry to the CVarRegistry by name and delegates
        static public CVar<T> Register<T>(string cvarName, Func<string> getter, Action<string> setter) where T : IConvertible
        {
            if (ContainsCVar(cvarName)) { return null; }

            CVarProperty<T> babyCVar = new CVarProperty<T>(getter, setter);

            registry[cvarName] = babyCVar;

            return new CVar<T>(cvarName, true);
        }

        // Returns an object for a given key, as the type given
        // - Asserts if the given key does not have a value
        static public T LookUp<T>(string cvarName) where T : IConvertible
        {
            return (T)Convert.ChangeType(registry[cvarName].stringValue, typeof(T));
        }

        // Returns the string-backed data store for a given CVar
        static public string LookUp(string cvarName)
        {
            return registry[cvarName].stringValue;
        }

        // Assigns a value to the specified CVar entry
        // - Raises an exception if the value could not be converted into a string
        static public void WriteTo<T>(string cvarName, T value) where T : IConvertible
        {
            try
            {
                registry[cvarName].stringValue = Convert.ToString(value);
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

            registry[cvarName].stringValue = value;
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
                 
                CVarRegistry.Register<float>("sv_timescale", GetTimescale, SetTimescale);
            }

            public static void SetTimescale(string value)
            {
                Time.timeScale = Convert.ToSingle(value);
            }

            public static string GetTimescale()
            {
                return Time.timeScale.ToString();
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