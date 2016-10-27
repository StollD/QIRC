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
using System.Threading;
using QIRC.Configuration;

namespace QIRC.Serialization
{
    /// <summary>
    /// A <see cref="List{T}"/> wrapper that is synced with a file on disk
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SerializeableList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
        protected Object lockObj = new Object();

        protected List<T> _data;

        protected List<T> data
        {
            get
            {
                lock (lockObj)
                    return _data;
            }
            set
            {
                lock (lockObj)
                    _data = value;
            }
        }

        protected String name { get; set; }

        public Int32 Capacity
        {
            get
            {
                lock (lockObj) return data.Capacity;
            }
            set
            {
                lock (lockObj) data.Capacity = value;
            }
        }

        public Int32 Count
        {
            get
            {
                lock (lockObj) return data.Count;
            }
        }

        Boolean IList.IsFixedSize
        {
            get { return false; }
        }

        Boolean ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        Boolean IList.IsReadOnly
        {
            get { return false; }
        }

        Boolean ICollection.IsSynchronized
        {
            get { return false; }
        }

        Object ICollection.SyncRoot
        {
            get
            {
                lock (lockObj) return (data as ICollection).SyncRoot;
            }
        }

        public T this[Int32 index]
        {
            get
            {
                lock (lockObj) return data[index];
            }
            set
            {
                lock (lockObj) data[index] = value;
            }
        }

        Object IList.this[Int32 index]
        {
            get
            {
                lock (lockObj) return data[index];
            }
            set
            {
                lock (lockObj) data[index] = (T) value;
            }
        }

        public SerializeableList(String name)
        {
            Directory.CreateDirectory(Paths.data);
            this.name = name;
            if (Paths.data.Exists(name + ".dat"))
            {
                DeflateStream stream = new DeflateStream(File.Open(Paths.data + name + ".dat", FileMode.Open), CompressionMode.Decompress, false);
                BsonReader bson = new BsonReader(stream, true, DateTimeKind.Utc);
                data = new JsonSerializer().Deserialize<List<T>>(bson);
                bson.Close();
            }
            else
            {
                data = new List<T>();
            }
            Thread save = new Thread(Save);
            save.IsBackground = true;
            save.Start();
        }

        public SerializeableList(String name, Int32 capacity)
        {
            Directory.CreateDirectory(Paths.data);
            this.name = name;
            if (Paths.data.Exists(name + ".dat"))
            {
                DeflateStream stream = new DeflateStream(File.Open(Paths.data + name + ".dat", FileMode.Open), CompressionMode.Decompress, false);
                BsonReader bson = new BsonReader(stream, true, DateTimeKind.Utc);
                data = new JsonSerializer().Deserialize<List<T>>(bson);
                data.Capacity = capacity;
                bson.Close();
            }
            else
            {
                data = new List<T>(capacity);
            }
            Thread save = new Thread(Save);
            save.IsBackground = true;
            save.Start();
        }

        public SerializeableList(String name, IEnumerable<T> collection)
        {
            Directory.CreateDirectory(Paths.data);
            this.name = name;
            if (Paths.data.Exists(name + ".dat"))
            {
                DeflateStream stream = new DeflateStream(File.Open(Paths.data + name + ".dat", FileMode.Open), CompressionMode.Decompress, false);
                BsonReader bson = new BsonReader(stream, true, DateTimeKind.Utc);
                data = new JsonSerializer().Deserialize<List<T>>(bson);
                data.AddRange(collection);
                bson.Close();
            }
            else
            {
                data = new List<T>(collection);
            }
            Thread save = new Thread(Save);
            save.IsBackground = true;
            save.Start();
        }

        public void Add(T item)
        {
            lock (lockObj)
                data.Add(item);
        }

        Int32 IList.Add(Object item)
        {
            lock (lockObj)
                return (data as IList).Add(item);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            lock (lockObj)
                data.AddRange(collection);
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            lock (lockObj)
                return data.AsReadOnly();
        }

        public Int32 BinarySearch(Int32 index, Int32 count, T item, IComparer<T> comparer)
        {
            lock (lockObj)
                return data.BinarySearch(index, count, item, comparer);
        }

        public Int32 BinarySearch(T item)
        {
            lock (lockObj)
                return data.BinarySearch(0, data.Count, item, null);
        }

        public Int32 BinarySearch(T item, IComparer<T> comparer)
        {
            lock (lockObj)
                return data.BinarySearch(0, data.Count, item, comparer);
        }

        public void Clear()
        {
            lock (lockObj)
                data.Clear();
        }

        public Boolean Contains(T item)
        {
            lock (lockObj)
                return data.Contains(item);
        }

        Boolean IList.Contains(Object item)
        {
            lock (lockObj)
                return (data as IList).Contains(item);
        }

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            lock (lockObj)
                return data.ConvertAll(converter);
        }

        public void CopyTo(T[] array)
        {
            lock (lockObj)
                data.CopyTo(array, 0);
        }

        void ICollection.CopyTo(Array array, Int32 arrayIndex)
        {
            lock (lockObj)
                (data as ICollection).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Int32 index, T[] array, Int32 arrayIndex, Int32 count)
        {
            lock (lockObj)
                data.CopyTo(index, array, arrayIndex, count);
        }

        public void CopyTo(T[] array, Int32 arrayIndex)
        {
            lock (lockObj)
                data.CopyTo(array, arrayIndex);
        }

        public Boolean Exists(Predicate<T> match)
        {
            lock (lockObj)
                return data.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            lock (lockObj)
                return data.Find(match);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            lock (lockObj)
                return data.FindAll(match);
        }

        public Int32 FindIndex(Predicate<T> match)
        {
            lock (lockObj)
                return data.FindIndex(match);
        }

        public Int32 FindIndex(Int32 startIndex, Predicate<T> match)
        {
            lock (lockObj)
                return data.FindIndex(startIndex, match);
        }

        public Int32 FindIndex(Int32 startIndex, Int32 count, Predicate<T> match)
        {
            lock (lockObj)
                return data.FindIndex(startIndex, count, match);
        }

        public T FindLast(Predicate<T> match)
        {
            lock (lockObj)
                return data.FindLast(match);
        }

        public Int32 FindLastIndex(Predicate<T> match)
        {
            lock (lockObj)
                return data.FindLastIndex(match);
        }

        public Int32 FindLastIndex(Int32 startIndex, Predicate<T> match)
        {
            lock (lockObj)
                return data.FindLastIndex(startIndex, match);
        }

        public Int32 FindLastIndex(Int32 startIndex, Int32 count, Predicate<T> match)
        {
            lock (lockObj)
                return data.FindLastIndex(startIndex, count, match);
        }

        public void ForEach(Action<T> action)
        {
            lock (lockObj)
                data.ForEach(action);
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (lockObj)
                return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (lockObj)
                return data.GetEnumerator();
        }

        public List<T> GetRange(Int32 index, Int32 count)
        {
            lock (lockObj)
                return data.GetRange(index, count);
        }

        public Int32 IndexOf(T item)
        {
            lock (lockObj)
                return data.IndexOf(item);
        }

        Int32 IList.IndexOf(Object item)
        {
            lock (lockObj)
                return (data as IList).IndexOf(item);
        }

        public Int32 IndexOf(T item, Int32 index)
        {
            lock (lockObj)
                return data.IndexOf(item, index);
        }

        public Int32 IndexOf(T item, Int32 index, Int32 count)
        {
            lock (lockObj)
                return data.IndexOf(item, index, count);
        }

        public void Insert(Int32 index, T item)
        {
            lock (lockObj)
                data.Insert(index, item);
        }

        void IList.Insert(Int32 index, Object item)
        {
            lock (lockObj)
                (data as IList).Insert(index, item);
        }

        public void InsertRange(Int32 index, IEnumerable<T> collection)
        {
            lock (lockObj)
                data.InsertRange(index, collection);
        }

        public Int32 LastIndexOf(T item)
        {
            lock (lockObj)
                return data.LastIndexOf(item);
        }

        public Int32 LastIndexOf(T item, Int32 index)
        {
            lock (lockObj)
                return data.LastIndexOf(item, index);
        }

        public Int32 LastIndexOf(T item, Int32 index, Int32 count)
        {
            lock (lockObj)
                return data.LastIndexOf(item, index, count);
        }

        public Boolean Remove(T item)
        {
            lock (lockObj)
                return data.Remove(item);
        }

        void IList.Remove(Object item)
        {
            lock (lockObj)
                (data as IList).Remove(item);
        }

        public Int32 RemoveAll(Predicate<T> match)
        {
            lock (lockObj)
                return data.RemoveAll(match);
        }

        public void RemoveAt(Int32 index)
        {
            lock (lockObj)
                data.RemoveAt(index);
        }

        public void RemoveRange(Int32 index, Int32 count)
        {
            lock (lockObj)
                data.RemoveRange(index, count);
        }

        public void Reverse()
        {
            lock (lockObj)
                data.Reverse();
        }

        public void Reverse(Int32 index, Int32 count)
        {
            lock (lockObj)
                data.Reverse(index, count);
        }

        public void Sort()
        {
            lock (lockObj)
                data.Sort();
        }

        public void Sort(IComparer<T> comparer)
        {
            lock (lockObj)
                data.Sort(comparer);
        }

        public void Sort(Int32 index, Int32 count, IComparer<T> comparer)
        {
            lock (lockObj)
                data.Sort(index, count, comparer);
        }

        public void Sort(Comparison<T> comparison)
        {
            lock (lockObj)
                data.Sort(comparison);
        }

        public T[] ToArray()
        {
            lock (lockObj)
                return data.ToArray();
        }

        public void TrimExcess()
        {
            lock (lockObj)
                data.TrimExcess();
        }

        public Boolean TrueForAll(Predicate<T> match)
        {
            lock (lockObj)
                return data.TrueForAll(match);
        }

        private void Save()
        {
            while (true)
            {
                Thread.Sleep(10 * 60 * 1000);
                lock (lockObj)
                {
                    Directory.CreateDirectory(Paths.data + "shadow/");
                    using (DeflateStream stream = new DeflateStream(File.Open(Paths.data + "shadow/" + name + ".dat", FileMode.Create), CompressionMode.Compress, false))
                    {
                        using (BsonWriter bson = new BsonWriter(stream))
                            new JsonSerializer().Serialize(bson, data, typeof(List<T>));
                    }
                    File.Copy(Paths.data + "shadow/" + name + ".dat", Paths.data + name + ".dat", true);
                }
            }
        }
    }
}

