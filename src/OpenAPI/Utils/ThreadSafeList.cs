using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAPI.Utils
{
	#region License & Information

	// This notice must be kept visible in the source.
	//
	// This section of source code belongs to Rick@AIBrain.Org unless otherwise specified,
	// or the original license has been overwritten by the automatic formatting of this code.
	// Any unmodified sections of source code borrowed from other projects retain their original license and thanks goes to the Authors.
	//
	// Donations and Royalties can be paid via
	// PayPal: paypal@aibrain.org
	// bitcoin:1Mad8TxTqxKnMiHuZxArFvX8BuFEB9nqX2
	// bitcoin:1NzEsF7eegeEWDr5Vr9sSSgtUC4aL6axJu
	// litecoin:LeUxdU2w3o6pLZGVys5xpDZvvo8DUrjBp9
	//
	// Usage of the source code or compiled binaries is AS-IS.
	// I am not responsible for Anything You Do.
	//
	// Contact me by email if you have any questions or helpful criticism.
	//
	// "Librainian/ThreadSafeList.cs" was last cleaned by Rick on 2014/08/13 at 10:37 PM
	//
	// Taken from: https://github.com/AIBrain/Librainian
	// Modified by: Kenny van Vulpen (https://github.com/kennyvv/)

	#endregion
	
	[DebuggerDisplay("Count={Count}")]
	public sealed class ThreadSafeList<T> : IList<T>
    {
        private readonly ReaderWriterLockSlim _lock;
        private readonly List<T> _list;
 
        public ThreadSafeList()
        {
            _list = new List<T>();
            _lock = new ReaderWriterLockSlim();
        }

        public ThreadSafeList(IEnumerable<T> items)
        {
            _list = new List<T>(items);
            _lock = new ReaderWriterLockSlim();
        }
 
        public int Count
        {
            get
            {
                _lock.EnterReadLock();
 
                int count;
                try
                {
                    count = _list.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
 
                return count;
            }
        }
 
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
 
        public T this[int index]
        {
            get
            {
                _lock.EnterReadLock();
 
                T result;
                try
                {
                    result = _list[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
 
                return result;
            }
            set
            {
                _lock.EnterWriteLock();
 
                try
                {
                    _list[index] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public bool TryAdd(T item)
        {
            _lock.EnterWriteLock();

            try
            {
                if (_list.Contains(item))
                    return false;
                
                _list.Add(item);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        public void Add(T item)
        {
            _lock.EnterWriteLock();
 
            try
            {
                _list.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
 
        public int IndexOf(T item)
        {
            _lock.EnterReadLock();
 
            int result;
            try
            {
                result = _list.IndexOf(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
 
            return result;
        }
 
        public void Insert(int index, T item)
        {
            _lock.EnterWriteLock();
 
            try
            {
                _list.Insert(index, item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
 
        public void RemoveAt(int index)
        {
            _lock.EnterWriteLock();
 
            try
            {
                _list.RemoveAt(index);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
 
        public void Clear()
        {
            _lock.EnterWriteLock();
 
            try
            {
                _list.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
 
        public bool Contains(T item)
        {
            _lock.EnterReadLock();
 
            bool result;
            try
            {
                result = _list.Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
 
            return result;
        }
 
        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.EnterWriteLock();
 
            try
            {
                _list.CopyTo(array, arrayIndex);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
 
        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
 
            bool result;
            try
            {
                result = _list.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
 
            return result;
        }
 
        public IEnumerator<T> GetEnumerator()
        {
            _lock.EnterReadLock();
 
            try
            {
                foreach (T value in _list)
                {
                    yield return value;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
 
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
