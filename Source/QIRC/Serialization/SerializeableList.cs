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

namespace QIRC.Serialization
{
    /// <summary>
    /// A <see cref="List{T}"/> wrapper that is synced with a file on disk
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SerializeableList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
        protected List<T> data { get; set; }

        protected String name { get; set; }

        public Int32 Capacity
        {
            get { return data.Capacity; }
            set { data.Capacity = value; Save(); }
        }

        public Int32 Count
        {
            get { return data.Count; }
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
            get
            {
                return false;
            }
        }

        Object ICollection.SyncRoot
        {
            get { return (data as ICollection).SyncRoot;}
        }

        public T this[Int32 index]
        {
            get { return data[index]; }
            set { data[index] = value; Save(); }
        }

        Object IList.this[Int32 index]
        {
            get { return data[index]; }
            set { data[index] = (T)value; Save(); }
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
                Save();
            }
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
                Save();
            }
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
                Save();
            }
        }

        public void Add(T item)
        {
            data.Add(item);
            Save();
        }

        Int32 IList.Add(Object item)
        {
            Int32 i = (data as IList).Add(item);
            Save();
            return i;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            data.AddRange(collection);
            Save();
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            return data.AsReadOnly();
        }

        public Int32 BinarySearch(Int32 index, Int32 count, T item, IComparer<T> comparer)
        {
            return data.BinarySearch(index, count, item, comparer);
        }

        public Int32 BinarySearch(T item)
        {
            return data.BinarySearch(0, data.Count, item, null);
        }

        public Int32 BinarySearch(T item, IComparer<T> comparer)
        {
            return data.BinarySearch(0, data.Count, item, comparer);
        }

        public void Clear()
        {
            data.Clear();
            Save();
        }

        public Boolean Contains(T item)
        {
            return data.Contains(item);
        }

        Boolean IList.Contains(Object item)
        {
            return (data as IList).Contains(item);
        }

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            return data.ConvertAll(converter);
        }

        public void CopyTo(T[] array)
        {
            data.CopyTo(array, 0);
        }

        void ICollection.CopyTo(Array array, Int32 arrayIndex)
        {
            (data as ICollection).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Int32 index, T[] array, Int32 arrayIndex, Int32 count)
        {
            data.CopyTo(index, array, arrayIndex, count);
        }

        public void CopyTo(T[] array, Int32 arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }

        public Boolean Exists(Predicate<T> match)
        {
            return data.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            return data.Find(match);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            return data.FindAll(match);
        }

        public Int32 FindIndex(Predicate<T> match)
        {
            return data.FindIndex(match);
        }

        public Int32 FindIndex(Int32 startIndex, Predicate<T> match)
        {
            return data.FindIndex(startIndex, match);
        }

        public Int32 FindIndex(Int32 startIndex, Int32 count, Predicate<T> match)
        {
            return data.FindIndex(startIndex, count, match);
        }

        public T FindLast(Predicate<T> match)
        {
            return data.FindLast(match);
        }

        public Int32 FindLastIndex(Predicate<T> match)
        {
            return data.FindLastIndex(match);
        }

        public Int32 FindLastIndex(Int32 startIndex, Predicate<T> match)
        {
            return data.FindLastIndex(startIndex, match);
        }

        public Int32 FindLastIndex(Int32 startIndex, Int32 count, Predicate<T> match)
        {
            return data.FindLastIndex(startIndex, count, match);
        }

        public void ForEach(Action<T> action)
        {
            data.ForEach(action);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public List<T> GetRange(Int32 index, Int32 count)
        {
            return data.GetRange(index, count);
        }

        public Int32 IndexOf(T item)
        {
            return data.IndexOf(item);
        }

        Int32 IList.IndexOf(Object item)
        {
            return (data as IList).IndexOf(item);
        }

        public Int32 IndexOf(T item, Int32 index)
        {
            return data.IndexOf(item, index);
        }

        public Int32 IndexOf(T item, Int32 index, Int32 count)
        {
            return data.IndexOf(item, index, count);
        }

        public void Insert(Int32 index, T item)
        {
            data.Insert(index, item);
            Save();
        }

        void IList.Insert(Int32 index, Object item)
        {
            (data as IList).Insert(index, item);
            Save();
        }

        public void InsertRange(Int32 index, IEnumerable<T> collection)
        {
            data.InsertRange(index, collection);
            Save();
        }

        public Int32 LastIndexOf(T item)
        {
            return data.LastIndexOf(item);
        }

        public Int32 LastIndexOf(T item, Int32 index)
        {
            return data.LastIndexOf(item, index);
        }

        public Int32 LastIndexOf(T item, Int32 index, Int32 count)
        {
            return data.LastIndexOf(item, index, count);
        }

        public Boolean Remove(T item)
        {
            Boolean b = data.Remove(item);
            Save();
            return b;
        }

        void IList.Remove(Object item)
        {
            (data as IList).Remove(item);
            Save();
        }

        public Int32 RemoveAll(Predicate<T> match)
        {
            Int32 i = data.RemoveAll(match);
            Save();
            return i;
        }

        public void RemoveAt(Int32 index)
        {
            data.RemoveAt(index);
            Save();
        }

        public void RemoveRange(Int32 index, Int32 count)
        {
            data.RemoveRange(index, count);
            Save();
        }

        public void Reverse()
        {
            data.Reverse();
            Save();
        }

        public void Reverse(Int32 index, Int32 count)
        {
            data.Reverse(index, count);
            Save();
        }

        public void Sort()
        {
            data.Sort();
            Save();
        }

        public void Sort(IComparer<T> comparer)
        {
            data.Sort(comparer);
            Save();
        }

        public void Sort(Int32 index, Int32 count, IComparer<T> comparer)
        {
            data.Sort(index, count, comparer);
            Save();
        }

        public void Sort(Comparison<T> comparison)
        {
            data.Sort(comparison);
            Save();
        }

        public T[] ToArray()
        {
            return data.ToArray();
        }

        public void TrimExcess()
        {
            data.TrimExcess();
            Save();
        }

        public Boolean TrueForAll(Predicate<T> match)
        {
            return data.TrueForAll(match);
        }

        private void Save()
        {
            Directory.CreateDirectory(Paths.data + "shadow/");
            using (DeflateStream stream = new DeflateStream(File.Open(Paths.data + "shadow/" + name + ".dat", FileMode.Create), CompressionMode.Compress, false))
            {
                using (BsonWriter bson = new BsonWriter(stream))
                    new JsonSerializer().Serialize(bson, data, typeof (List<T>));
            }
            File.Copy(Paths.data + "shadow/" + name + ".dat", Paths.data + name + ".dat", true);
        }
    }
}

