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
	public class IndexedLinkedList<T> {

        LinkedList<T> data = new LinkedList<T>();
        Dictionary<T, LinkedListNode<T>> index = new Dictionary<T, LinkedListNode<T>>();

        public void Add(T value) {
            index[value] = data.AddLast(value);
        }

        public void RemoveFirst() {
            index.Remove(data.First.Value);
            data.RemoveFirst();
        }

        public void Remove(T value) {
            LinkedListNode<T> node;
            if (index.TryGetValue(value, out node)) {
                data.Remove(node);
                index.Remove(value);
            }
        }

        public int Count {
            get {
                return data.Count;
            }
        }

        public void Clear() {
            data.Clear();
            index.Clear();
        }

        public T First {
            get {
                return data.First.Value;
            }
        }
    }
}
