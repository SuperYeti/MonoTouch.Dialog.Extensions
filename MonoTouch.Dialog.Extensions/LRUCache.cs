/*
Copyright (c) 2010 Novell Inc.

Author: Miguel de Icaza
Updates: Warren Moxley

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTouch.Dialog.Extensions
{
	public class LRUCache<TKey, TValue> : IDictionary<TKey, TValue> {

        Dictionary<TKey, TValue> data;
        IndexedLinkedList<TKey> lruList = new IndexedLinkedList<TKey>();
        ICollection<KeyValuePair<TKey, TValue>> dataAsCollection;
        int capacity;

        public LRUCache(int capacity) {

            if (capacity <= 0) {
                throw new ArgumentException("capacity should always be bigger than 0");
            }

            data = new Dictionary<TKey, TValue>(capacity);
            dataAsCollection = data;
            this.capacity = capacity;
        }

        public void Add(TKey key, TValue value) {
            if (!ContainsKey(key)) {
                this[key] = value;
            } else {
                throw new ArgumentException("An attempt was made to insert a duplicate key in the cache.");
            }
        }

        public bool ContainsKey(TKey key) {
            return data.ContainsKey(key);
        }

        public ICollection<TKey> Keys {
            get {
                return data.Keys;
            }
        }

        public bool Remove(TKey key) {
            bool existed = data.Remove(key);
            lruList.Remove(key);
            return existed;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return data.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values {
            get { return data.Values; }
        }

        public TValue this[TKey key] {
            get {
				try
				{
	                var value = data[key];
	                lruList.Remove(key);
	                lruList.Add(key);
	                return value;
				}
				catch
				{
					return default(TValue);
				}
            }
            set {
                data[key] = value;
                lruList.Remove(key);
                lruList.Add(key);
				
				if (data.Count > capacity) {
                    ReclaimLRU(1);
                }
            }
        }
		
		public void ReclaimLRU(int itemsToReclaim)
		{
			while(--itemsToReclaim >= 0 && lruList.First != null)
			{
			    Remove(lruList.First);
	            //lruList.RemoveFirst();
			}
        
		}

        public void Add(KeyValuePair<TKey, TValue> item) {
            Add(item.Key, item.Value);
        }

        public void Clear() {
            data.Clear();
            lruList.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            return dataAsCollection.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            dataAsCollection.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return data.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {

            bool removed = dataAsCollection.Remove(item);
            if (removed) {
                lruList.Remove(item.Key);
            }
            return removed;
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return dataAsCollection.GetEnumerator();
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ((System.Collections.IEnumerable)data).GetEnumerator();
        }

    }
	
}
