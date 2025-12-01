#nullable enable
using System.Collections;
using System.Runtime.Serialization;

namespace Masuit.Tools.Abstractions.Systems
{
    [Serializable]
    public sealed class ConcurrentLinkedList<T> :
        ICollection<T>,
        IEnumerable<T>,
        IEnumerable,
        IReadOnlyCollection<T>,
        ICollection,
        IDeserializationCallback,
        ISerializable,
        IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly LinkedList<T> _linkedList;

        #region Properties

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _linkedList.Count;
                }
                finally
                {
                    if (_lock.IsReadLockHeld)
                        _lock.ExitReadLock();
                }
            }
        }

        public bool IsSynchronized => false;
        public object SyncRoot => false;
        public bool IsReadOnly => false;

        public LinkedListNode<T>? First
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _linkedList.First;
                }
                finally
                {
                    if (_lock.IsReadLockHeld)
                        _lock.ExitReadLock();
                }
            }
        }

        public LinkedListNode<T>? Last
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _linkedList.Last;
                }
                finally
                {
                    if (_lock.IsReadLockHeld)
                        _lock.ExitReadLock();
                }
            }
        }

        #endregion

        #region Constructors

        public ConcurrentLinkedList()
        {
            _linkedList = [];
        }

        public ConcurrentLinkedList(IEnumerable<T> collection)
        {
            _linkedList = new LinkedList<T>(collection);
        }

        public ConcurrentLinkedList(SerializationInfo info, StreamingContext context)
        {
            _linkedList = [];
            ISerializable iSerializable = _linkedList;
            iSerializable.GetObjectData(info, context);
        }

        #endregion

        #region Add methods

        public void Add(T item)
        {
            AddLast(item);
        }

        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            _lock.EnterWriteLock();
            try
            {
                _linkedList.AddAfter(node, newNode);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
        {
            _lock.EnterWriteLock();
            try
            {
                return _linkedList.AddAfter(node, value);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            _lock.EnterWriteLock();
            try
            {
                _linkedList.AddBefore(node, newNode);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
        {
            _lock.EnterWriteLock();
            try
            {
                return _linkedList.AddBefore(node, value);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public void AddFirst(LinkedListNode<T> node)
        {
            _lock.EnterWriteLock();
            try
            {
                _linkedList.AddFirst(node);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public LinkedListNode<T> AddFirst(T value)
        {
            _lock.EnterWriteLock();
            try
            {
                return _linkedList.AddFirst(value);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public void AddLast(LinkedListNode<T> node)
        {
            _lock.EnterWriteLock();
            try
            {
                _linkedList.AddLast(node);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public LinkedListNode<T> AddLast(T value)
        {
            _lock.EnterWriteLock();
            try
            {
                return _linkedList.AddLast(value);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        #endregion

        #region List Operations

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _linkedList.Clear();
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return _linkedList.Contains(item);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                _linkedList.CopyTo(array, arrayIndex);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public void CopyTo(Array array, int index)
        {
            _lock.EnterReadLock();
            try
            {
                ((ICollection)_linkedList).CopyTo(array, index);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public LinkedListNode<T>? Find(T value)
        {
            _lock.EnterReadLock();
            try
            {
                return _linkedList.Find(value);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public LinkedListNode<T>? FindLast(T value)
        {
            _lock.EnterReadLock();
            try
            {
                return _linkedList.FindLast(value);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _lock.EnterReadLock();
            try
            {
                _linkedList.GetObjectData(info, context);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public void OnDeserialization(object? sender)
        {
            _linkedList.OnDeserialization(sender);
        }

        public IEnumerator<T> GetEnumerator()
        {
            List<T> snapshot;
            _lock.EnterReadLock();
            try
            {
                snapshot = _linkedList.ToList();
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
            return snapshot.GetEnumerator();
        }

        #endregion

        #region Remove methods

        public void Remove(LinkedListNode<T> node)
        {
            _lock.EnterWriteLock();
            try
            {
                _linkedList.Remove(node);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _linkedList.Remove(item);
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public void RemoveFirst()
        {
            _lock.EnterWriteLock();
            try
            {
                _linkedList.RemoveFirst();
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        public void RemoveLast()
        {
            _lock.EnterWriteLock();
            try
            {
                _linkedList.RemoveLast();
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                _lock.Dispose();
        }

        ~ConcurrentLinkedList()
        {
            Dispose(false);
        }

        #endregion
    }
}