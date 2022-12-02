using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GF
{
    /// <summary>
    /// 链表范围,标识首尾节点
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct LinkedListRange<T> : IEnumerable<T>, IEnumerable
    {
        private readonly LinkedListNode<T> _first;
        private readonly LinkedListNode<T> _terminal;

        public LinkedListRange(LinkedListNode<T> first, LinkedListNode<T> last)
        {
            if (first == null || last == null || first == last)
            {
                throw new ArgumentException("range is invalid!");
            }

            _first = first;
            _terminal = last;
        }

        public bool IsValid
        {
            get { return _first != null && _terminal != null && _first != _terminal; }
        }

        public LinkedListNode<T> First
        {
            get { return _first; }
        }

        public LinkedListNode<T> Terminal
        {
            get { return _terminal; }
        }

        public int Count
        {
            get
            {
                if (!IsValid)
                {
                    return 0;
                }

                int count = 0;
                for (LinkedListNode<T> current = _first; current != null && current != _terminal; current = current.Next)
                {
                    count++;
                }

                return count;
            }
        }

        public bool Contains(T value)
        {
            for (LinkedListNode<T> current = _first; current != null && current != _terminal; current = current.Next)
            {
                if (current.Value.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator();
        }

        /// <summary>
        /// 循环访问集合的枚举数。
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly LinkedListRange<T> _linkedListRange;
            private LinkedListNode<T> _currentNode;
            private T _currentValue;

            internal Enumerator(LinkedListRange<T> range)
            {
                if (!range.IsValid)
                {
                    throw new ArgumentException("Range is invalid.");
                }

                _linkedListRange = range;
                _currentNode = _linkedListRange._first;
                _currentValue = default(T);
            }

            /// <summary>
            /// 获取当前结点。
            /// </summary>
            public T Current
            {
                get { return _currentValue; }
            }

            /// <summary>
            /// 获取当前的枚举数。
            /// </summary>
            object IEnumerator.Current
            {
                get { return _currentValue; }
            }

            /// <summary>
            /// 清理枚举数。
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// 获取下一个结点。
            /// </summary>
            /// <returns>返回下一个结点。</returns>
            public bool MoveNext()
            {
                if (_currentNode == null || _currentNode == _linkedListRange._terminal)
                {
                    return false;
                }

                _currentValue = _currentNode.Value;
                _currentNode = _currentNode.Next;
                return true;
            }

            /// <summary>
            /// 重置枚举数。
            /// </summary>
            void IEnumerator.Reset()
            {
                _currentNode = _linkedListRange._first;
                _currentValue = default(T);
            }
        }
    }
}