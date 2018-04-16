using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class MethodNotFoundException : CVarRegistryException
    {
        public MethodNotFoundException()
        {
        }
        public MethodNotFoundException(string message)
            : base(message)
        {
        }
        public MethodNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    // TODO: enforce support for tagged [CVarProperty] classes?

    public abstract class CVarRegistry
    {
        // Specifies that both public and non-public members are to be included in the search.
        protected const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic;

        // Adds an entry to the CVarRegistry by name
        public abstract CVar<T> Register<T>(string cvarName);

        // Adds an entry to the CVarRegistry by name and sets an initial value by string
        //public abstract CVar<T> Register<T>(string cvarName, string initialValue);

        // Adds an entry to the CVarRegistry by name and sets an initial value
        public abstract CVar<T> Register<T>(string cvarName, T initialValue);

        // Adds an dataentry to the CVarRegistry by existing CVar
        public abstract CVar<T> Register<T>(CVar<T> newCvar);

        // Adds a property-entry to the CVarRegistry by name and delegates
        public abstract CVar<T> Register<T>(string cvarName, Func<string> getter, Action<string> setter);

        public abstract void RegisterStaticMembers<T>();

        // Returns an object for a given key, as the type given
        // - Asserts if the given key does not have a value
        public abstract T LookUp<T>(string cvarName);

        // Returns the string-backed data store for a given CVar
        public abstract string LookUp(string cvarName);

        // Assigns a value to the specified CVar entry
        // - Raises an exception if the value could not be converted into a string
        public abstract void WriteTo<T>(string cvarName, T value) ;

        // Assigns a string value to the specified CVar entry
        // - Raises an exception if the string could not be converted into CVar type
        public abstract void WriteTo(string cvarName, string value);

        // Returns true if the CVar is registered with this registry
        public abstract bool ContainsCVar(string cvarName);

        // Returns an array containing the name of each registered CVar
        public abstract string[] GetCVarNames();

        // Searches and registers CVars declared by attribute.
        public abstract void GatherCVars();
    }

    public class StandardCVarRegistry : CVarRegistry
    {
        // Common interface for CVar access.
        protected interface ICVar
        {
            Type type { get; }
            string stringValue { get; set; }
        }

        // Delegate-backed CVar.
        //  - where T is the backing type
        protected class DelegateCVar<T> : ICVar
        {
            Func<string> getter;
            Action<string> setter;

            public DelegateCVar(Func<string> getter, Action<string> setter)
            {
                this.getter = getter;
                this.setter = setter;
            }

            public Type type { get { return typeof(T); } }
            public string stringValue
            {
                get { return getter(); }
                set { setter(value); }
            }
        }

        // String-backed CVar.
        //  - where T is the backing type
        protected class StringCVar<T> : ICVar
        {
            public StringCVar(string initialValue)
            {
                stringValue = initialValue;
            }

            public Type type { get { return typeof(T); } }
            public string stringValue { get; set; }
        }

        // Field-backed CVar.
        //  - where T is the backing type
        protected class FieldCVar<T> : ICVar
        {
            public readonly FieldInfo field;
            public object instance;

            public FieldCVar(FieldInfo field, object instance)
            {
                this.field = field;
                this.instance = instance;
            }

            public Type type { get { return typeof(T); } }
            public string stringValue
            {
                get
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(type);
                    return converter.ConvertToString(field.GetValue(instance));
                }
                set
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(type);
                    field.SetValue(instance, converter.ConvertFromString(value));
                }
            }
        }

        public StandardCVarRegistry()
        {
            // TODO: consider two step initialize
            //  1. constructor initializes core commands
            //  2. Initialize() method must collect user-defined methods?
            Register("version", "0.1");
            Register("cl_playerName", "PlayerName");
            Register("sensitivity", 3);

            GatherCVars();
        }

        private static MethodInfo[] GetStaticPublicMethodsWithAttribute(Type type, Type attributeType)
        {
            var foundMethods =
                from m in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                let methods = m.GetCustomAttributes(attributeType, false)
                where methods != null && methods.Length > 0
                select m;

            return foundMethods.ToArray();
        }
        private static PropertyInfo[] GetStaticPublicPropertiesWithAttribute(Type type, Type attributeType)
        {
            var foundProps =
                from m in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                let properties = m.GetCustomAttributes(attributeType, false)
                where properties != null && properties.Length > 0
                select m;

            return foundProps.ToArray();
        }
        private static Func<string> GetGetter(MethodInfo method)
        {
            method.ThrowIfNull("method");

            if (method.ReturnType != typeof(string) ||
               method.GetParameters().Length != 0 ||
               !method.IsStatic)
            {
                throw new ArgumentException("Method is unsuitable for use as CVar getter");
            }

            var test = Expression.Lambda<Func<string>>(Expression.Call(method)).Compile();
            return test;
        }
        private static Action<string> GetSetter(MethodInfo method)
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
        private static void GetCVarPropertyMethods(Type type, out Func<string> getter, out Action<string> setter)
        { 
            var prop = (CVarPropertyDescriptorAttribute)Attribute.GetCustomAttribute(type, typeof(CVarPropertyDescriptorAttribute));
            var accessorProp = GetStaticPublicPropertiesWithAttribute(type, typeof(CVarPropertyAccessorAttribute));

            getter = (accessorProp.Count() > 0)             ? GetGetter(accessorProp.First().GetGetMethod(true)) :
                     !string.IsNullOrEmpty(prop.getterName) ? GetGetter(type.GetMethod(prop.getterName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) :
                                                              GetGetter(GetStaticPublicMethodsWithAttribute(type, typeof(CVarPropertyGetterAttribute)).First());

            setter = (accessorProp.Count() > 0)             ? GetSetter(accessorProp.First().GetSetMethod(true)) :
                     !string.IsNullOrEmpty(prop.setterName) ? GetSetter(type.GetMethod(prop.setterName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) :
                                                              GetSetter(GetStaticPublicMethodsWithAttribute(type, typeof(CVarPropertySetterAttribute)).First());
        }

        protected Dictionary<string, ICVar> registry = new Dictionary<string, ICVar>();

        public override CVar<T> Register<T>(string cvarName)
        {
            return Register(cvarName, default(T));
        }

        public override CVar<T> Register<T>(string cvarName, T initialValue)
        {
            if (ContainsCVar(cvarName)) { return null; }

            StringCVar<T> babyCVar;
            try
            {
                babyCVar = new StringCVar<T>(initialValue.ToString());
            }
            catch (NullReferenceException ex)
            {
                // perform special logic for strings
                //   - `default(String)` yields `null` since it's a ref type
                if (typeof(T) == typeof(String))
                {
                    babyCVar = new StringCVar<T>(string.Empty);
                }
                else
                {
                    throw new CVarRegistryException("Failed to create internal data store for CVar.", ex);
                }
            }

            registry[cvarName] = babyCVar;

            return new CVar<T>(cvarName, true);
        }

        public override CVar<T> Register<T>(CVar<T> newCvar)
        {
            return Register<T>(newCvar.name);
        }

        public override CVar<T> Register<T>(string cvarName, Func<string> getter, Action<string> setter)
        {
            if (ContainsCVar(cvarName)) { return null; }

            DelegateCVar<T> babyCVar = new DelegateCVar<T>(getter, setter);

            registry[cvarName] = babyCVar;

            return new CVar<T>(cvarName, true);
        }

        public override T LookUp<T>(string cvarName)
        {
            var cvar = registry[cvarName];

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromString(cvar.stringValue));
        }

        public override string LookUp(string cvarName)
        {
            return registry[cvarName].stringValue;
        }

        public override void WriteTo<T>(string cvarName, T value)
        {
            var cvar = registry[cvarName];

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if(converter.CanConvertTo(cvar.type))
            {
                registry[cvarName].stringValue = converter.ConvertToString(value);
            }
            else
            {
                throw new CVarRegistryException("Failed to convert value to string for storage.");
            }
        }

        public override void WriteTo(string cvarName, string value)
        {
            var cvar = registry[cvarName];
            TypeConverter converter = TypeDescriptor.GetConverter(cvar.type);

            // perform runtime check to see if string value is valid for this type
            try
            {
                converter.ConvertFromString(value);
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

            foreach(var key in registry.Keys) { keyNames.Add(key); }

            return keyNames.ToArray();
        }

        public override void GatherCVars()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var typesWithMyAttribute =
                from t in assembly.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(CVarPropertyDescriptorAttribute), true)
                where attributes != null && attributes.Length > 0
                select new { Type = t, Attributes = attributes.Cast<CVarPropertyDescriptorAttribute>() };

            // create CVarProperty for each properly defined "type"
            foreach (var res in typesWithMyAttribute)
            {
                var type = res.Type;
                var attribData = (CVarPropertyDescriptorAttribute)Attribute.GetCustomAttribute(type, typeof(CVarPropertyDescriptorAttribute));

                Func<string> getterMethod;
                Action<string> setterMethod;

                try
                {
                    GetCVarPropertyMethods(type, out getterMethod, out setterMethod);
                }
                catch (Exception ex)
                {
                    if (!(ex is ArgumentNullException || ex is MethodNotFoundException)) { throw; }
                    // TODO: add internal logging system for console
                    //ConsoleLocator.console.ProcessConsoleInput("echo Failed to find method for " + type.FullName);
                    continue;
                }
                Type specType = typeof(DelegateCVar<>).MakeGenericType(type); 
                var finalCVar = (ICVar)Activator.CreateInstance(specType, getterMethod, setterMethod);

                registry[attribData.token] = finalCVar;
            }

            // Modified from...
            // http://stackoverflow.com/questions/607178/how-enumerate-all-classes-with-custom-class-attribute

            // Modified from...
            // http://stackoverflow.com/questions/2933221/can-you-get-a-funct-or-similar-from-a-methodinfo-object  
        }

        public override void RegisterStaticMembers<T>()
        {
            var type = typeof(T);

            // iterate over each field and check for the CVarAttribute
            foreach (var fieldInfo in type.GetFields(Any | BindingFlags.Static | BindingFlags.GetField))
            {
                // try to fetch attribute
                var attributes = fieldInfo.GetCustomAttributes(typeof(CVarAttribute), true);

                // skip if not found
                if (attributes.Count() < 1)
                    continue;

                var cvarDescriptor = attributes.First() as CVarAttribute;

                Type specType = typeof(FieldCVar<>).MakeGenericType(fieldInfo.FieldType);
                var finalCVar = (ICVar)Activator.CreateInstance(specType, fieldInfo, null);

                registry.Add(cvarDescriptor.name, finalCVar);
                WriteTo(cvarDescriptor.name, cvarDescriptor.defaultValue);
            }

            // iterate over each property and check for the CVarAttribute
            foreach (var propInfo in type.GetProperties(Any | BindingFlags.Static | BindingFlags.GetProperty))
            {
                // try to fetch attribute
                var attributes = propInfo.GetCustomAttributes(typeof(CVarPropertyAttribute), true);

                // skip if not found
                if (attributes.Count() < 1)
                    continue;

                var cvarDescriptor = attributes.First() as CVarPropertyAttribute;

                Type specType = typeof(DelegateCVar<>).MakeGenericType(propInfo.PropertyType);
                var finalCVar = (ICVar)Activator.CreateInstance(specType, GetGetter(propInfo.GetGetMethod(true)), GetSetter(propInfo.GetSetMethod(true))); 

                registry.Add(cvarDescriptor.name, finalCVar);
            }
        }
    }

    // Type-safe accessor for registered CVar
    // - Bypassing the registration check will allow the creation of accessors for non-existant CVars
    public class CVar<T> 
    {
        // The name of the CVar accessed by this object.
        public readonly string name;

        // The type of the CVar accessed by this object.
        public Type type { get { return typeof(T); } }

        // The value of the CVar as the type given for this accessor.
        public T value
        {
            get { return ConsoleLocator.cvarRegistry.LookUp<T>(name); }
        }

        // The value of the CVar as a string.
        public string stringValue
        {
            get { return ConsoleLocator.cvarRegistry.LookUp(name); }
        }

        // Creates an accessor to a registered CVar
        // - Set `isLazy` to true to avoid checking if the CVar exists at creation
        public CVar(string cvarName, bool isLazy = false)
        {
            name = cvarName;

            if(!isLazy && !ConsoleLocator.cvarRegistry.ContainsCVar(name))
            {
                throw new CVarRegistryException("Requested CVar does not exist.");
            }
        }

        // Implicitly returns the value of the CVar as the type of this accessor.
        public static implicit operator T(CVar<T> cvar) 
        {
            return cvar.value;
        }

        // Provides the string representation of the CVar.
        public override string ToString()
        {
            return stringValue;
        }

        // Writes a value to the CVar.
        //  - Will fail if the value cannot be converted into the CVar's type.
        public virtual void Write(T value)
        {
            ConsoleLocator.cvarRegistry.WriteTo(name, value);
        }

        // Writes a value to the CVar.
        //  - Will fail if the string cannot be converted into the CVar's type.
        public virtual void Write(string value)
        {
            ConsoleLocator.cvarRegistry.WriteTo(name, value);
        }
    }

    // TODO: does inherited affect inherited properties?
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CVarAttribute : Attribute
    {
        public readonly string name;
        public readonly string defaultValue;

        public CVarAttribute(string name, string defaultValue="")
        {
            this.name = name;
            this.defaultValue = defaultValue;
        }
    }

    // TODO: does inherited affect inherited properties?
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CVarPropertyAttribute : Attribute
    {
        public readonly string name;
        public readonly string defaultValue;

        public CVarPropertyAttribute(string name, string defaultValue = "")
        {
            this.name = name;
            this.defaultValue = defaultValue;
        }
    }

    // TODO: does inherited affect inherited properties?
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CVarPropertyDescriptorAttribute : Attribute
    {
        public readonly string token;
        public readonly Type type;

        // For internal use when determining what the getter method is w/o the attribute.
        //  - Not valid when the CVarPropertyGetter attribute is used to identify the setter. 
        public string getterName { get; private set; }

        // For internal use when determining what the setter method is w/o the attribute.
        //  - Not valid when the CVarPropertySetter attribute is used to identify the setter. 
        public string setterName { get; private set; }

        public CVarPropertyDescriptorAttribute(string token, Type type)
        {
            // runtime check :(
            if (type is IConvertible) { throw new ArgumentException("CVar must be of type IConvertible"); }

            this.token = token;
            this.type = type;
        }
        public CVarPropertyDescriptorAttribute(string token, Type type, string getterName, string setterName) : this(token, type)
        {
            this.setterName = setterName;
            this.getterName = getterName;
        }
        public CVarPropertyDescriptorAttribute(string token, Type type, string propertyName) : this(token, type)
        {
            // TODO: Can I resolve the property name from here?
            // TODO: IS THIS DEFINED BY THE SPECIFICATION
            //  - http://stackoverflow.com/questions/23102639/are-c-sharp-properties-actually-methods
            //    THEY'RE DEFINITELY METHODS SO IT'S OKAY, RIGHT?
            //    ... oh, it totally is in section 10.3.9.1 http://i.imgur.com/ZXmBmJP.png
            this.setterName = "set_" + propertyName;
            this.getterName = "get_" + propertyName;
        }
    }

    // Tags a static method as the getter for a CVarProperty.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CVarPropertyGetterAttribute : Attribute { }

    // Tags a static method as the setter for a CVarProperty.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CVarPropertySetterAttribute : Attribute { }

    // Tags a static property as the getter and setter for a CVarProperty.
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CVarPropertyAccessorAttribute : Attribute { }
}