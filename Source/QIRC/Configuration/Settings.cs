/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2016
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// Newtonsoft
using Newtonsoft.Json;

/// QIRC
using QIRC.Constants;
using QIRC.IRC;

/// System
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using PathIO = System.IO.Path;

/// <summary>
/// Here's everything stored that is related to the bot configuration.
/// Nothing is hardcoded here, except the path where the files are stored.
/// </summary>
namespace QIRC.Configuration
{
    /// <summary>
    /// Here are the settings for the bot stored. They are loaded dynamically,
    /// so that plugins can interface with them and inject their own settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The values that are loaded from the settings files.
        /// You can only access them through <see cref="Read{T}(String)"/> and <see cref="Write{T}(String, T)"/>
        /// </summary>
        protected static Dictionary<String, Object> values { get; set; }

        /// <summary>
        /// The assosications between value keys and files on disk.
        /// Stores also the default values for a key.
        /// </summary>
        protected static HashSet<SettingsFile> files { get; set; }

        /// <summary>
        /// A boolean that indicates if the initial load is completed
        /// </summary>
        private static Boolean isLoaded { get; set; }

        /// <summary>
        /// Settings for the JSON Serializer
        /// </summary>
        public static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            FloatParseHandling = FloatParseHandling.Double,
            NullValueHandling = NullValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Auto
        };

        /// <summary>
        /// Reads a value from <see cref="values"/>
        /// </summary>
        /// <typeparam name="T">The type of the returned object.</typeparam>
        /// <param name="name">An identifier to access the internal object.</param>
        /// <returns>The value of <paramref name="name"/></returns>
        public static T Read<T>(String name) 
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (!values.ContainsKey(name))
                throw new ArgumentException(String.Format("\"{0}\" is not a valid key!", name), nameof(name));
            try
            {
                return (T)Convert.ChangeType(values[name], typeof(T));
            }
            catch
            {
                try
                {
                    return (T)values[name];
                }
                catch
                {
                    return default(T);
                }
            }
        }

        /// <summary>
        /// Overwrites a value from <see cref="values"/> with the given object.
        /// </summary>
        /// <typeparam name="T">The type of the given object.</typeparam>
        /// <param name="name">An identifier to access the internal object.</param>
        /// <param name="value">The new object that should be stored as <paramref name="name"/></param>
        public static void Write<T>(String name, T value)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (!values.ContainsKey(name))
                values.Add(name, value);
            else
                values[name] = value;
            Save();
        }

        /// <summary>
        /// Loads the setting files from disk.
        /// This could get a bit performance intensive...
        /// </summary>
        public static void Load()
        {
            foreach (SettingsFile file in files)
            {
                String name = file.ToString();
                Object type = file.ToObject();
                if (Paths.settings.Exists(name + ".json"))
                {
                    String json = File.ReadAllText(PathIO.Combine(Paths.settings, name + ".json"));
                    type = JsonConvert.DeserializeObject(json, type.GetType(), settings);
                }
                else
                {
                    String json = JsonConvert.SerializeObject(type, settings);
                    File.WriteAllText(PathIO.Combine(Paths.settings, name + ".json"), json);
                }
                foreach (FieldInfo field in type.GetType().GetFields())
                    Write(field.Name, field.GetValue(type));
            }
            isLoaded = true;
        }

        /// <summary>
        /// Write the setting files to disk.
        /// This could get a bit performance intensive...
        /// </summary>
        public static void Save()
        {
            if (!isLoaded)
                return;
            foreach (SettingsFile file in files)
            {
                Dictionary<String, Object> fileValues = new Dictionary<String, Object>();
                foreach (KeyValuePair<String, Object> pair in file)
                    fileValues.Add(pair.Key, Read<Object>(pair.Key));
                String name = file.ToString();
                Object type = file.ToObject(fileValues);
                String json = JsonConvert.SerializeObject(type, settings);
                File.WriteAllText(PathIO.Combine(Paths.settings, name + ".json"), json);
            }
        }

        /// <summary>
        /// Adds a <see cref="SettingsFile"/> to <see cref="files"/>
        /// </summary>
        public static void AddFile(SettingsFile file)
        {
            files.Add(file);
        }

        /// <summary>
        /// Removes a <see cref="SettingsFile"/> from <see cref="files"/>
        /// </summary>
        public static void RemoveFile(SettingsFile file)
        {
            files.Remove(file);
        }

        /// <summary>
        /// Removes a <see cref="SettingsFile"/> from <see cref="files"/>
        /// </summary>
        public static void RemoveFile(String file)
        {
            files.RemoveWhere(f => f.ToString() == file);
        }

        /// <summary>
        /// Grabs a <see cref="SettingsFile"/> based on it's name
        /// </summary>
        public static void GetFile(String name, ref SettingsFile file)
        {
            if (files.Count(f => f.ToString() == name) == 0)
                throw new IndexOutOfRangeException();
            file = files.FirstOrDefault(f => f.ToString() == name);
        }

        /// <summary>
        /// Static Constructor for initialization.
        /// </summary>
        static Settings()
        {
            Directory.CreateDirectory(Paths.settings);
            values = new Dictionary<String, Object>();
            files = new HashSet<SettingsFile>()
            {
                new SettingsFile("settings",
                    "name", "QIRC-Bot",
                    "control", "!",
                    "saveInterval", 10
                ),
                new SettingsFile("connection",
                    "host", "",
                    "port", 6667,
                    "useSSL", false,
                    "password", "",
                    "channels", new List<ProtoIrcChannel>(),
                    "admins", new List<ProtoIrcAdmin>()
                )
            };
        }
    }
}
