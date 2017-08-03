/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using System;
using SQLite;

namespace QIRC.Serialization
{
    /// <summary>
    /// Interface for database storage of types
    /// </summary>
    public class Storage<T> where T : Storage<T>, new()
    {
        private static TableQuery<T> _query;
        public static TableQuery<T> Query
        {
            get
            {
                if (_query == null)
                {
                    BotController.Database.CreateTable<T>();
                    _query = BotController.Database.Table<T>();
                }
                return _query;
            }
        } 
    }

    public static class QueryExt
    {
        /// <summary>
        /// Adds a
        /// </summary>
        public static T Insert<T>(this TableQuery<T> query, params Object[] keys)
        {
            T obj = (T)Activator.CreateInstance(typeof(T), keys);
            BotController.Database.Insert(obj, typeof(T));
            return obj;
        }
    }
}