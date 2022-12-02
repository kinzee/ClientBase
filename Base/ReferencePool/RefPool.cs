using System;
using System.Collections.Generic;

namespace GF
{
    public static partial class RefPool
    {
        private static readonly Dictionary<Type, RefCollection> _refCollections = new Dictionary<Type, RefCollection>();
        private static bool _enableStrictCheck = false;

        public static bool EnableStrictCheck
        {
            get { return _enableStrictCheck; }
            set { _enableStrictCheck = value; }
        }

        public static int Count
        {
            get { return _refCollections.Count; }
        }

        public static void ClearAll()
        {
            lock (_refCollections)
            {
                foreach (KeyValuePair<Type, RefCollection> refCollection in _refCollections)
                {
                    refCollection.Value.RemoveAll();
                }

                _refCollections.Clear();
            }
        }

        public static T Acquire<T>() where T : class, IRef, new()
        {
            return GetRefCollection(typeof(T)).Acquire<T>();
        }

        public static IRef Acquire(Type refType)
        {
            InternalCheckReferenceType(refType);
            return GetRefCollection(refType).Acquire();
        }

        public static void Release(IRef reference)
        {
            if (reference == null)
            {
                throw new GFException("Reference is invalid.");
            }

            Type refType = reference.GetType();
            InternalCheckReferenceType(refType);
            GetRefCollection(refType).Release(reference);
        }

        public static void Add<T>(int count) where T : class, IRef, new()
        {
            GetRefCollection(typeof(T)).Add<T>(count);
        }

        public static void Add(Type refType, int count)
        {
            InternalCheckReferenceType(refType);
            GetRefCollection(refType).Add(count);
        }

        public static void Remove<T>(int count) where T : class, IRef
        {
            GetRefCollection(typeof(T)).Remove(count);
        }

        public static void Remove(Type refType, int count)
        {
            InternalCheckReferenceType(refType);
            GetRefCollection(refType).Remove(count);
        }

        public static void RemoveAll<T>() where T : class, IRef
        {
            GetRefCollection(typeof(T)).RemoveAll();
        }

        public static void RemoveAll(Type refType)
        {
            InternalCheckReferenceType(refType);
            GetRefCollection(refType).RemoveAll();
        }

        private static void InternalCheckReferenceType(Type refType)
        {
            if (!_enableStrictCheck)
            {
                return;
            }

            if (refType == null)
            {
                throw new GFException("Reference type is invalid.");
            }

            if (!refType.IsClass || refType.IsAbstract)
            {
                throw new GFException("Reference type is not a non-abstract class type.");
            }

            if (!typeof(IRef).IsAssignableFrom(refType))
            {
                throw new GFException(string.Format("Reference type '{0}' is invalid.",
                    refType.FullName));
            }
        }

        /// <summary>
        /// 获取指定引用类型的引用池，没有则创建
        /// </summary>
        /// <param name="refType">类型</param>
        /// <returns></returns>
        /// <exception cref="GFException"></exception>
        private static RefCollection GetRefCollection(Type refType)
        {
            if (refType == null)
            {
                throw new GFException("ReferenceType is invalid.");
            }

            RefCollection refCollection = null;
            lock (_refCollections)
            {
                if (!_refCollections.TryGetValue(refType, out refCollection))
                {
                    refCollection = new RefCollection(refType);
                    _refCollections.Add(refType, refCollection);
                }
            }

            return refCollection;
        }
    }
}