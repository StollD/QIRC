/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */
 
using QIRC.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using QIRC.Configuration;

namespace QIRC.Serialization
{
    /// <summary>
    /// A <see cref="List{T}"/> wrapper that is synced with a file on disk
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SerializeableList<T> : List<T>
    {
        /// <summary>
        /// The filename oft the list
        /// </summary>
        protected String name { get; set; }

        public SerializeableList(String name) : base()
        {
            Directory.CreateDirectory(Paths.data);
            this.name = name;
            if (Paths.data.Exists(name + ".dat"))
            {
                DeflateStream stream = new DeflateStream(File.Open(Paths.data + name + ".dat", FileMode.Open), CompressionMode.Decompress, false);
                BsonReader bson = new BsonReader(stream, true, DateTimeKind.Utc);
                List<T> data = new JsonSerializer().Deserialize<List<T>>(bson);
                bson.Close();
                AddRange(data);
            }
            Task.Run(() => Save());
        }

        public SerializeableList(String name, Int32 capacity) : base(capacity)
        {
            Directory.CreateDirectory(Paths.data);
            this.name = name;
            if (Paths.data.Exists(name + ".dat"))
            {
                DeflateStream stream = new DeflateStream(File.Open(Paths.data + name + ".dat", FileMode.Open), CompressionMode.Decompress, false);
                BsonReader bson = new BsonReader(stream, true, DateTimeKind.Utc);
                List<T> data = new JsonSerializer().Deserialize<List<T>>(bson);
                bson.Close();
                AddRange(data);
            }
            Task.Run(() => Save());
        }

        public SerializeableList(String name, IEnumerable<T> collection) : base(collection)
        {
            Directory.CreateDirectory(Paths.data);
            this.name = name;
            if (Paths.data.Exists(name + ".dat"))
            {
                DeflateStream stream = new DeflateStream(File.Open(Paths.data + name + ".dat", FileMode.Open), CompressionMode.Decompress, false);
                BsonReader bson = new BsonReader(stream, true, DateTimeKind.Utc);
                List<T> data = new JsonSerializer().Deserialize<List<T>>(bson);
                bson.Close();
                AddRange(data);
            }
            Task.Run(() => Save());
        }

        /// <summary>
        /// Saves the data to the file
        /// </summary>
        private void Save()
        {
            Directory.CreateDirectory(Paths.data + "shadow/");
            while (true)
            {
                using (DeflateStream stream = new DeflateStream(File.Open(Paths.data + "shadow/" + name + ".dat", FileMode.Create), CompressionMode.Compress, false))
                {
                    using (BsonWriter bson = new BsonWriter(stream))
                        new JsonSerializer().Serialize(bson, this, typeof (List<T>));
                }
                File.Copy(Paths.data + "shadow/" + name + ".dat", Paths.data + name + ".dat", true);
                Thread.Sleep(600000);
            }
        }
    }
}

