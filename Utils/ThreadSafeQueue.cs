using System.Collections.Generic;

namespace piskworks.Utils
{
    
    /// <summary>
    /// Thread safe queue that can be accessed from different threads without conflict
    /// </summary>
    /// <typeparam name="T">Items in the queue</typeparam>
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