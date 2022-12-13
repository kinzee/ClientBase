using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GFramework
{
    /// <summary>
    /// 多值l
    /// </summary>
    public class MultiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, LinkedListRange<TValue>>>, IEnumerable
    {
        private readonly CachedLinkedList<TValue> _linkedList;
        private readonly Dictionary<TKey, LinkedListRange<TValue>> _dictionary;

        public MultiDictionary()
        {
            _linkedList = new CachedLinkedList<TValue>();
            _dictionary = new Dictionary<TKey, LinkedListRange<TValue>>();
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public LinkedListRange<TValue> this[TKey key]
        {
            get
            {
                LinkedListRange<TValue> range = default(LinkedListRange<TValue>);
                _dictionary.TryGetValue(key, out range);
                return range;
            }
        }

        public void Clear()
        {
            _linkedList.Clear();
            _dictionary.Clear();
        }

        public bool Contains(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Contains(TKey key, TValue value)
        {
            LinkedListRange<TValue> range = default(LinkedListRange<TValue>);
            if (_dictionary.TryGetValue(key, out range))
            {
                return range.Contains(value);
            }

            return false;
        }

        public bool TryGetValue(TKey key, out LinkedListRange<TValue> range)
        {
            return _dictionary.TryGetValue(key, out range);
        }

        public void Add(TKey key, TValue value)
        {
            LinkedListRange<TValue> range = default(LinkedListRange<TValue>);
            if (_dictionary.TryGetValue(key, out range))
            {
                _linkedList.AddBefore(range.Terminal, value);
            }
            else
            {
                LinkedListNode<TValue> first = _linkedList.AddLast(value);
                LinkedListNode<TValue> last = _linkedList.AddLast(default(TValue));
                _dictionary.Add(key, new LinkedListRange<TValue>(first, last));
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            LinkedListRange<TValue> range = default(LinkedListRange<TValue>);
            if (_dictionary.TryGetValue(key, out range))
            {
                for (LinkedListNode<TValue> current = range.First;
                     current != null && current != range.Terminal;
                     current = current.Next)
                {
                    if (current.Value.Equals(value))
                    {
                        if (current == range.First)
                        {
                            LinkedListNode<TValue> next = current.Next;
                            if (next == range.Terminal)
                            {
                                _linkedList.Remove(next);
                                _dictionary.Remove(key);
                            }
                            else
                            {
                                _dictionary[key] = new LinkedListRange<TValue>(next, range.Terminal);
                            }
                        }

                        _linkedList.Remove(current);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool RemoveAll(TKey key)
        {
            LinkedListRange<TValue> range = default(LinkedListRange<TValue>);
            if (_dictionary.TryGetValue(key, out range))
            {
                _dictionary.Remove(key);
                LinkedListNode<TValue> current = range.First;
                while (current != null)
                {
                    LinkedListNode<TValue> next = current != range.Terminal ? current.Next : null;
                    _linkedList.Remove(current);
                    current = next;
                }

                return true;
            }

            return false;
        }


        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator<KeyValuePair<TKey, LinkedListRange<TValue>>>
            IEnumerable<KeyValuePair<TKey, LinkedListRange<TValue>>>.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 循环访问集合的枚举数。
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, LinkedListRange<TValue>>>, IEnumerator
        {
            private Dictionary<TKey, LinkedListRange<TValue>>.Enumerator _enumerator;

            internal Enumerator(Dictionary<TKey, LinkedListRange<TValue>> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentException("Dictionary is invalid.");
                }

                _enumerator = dictionary.GetEnumerator();
            }

            /// <summary>
            /// 获取当前结点。
            /// </summary>
            public KeyValuePair<TKey, LinkedListRange<TValue>> Current
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
                ((IEnumerator<KeyValuePair<TKey, LinkedListRange<TValue>>>)_enumerator).Reset();
            }
        }
    }
}