using System.Collections.Generic;

namespace piskworks
{
    public class ThreadSafeQueue<T>
    {
        private Queue<T> _queue;

        public int Count {
            get {
                lock (_queue) {
                    return _queue.Count;
                }
            }
        }

        public ThreadSafeQueue()
        {
            _queue = new Queue<T>();
        }

        public void Enqueue(T item)
        {
            lock (_queue) {
                _queue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            lock (_queue) {
                if (_queue.Count > 0) {
                    return _queue.Dequeue();
                }
                return default(T);
            }
        }
    }
}