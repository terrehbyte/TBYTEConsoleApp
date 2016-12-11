using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TBYTEConsole.Utilities;

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

    public abstract class CVarRegistry
    {
        public CVarRegistry()
        {
            // TODO: enforce support for tagged [CVarProperty] classes?
        }

        // Adds an dataentry to the CVarRegistry by name and sets an initial value
        public abstract CVar<T> Register<T>(string cvarName, T initialValue) where T : IConvertible;

        // Adds an dataentry to the CVarRegistry by name
        public abstract CVar<T> Register<T>(string cvarName) where T : IConvertible;

        // Adds an dataentry to the CVarRegistry by existing CVar
        public abstract CVar<T> Register<T>(CVar<T> newCvar) where T : IConvertible;

        // Adds a property-entry to the CVarRegistry by name and delegates
        public abstract CVar<T> Register<T>(string cvarName, Func<string> getter, Action<string> setter) where T : IConvertible;

        // Adds a property-entry to the CVarRegistry by name and delegates
        public abstract CVar<T> Register<T>(Type type, string cvarName, Func<string> getter, Action<string> setter) where T : IConvertible;

        // Returns an object for a given key, as the type given
        // - Asserts if the given key does not have a value
        public abstract T LookUp<T>(string cvarName) where T : IConvertible;

        // Returns the string-backed data store for a given CVar
        public abstract string LookUp(string cvarName);

        // Assigns a value to the specified CVar entry
        // - Raises an exception if the value could not be converted into a string
        public abstract void WriteTo<T>(string cvarName, T value) where T : IConvertible;

        // Assigns a string value to the specified CVar entry
        // - Raises an exception if the string could not be converted into CVar type
        public abstract void WriteTo(string cvarName, string value);

        // Returns true if the CVar is registered with this registry
        public abstract bool ContainsCVar(string cvarName);

        // Returns an array containing the name of each registered CVar
        public abstract string[] GetCVarNames();
    }

    public class StandardCVarRegistry : CVarRegistry
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

        public StandardCVarRegistry()
        {
            Register("version", "0.1");
            Register("cl_playerName", "PlayerName");
            Register("sensitivity", 3);

            GatherProperties();
        }

        static private Func<string> GetGetter(MethodInfo method)
        {
            method.ThrowIfNull("method");

            if(method.ReturnType != typeof(string) ||
               method.GetParameters().Length != 0  ||
               !method.IsStatic)
            {
                throw new ArgumentException("Method is unsuitable for use as CVar getter");
            }
            
            var test = Expression.Lambda<Func<string>>(Expression.Call(method)).Compile();
            return test;
        }

        static private Action<string> GetSetter(MethodInfo method)
        { 
            method.ThrowIfNull("method");

            if (method.ReturnType != typeof(void) ||
               method.GetParameters().Length != 1 ||
               !method.IsStatic)
            {
                throw new ArgumentException("Method is unsuitable for use as CVar setter");
            }


            var input = Expression.Parameter(typeof(string), "input");
            var test = Expression.Lambda<Action<string>>(
                Expression.Call(method, input),
                input)
                .Compile();
            return test;
        }

        private ICVarReadWritable[] GatherProperties()
        {
            List<ICVarReadWritable> gatheredCVars = new List<ICVarReadWritable>();

            Assembly assembly = Assembly.GetExecutingAssembly();

            var typesWithMyAttribute =
                from t in assembly.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(CVarPropertyAttribute), true)
                where attributes != null && attributes.Length > 0
                select new { Type = t, Attributes = attributes.Cast<CVarPropertyAttribute>() };

            // create CVarProperty for each properly defined "type"
            foreach(var res in typesWithMyAttribute)
            {
                var type = res.Type;
                var attribData = (CVarPropertyAttribute)Attribute.GetCustomAttribute(type, typeof(CVarPropertyAttribute));

                var getterMethods =
                    from m in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    let methods = m.GetCustomAttributes(typeof(CVarPropertyGetterAttribute), false)
                    where methods != null && methods.Length > 0
                    select m;

                int count = getterMethods.Count(); 

                if (count != 1) continue;

                var getterMethod = GetGetter(getterMethods.First());

                var setterMethods =
                    from m in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    let methods = m.GetCustomAttributes(typeof(CVarPropertySetterAttribute), false)
                    where methods != null && methods.Length > 0
                    select m;

                count = setterMethods.Count();

                if (count != 1) continue;

                var setterMethod = GetSetter(setterMethods.First());

                Type specType = typeof(CVarProperty<>).MakeGenericType(type);
                var finalCVar = (ICVarReadWritable)Activator.CreateInstance(specType, attribData.type, getterMethod, setterMethod);

                registry[attribData.token] = finalCVar;
            }

            return null;

            // Modified from...
            // http://stackoverflow.com/questions/607178/how-enumerate-all-classes-with-custom-class-attribute

            // Modified from...
            // http://stackoverflow.com/questions/2933221/can-you-get-a-funct-or-similar-from-a-methodinfo-object    
        }

        private Dictionary<string, ICVarReadWritable> registry = new Dictionary<string, ICVarReadWritable>();

        public override CVar<T> Register<T>(string cvarName, T initialValue)
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

        public override CVar<T> Register<T>(string cvarName)
        {
            return Register(cvarName, default(T));
        }

        public override CVar<T> Register<T>(CVar<T> newCvar)
        {
            return Register<T>(newCvar.name);
        }

        public override CVar<T> Register<T>(string cvarName, Func<string> getter, Action<string> setter)
        {
            if (ContainsCVar(cvarName)) { return null; }

            CVarProperty<T> babyCVar = new CVarProperty<T>(getter, setter);

            registry[cvarName] = babyCVar;

            return new CVar<T>(cvarName, true);
        }

        public override CVar<T> Register<T>(Type type, string cvarName, Func<string> getter, Action<string> setter)
        {
            if (ContainsCVar(cvarName)) { return null; }

            CVarProperty<T> babyCVar = new CVarProperty<T>(getter, setter);

            registry[cvarName] = babyCVar;

            return new CVar<T>(cvarName, true);
        }

        public override T LookUp<T>(string cvarName)
        {
            return (T)Convert.ChangeType(registry[cvarName].stringValue, typeof(T));
        }

        public override string LookUp(string cvarName)
        {
            return registry[cvarName].stringValue;
        }

        public override void WriteTo<T>(string cvarName, T value)
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

        public override void WriteTo(string cvarName, string value)
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

        public override bool ContainsCVar(string cvarName)
        {
            return registry.ContainsKey(cvarName);
        }

        public override string[] GetCVarNames()
        {
            List<string> keyNames = new List<string>();

            foreach(var key in registry.Keys)
            {
                keyNames.Add(key);
            }

            return keyNames.ToArray();
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
                return ConsoleSvc.cvarRegistry.LookUp<T>(name);
            }
        }

        // The string-backed value of the CVar.
        public string stringValue
        {
            get
            {
                return ConsoleSvc.cvarRegistry.LookUp(name);
            }
        }

        // Creates an accessor to a registered CVar
        // - Set `isLazy` to true to avoid checking if the CVar exists at creation
        public CVar(string cvarName, bool isLazy = false)
        {
            name = cvarName;
            type = typeof(T);

            if(!isLazy && !ConsoleSvc.cvarRegistry.ContainsCVar(name))
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
            ConsoleSvc.cvarRegistry.WriteTo(name, value);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CVarPropertyAttribute : Attribute
    {
        public readonly string token;
        public readonly Type type;

        public CVarPropertyAttribute(string token, Type type)
        {
            // runtime check :(
            if (type is IConvertible) { throw new ArgumentException("CVar must be of type IConvertible"); }

            this.token = token;
            this.type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CVarPropertyGetterAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CVarPropertySetterAttribute : Attribute
    {

    }
}