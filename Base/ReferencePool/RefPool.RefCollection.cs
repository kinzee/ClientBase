using System;
using System.Collections.Generic;

namespace GFramework
{
    public static partial class RefPool
    {
        /// <summary>
        /// 引用集合
        /// </summary>
        private sealed class RefCollection
        {
            private readonly Queue<IRef> _references;
            private readonly Type _referenceType;
            private int _usingReferenceCount;
            private int _acquireReferenceCount;
            private int _releaseReferenceCount;
            private int _addReferenceCount;
            private int _removeReferenceCount;

            public RefCollection(Type referenceType)
            {
                _references = new Queue<IRef>();
                _referenceType = referenceType;
                _usingReferenceCount = 0;
                _acquireReferenceCount = 0;
                _releaseReferenceCount = 0;
                _addReferenceCount = 0;
                _removeReferenceCount = 0;
            }

            public Type ReferenceType
            {
                get { return _referenceType; }
            }

            public int UnusedReferenceCount
            {
                get { return _references.Count; }
            }

            public int UsingReferenceCount
            {
                get { return _usingReferenceCount; }
            }

            public int AcquireReferenceCount
            {
                get { return _acquireReferenceCount; }
            }

            public int ReleaseReferenceCount
            {
                get { return _releaseReferenceCount; }
            }

            public int AddReferenceCount
            {
                get { return _addReferenceCount; }
            }

            public int RemoveReferenceCount
            {
                get { return _removeReferenceCount; }
            }

            public T Acquire<T>() where T : class, IRef, new()
            {
                if (typeof(T) != _referenceType)
                {
                    throw new GFrameworkException("Type is invalid.");
                }

                _usingReferenceCount++;
                _acquireReferenceCount++;
                lock (_references)
                {
                    if (_references.Count > 0)
                    {
                        return (T)_references.Dequeue();
                    }
                }

                _addReferenceCount++;
                return new T();
            }

            public IRef Acquire()
            {
                _usingReferenceCount++;
                _acquireReferenceCount++;
                lock (_references)
                {
                    if (_references.Count > 0)
                    {
                        return _references.Dequeue();
                    }
                }

                _addReferenceCount++;
                return (IRef)Activator.CreateInstance(_referenceType);
            }

            public void Release(IRef reference)
            {
                reference.Clear();
                lock (_references)
                {
                    if (_enableStrictCheck && _references.Contains(reference))
                    {
                        throw new GFrameworkException("The reference has been released.");
                    }

                    _references.Enqueue(reference);
                }

                _releaseReferenceCount++;
                _usingReferenceCount--;
            }

            public void Add<T>(int count) where T : class, IRef, new()
            {
                if (typeof(T) != _referenceType)
                {
                    throw new GFrameworkException("Type is invalid.");
                }

                lock (_references)
                {
                    _addReferenceCount += count;
                    while (count-- > 0)
                    {
                        _references.Enqueue(new T());
                    }
                }
            }

            public void Add(int count)
            {
                lock (_references)
                {
                    _addReferenceCount += count;
                    while (count-- > 0)
                    {
                        _references.Enqueue((IRef)Activator.CreateInstance(_referenceType));
                    }
                }
            }

            public void Remove(int count)
            {
                lock (_references)
                {
                    if (count > _references.Count)
                    {
                        count = _references.Count;
                    }

                    _removeReferenceCount += count;
                    while (count-- > 0)
                    {
                        _references.Dequeue();
                    }
                }
            }

            public void RemoveAll()
            {
                lock (_references)
                {
                    _removeReferenceCount += _references.Count;
                    _references.Clear();
                }
            }
        }
    }
}