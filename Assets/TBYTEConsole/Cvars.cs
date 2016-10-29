using UnityEngine;
using System.Collections.Generic;

namespace TBYTEConsole
{
    public enum CVarType
    {
        INT,
        FLOAT,
        DOUBLE,
        STRING,
        VECTOR2,
        VECTOR3,

        CUSTOM
    }

    public static class CVarRegistry
    {
        static Dictionary<string, object> registry = new Dictionary<string, object>();

        // Adds an entry to the CVarRegistry
        static public void Register(string cvarName, object cvarData)
        {
            Debug.Assert(!registry.ContainsKey(cvarName),
                string.Format("CVar Registry already contains an entry for {0}", cvarName));

            registry[cvarName] = cvarData;
        }

        // Returns an object for a given key
        // - Asserts if the given key does not have a value
        static public object LookUp(string cvarName)
        {
            Debug.Assert(registry.ContainsKey(cvarName),
                string.Format("CVar Registry does not contain an entry for {0}", cvarName));

            return registry[cvarName];
        }

        // Returns an object for a given key
        // - Asserts if the given key does not have a value
        static public object LookUp(CVar cvar)
        {
            throw new System.NotImplementedException();
            return null;
        }

        // Returns an object for a given key, as the type given
        // - Asserts if the given key does not have a value
        static public T LookUp<T>(string cvarName) where T : struct
        {
            return (T)LookUp(cvarName);
        }
    }

    // Type-safe accessor for a particular CVar
    public class CVar
    {
        private readonly string dataName;
        private readonly CVarType dataType;

        public CVar(string name, int defaultValue)
        {
            dataName = name;
            dataType = CVarType.INT;

            CVarRegistry.Register(name, defaultValue);
        }
        public CVar(string name, float defaultValue)
        {
            dataName = name;
            dataType = CVarType.FLOAT;

            CVarRegistry.Register(name, defaultValue);
        }
        public CVar(string name, double defaultValue)
        {
            dataName = name;
            dataType = CVarType.DOUBLE;

            CVarRegistry.Register(name, defaultValue);
        }
        public CVar(string name, string defaultValue)
        {
            dataName = name;
            dataType = CVarType.STRING;

            CVarRegistry.Register(name, defaultValue);
        }

        public CVar(string name, object defaultValue)
        {
            dataName = name;
            dataType = CVarType.CUSTOM;

            CVarRegistry.Register(name, defaultValue);
        }

        public int GetIntValue()
        {
            Debug.Assert(dataType == CVarType.INT, "Mismatch detected between value fetched and value stored!");

            return CVarRegistry.LookUp<int>(dataName);
        }
        public float GetFloatValue()
        {
            Debug.Assert(dataType == CVarType.FLOAT, "Mismatch detected between value fetched and value stored!");

            return CVarRegistry.LookUp<float>(dataName);
        }
        public double GetDoubleValue()
        {
            Debug.Assert(dataType == CVarType.DOUBLE, "Mismatch detected between value fetched and value stored!");

            return CVarRegistry.LookUp<double>(dataName);
        }
        public string GetStringValue()
        {
            Debug.Assert(dataType == CVarType.STRING, "Mismatch detected between value fetched and value stored!");

            return CVarRegistry.LookUp(dataName) as string;
        }
        public object GetValue()
        {
            return CVarRegistry.LookUp(dataName);
        }
    }
}