using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Ryadel.Components.Threading
{
    /// <summary>
    /// A LockProvider based upon the SemaphoreSlim class to selectively lock objects, resources or statement blocks 
    /// according to given unique IDs in a sync or async way.
    /// 
    /// SAMPLE USAGE & ADDITIONAL INFO:
    /// - https://www.ryadel.com/en/asp-net-core-lock-threads-async-custom-ids-lockprovider/
    /// - https://github.com/Darkseal/LockProvider/
    /// </summary>
    public class LockProvider<T>
    {
        static readonly LazyConcurrentDictionary<T, InnerSemaphore> lockDictionary = new LazyConcurrentDictionary<T, InnerSemaphore>();

        public LockProvider() { }

        /// <summary>
        /// Blocks the current thread (according to the given ID) until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        public void Wait(T idToLock)
        {
            lockDictionary.GetOrAdd(idToLock, new InnerSemaphore(1, 1)).Wait();
        }

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID) until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        public async Task WaitAsync(T idToLock)
        {
            await lockDictionary.GetOrAdd(idToLock, new InnerSemaphore(1, 1)).WaitAsync();
        }

        public void Release(T idToUnlock)
        {
            InnerSemaphore semaphore;
            if (lockDictionary.TryGetValue(idToUnlock, out semaphore))
            {
                semaphore.Release();
                if (!semaphore.HasWaiters && lockDictionary.TryRemove(idToUnlock, out semaphore))
                    semaphore.Dispose();
            }
        }
    }

    public class InnerSemaphore : IDisposable
    {
        private SemaphoreSlim _semaphore;
        private int _waiters;

        public InnerSemaphore(int initialCount, int maxCount)
        {
            _semaphore = new SemaphoreSlim(initialCount, maxCount);
            _waiters = 0;
        }

        public void Wait()
        {
            _waiters++;
            _semaphore.Wait();
        }

        public async Task WaitAsync()
        {
            _waiters++;
            await _semaphore.WaitAsync();
        }

        public void Release()
        {
            _waiters--;
            _semaphore.Release();
        }

        public void Dispose()
        {
            if (_semaphore != null)
                _semaphore.Dispose();
        }
        public bool HasWaiters { get { return _waiters > 0; } }
    }

    public class LazyConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _concurrentDictionary;

        public LazyConcurrentDictionary()
        {
            _concurrentDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            var lazyResult = _concurrentDictionary.GetOrAdd(key, k => new Lazy<TValue>(() => value, LazyThreadSafetyMode.ExecutionAndPublication));
            return lazyResult.Value;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            var lazyResult = _concurrentDictionary.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));
            return lazyResult.Value;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Lazy<TValue> lazyResult;
            var success = _concurrentDictionary.TryGetValue(key, out lazyResult);
            value = (success) ? lazyResult.Value : default(TValue);
            return success;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            Lazy<TValue> lazyResult;
            var success = _concurrentDictionary.TryRemove(key, out lazyResult);
            value = (success) ? lazyResult.Value : default(TValue);
            return success;
        }
    }
}
