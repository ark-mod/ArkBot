using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Threading
{
    public delegate void ItemAddedEventHandler<T>(object sender, T item);

    public class ConcurrentQueueUnique<T> : IProducerConsumerCollection<T>
    {
        private object _lock = new object();
        private ConcurrentQueue<T> _queue;
        private Dictionary<T, bool> _set;
        private object _syncRoot = new object();

        public event ItemAddedEventHandler<T> ItemAdded;

        public ConcurrentQueueUnique()
        {
            _queue = new ConcurrentQueue<T>();
            _set = new Dictionary<T, bool>();
        }

        public int Count { get { return _set.Count; } }
        public bool IsSynchronized {  get { return true; } }

        public object SyncRoot {  get { return _syncRoot; } }

        public void CopyTo(Array array, int index)
        {
            _queue.ToArray().CopyTo(array, index);
        }

        public void CopyTo(T[] array, int index)
        {
            _queue.CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        public T[] ToArray()
        {
            return _queue.ToArray();
        }

        //hacky but suits our purpose (performance of no concern)
        public bool TryAdd(T item)
        {
            if (item == null) throw new ArgumentNullException("Item must not be null.");

            lock (_lock)
            {
                if (!_set.ContainsKey(item))
                {
                    _set.Add(item, true);
                    _queue.Enqueue(item);
                    OnItemAdded(item);
                }
                return true;
            }
        }

        //hacky but suits our purpose (performance of no concern)
        public bool TryTake(out T item)
        {
            lock (_lock)
            {
                var success = _queue.TryDequeue(out item);
                if (success) _set.Remove(item);

                return success;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        protected void OnItemAdded(T item)
        {
            ItemAdded?.Invoke(this, item);
        }
    }
}
