/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2016
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

/// <summary>
/// Here's everything stored that is related to the bot configuration.
/// Nothing is hardcoded here, except the path where the files are stored.
/// </summary>
namespace QIRC.Configuration
{
    /// <summary>
    /// This bundles values from <see cref="Settings.values"/> into a file representation.
    /// </summary>
    public class SettingsFile : IEnumerable<KeyValuePair<String, Object>>
    {
        /// <summary>
        /// The name of the file that gets written to disk.
        /// </summary>
        protected String name { get; set; }

        /// <summary>
        /// The loaded values and a default value for them.
        /// </summary>
        protected Dictionary<String, Object> values { get; set; }

        /// <summary>
        /// Creates the File representation
        /// </summary>
        /// <param name="name">The name of the file in the settings directory without file extension</param>
        /// <param name="defaults">The loaded keys with their default values</param>
        public SettingsFile(String name, params Object[] defaults)
        {
            this.name = name;
            values = new Dictionary<String, Object>();
            for (Int32 i = 0; i < defaults.Length; i += 2)
            {
                String key = defaults[i] as String;
                Object value = defaults[i + 1];
                values.Add(key, value);
            }
        }

        /// <summary>
        /// Add a new key with it's default value to <see cref="values"/>
        /// </summary>
        /// <typeparam name="T">The type of the given object.</typeparam>
        /// <param name="name">The name of the key loaded from the settings file.</param>
        /// <param name="value">The default value that gets written when a new file is created.</param>
        public void Add<T>(String name, T value)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (values.ContainsKey(name))
                throw new ArgumentException(String.Format("A key named \"{0}\" does already exist!", name), nameof(name));
            values.Add(name, value);
        }

        /// <summary>
        /// Reads a default value from <see cref="values"/>
        /// </summary>
        /// <typeparam name="T">The type of the returned object.</typeparam>
        /// <param name="name">An identifier to access the object.</param>
        /// <returns>The value of <paramref name="name"/></returns>
        public T Get<T>(String name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (!values.ContainsKey(name))
                throw new ArgumentException(String.Format("\"{0}\" is not a valid key!", name), nameof(name));
            return (T)Convert.ChangeType(values[name], typeof(T));
        }

        /// <summary>
        /// Transforms the file settings into a serializeable type.
        /// </summary>
        public Object ToObject()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder builder = moduleBuilder.DefineType(name, 
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout, 
                null
            );
            ConstructorBuilder constructor = builder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            foreach (KeyValuePair<String, Object> pair in values)
            {
                FieldBuilder field = builder.DefineField(pair.Key, pair.Value.GetType(), FieldAttributes.Public);
            }
            Type type = builder.CreateType();
            Object obj = Activator.CreateInstance(type);
            foreach (FieldInfo info in type.GetFields())
                info.SetValue(obj, values[info.Name]);
            return obj;
        }

        /// <summary>
        /// Transforms the file settings into a serializeable type.
        /// Here we use external values.
        /// </summary>
        public Object ToObject(Dictionary<String, Object> values)
        {
            Dictionary<String, Object> backup = this.values;
            this.values = values;
            Object value = ToObject();
            this.values = backup;
            return value;
        }

        /// <summary>
        /// An enumerator for iterating over the default values.
        /// </summary>
        /// <returns>An enumerator for iterating over the default values.</returns>
        public IEnumerator<KeyValuePair<String, Object>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// An enumerator for iterating over the default values.
        /// </summary>
        /// <returns>An enumerator for iterating over the default values.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// Returns the name of the file
        /// </summary>
        /// <returns>The name of the file on disk</returns>
        public override String ToString()
        {
            return name;
        }
    }
}
