using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GFramework
{
    /// <summary>
    /// 带有缓存节点的链表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CachedLinkedList<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private readonly LinkedList<T> _linkedList;
        private readonly Queue<LinkedListNode<T>> _cachedNodes;

        public CachedLinkedList()
        {
            _linkedList = new LinkedList<T>();
            _cachedNodes = new Queue<LinkedListNode<T>>();
        }

        public int Count
        {
            get { return _linkedList.Count; }
        }

        public int CachedNodeCount
        {
            get { return _cachedNodes.Count; }
        }

        public LinkedListNode<T> First
        {
            get { return _linkedList.First; }
        }

        public LinkedListNode<T> Last
        {
            get { return _linkedList.Last; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<T>)_linkedList).IsReadOnly; }
        }

        public object SyncRoot
        {
            get { return ((ICollection)_linkedList).SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return ((ICollection)_linkedList).IsSynchronized; }
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
        {
            LinkedListNode<T> newNode = AcquireNode(value);
            _linkedList.AddAfter(node, newNode);
            return newNode;
        }

        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            _linkedList.AddAfter(node, newNode);
        }

        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
        {
            LinkedListNode<T> newNode = AcquireNode(value);
            _linkedList.AddBefore(node, newNode);
            return newNode;
        }

        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            _linkedList.AddBefore(node, newNode);
        }

        public LinkedListNode<T> AddFirst(T value)
        {
            LinkedListNode<T> node = AcquireNode(value);
            _linkedList.AddFirst(node);
            return node;
        }

        public void AddFirst(LinkedListNode<T> node)
        {
            _linkedList.AddFirst(node);
        }

        public LinkedListNode<T> AddLast(T value)
        {
            LinkedListNode<T> node = AcquireNode(value);
            _linkedList.AddLast(node);
            return node;
        }

        public void AddLast(LinkedListNode<T> node)
        {
            _linkedList.AddLast(node);
        }

        public void Clear()
        {
            LinkedListNode<T> current = _linkedList.First;
            while (current != null)
            {
                ReleaseNode(current);
                current = current.Next;
            }

            _linkedList.Clear();
        }

        public void ClearCachedNodes()
        {
            _cachedNodes.Clear();
        }

        public bool Contains(T value)
        {
            return _linkedList.Contains(value);
        }

        public bool Remove(T value)
        {
            LinkedListNode<T> node = _linkedList.Find(value);
            if (node != null)
            {
                _linkedList.Remove(node);
                ReleaseNode(node);
                return true;
            }

            return false;
        }

        public void Remove(LinkedListNode<T> node)
        {
            _linkedList.Remove(node);
            ReleaseNode(node);
        }

        public void RemoveFirst()
        {
            LinkedListNode<T> first = _linkedList.First;
            if (first == null)
            {
                throw new ArgumentException("First is invalid.");
            }

            _linkedList.RemoveFirst();
            ReleaseNode(first);
        }

        public void RemoveLast()
        {
            LinkedListNode<T> last = _linkedList.Last;
            if (last == null)
            {
                throw new ArgumentException("Last is invalid.");
            }

            _linkedList.RemoveLast();
            ReleaseNode(last);
        }

        private LinkedListNode<T> AcquireNode(T value)
        {
            LinkedListNode<T> node = null;
            if (_cachedNodes.Count > 0)
            {
                node = _cachedNodes.Dequeue();
                node.Value = value;
            }
            else
            {
                node = new LinkedListNode<T>(value);
            }

            return node;
        }

        private void ReleaseNode(LinkedListNode<T> node)
        {
            node.Value = default(T);
            _cachedNodes.Enqueue(node);
        }

        public void Add(T item)
        {
            AddLast(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _linkedList.CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_linkedList).CopyTo(array, index);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_linkedList);
        }

        /// <summary>
        /// 循环访问集合的枚举数。
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private LinkedList<T>.Enumerator _enumerator;

            internal Enumerator(LinkedList<T> linkedList)
            {
                if (linkedList == null)
                {
                    throw new ArgumentException("Linked list is invalid.");
                }

                _enumerator = linkedList.GetEnumerator();
            }

            /// <summary>
            /// 获取当前结点。
            /// </summary>
            public T Current
            {
                get { return _enumerator.Current; }
            }

            /// <summary>
            /// 获取当前的枚举数。
            /// </summary>
            object IEnumerator.Current
            {
                get { return _enumerator.Current; }
            }

            /// <summary>
            /// 清理枚举数。
            /// </summary>
            public void Dispose()
            {
                _enumerator.Dispose();
            }

            /// <summary>
            /// 获取下一个结点。
            /// </summary>
            /// <returns>返回下一个结点。</returns>
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            /// <summary>
            /// 重置枚举数。
            /// </summary>
            void IEnumerator.Reset()
            {
                ((IEnumerator<T>)_enumerator).Reset();
            }
        }
    }
}