using System;
using System.Collections.Generic;

namespace GF
{
    public  static partial class RefPool
    {
        private sealed class RefCollection
        {
             private readonly Queue<IRef> _references;
            private readonly Type _referenceType;
            private int m_UsingReferenceCount;
            private int m_AcquireReferenceCount;
            private int m_ReleaseReferenceCount;
            private int m_AddReferenceCount;
            private int m_RemoveReferenceCount;

            public RefCollection(Type referenceType)
            {
                _references = new Queue<IRef>();
                _referenceType = referenceType;
                m_UsingReferenceCount = 0;
                m_AcquireReferenceCount = 0;
                m_ReleaseReferenceCount = 0;
                m_AddReferenceCount = 0;
                m_RemoveReferenceCount = 0;
            }

            public Type ReferenceType
            {
                get
                {
                    return _referenceType;
                }
            }

            public int UnusedReferenceCount
            {
                get
                {
                    return _references.Count;
                }
            }

            public int UsingReferenceCount
            {
                get
                {
                    return m_UsingReferenceCount;
                }
            }

            public int AcquireReferenceCount
            {
                get
                {
                    return m_AcquireReferenceCount;
                }
            }

            public int ReleaseReferenceCount
            {
                get
                {
                    return m_ReleaseReferenceCount;
                }
            }

            public int AddReferenceCount
            {
                get
                {
                    return m_AddReferenceCount;
                }
            }

            public int RemoveReferenceCount
            {
                get
                {
                    return m_RemoveReferenceCount;
                }
            }

            public T Acquire<T>() where T : class, IRef, new()
            {
                if (typeof(T) != _referenceType)
                {
                    throw new GameFrameworkException("Type is invalid.");
                }

                m_UsingReferenceCount++;
                m_AcquireReferenceCount++;
                lock (_references)
                {
                    if (_references.Count > 0)
                    {
                        return (T)_references.Dequeue();
                    }
                }

                m_AddReferenceCount++;
                return new T();
            }

            public IRef Acquire()
            {
                m_UsingReferenceCount++;
                m_AcquireReferenceCount++;
                lock (_references)
                {
                    if (_references.Count > 0)
                    {
                        return _references.Dequeue();
                    }
                }

                m_AddReferenceCount++;
                return (IRef)Activator.CreateInstance(_referenceType);
            }

            public void Release(IRef reference)
            {
                reference.Clear();
                lock (_references)
                {
                    if (m_EnableStrictCheck && _references.Contains(reference))
                    {
                        throw new GameFrameworkException("The reference has been released.");
                    }

                    _references.Enqueue(reference);
                }

                m_ReleaseReferenceCount++;
                m_UsingReferenceCount--;
            }

            public void Add<T>(int count) where T : class, IRef, new()
            {
                if (typeof(T) != _referenceType)
                {
                    throw new GameFrameworkException("Type is invalid.");
                }

                lock (_references)
                {
                    m_AddReferenceCount += count;
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
                    m_AddReferenceCount += count;
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

                    m_RemoveReferenceCount += count;
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
                    m_RemoveReferenceCount += _references.Count;
                    _references.Clear();
                }
            }
            
        }
    }
}