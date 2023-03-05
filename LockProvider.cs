using System.Collections.Concurrent;

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
        static readonly LazyConcurrentDictionary<T, InnerSemaphore> LockDictionary = new LazyConcurrentDictionary<T, InnerSemaphore>();

        public LockProvider() { }

        /// <summary>
        /// Blocks the current thread (according to the given ID) until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        public void Wait(T idToLock)
        {
            LockDictionary.GetOrAdd(idToLock, new InnerSemaphore(1, 1)).Wait();
        }

        /// <summary>
        /// Blocks the current thread (according to the given ID) until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        /// <param name="millisecondsTimeout"></param>
        public bool Wait(T idToLock, int millisecondsTimeout)
        {
            var semaphore = LockDictionary.GetOrAdd(idToLock, new InnerSemaphore(1, 1));
            if (!semaphore.Wait(millisecondsTimeout))
            {
                if (!semaphore.HasWaiters && LockDictionary.TryRemove(idToLock, out semaphore))
                    semaphore.Dispose();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Blocks the current thread (according to the given ID) until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="token"></param>
        public bool Wait(T idToLock, int millisecondsTimeout, CancellationToken token)
        {
            var semaphore = LockDictionary.GetOrAdd(idToLock, new InnerSemaphore(1, 1));
            if (!semaphore.Wait(millisecondsTimeout, token))
            {
                if (!semaphore.HasWaiters && LockDictionary.TryRemove(idToLock, out semaphore))
                    semaphore.Dispose();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID) until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        public async Task WaitAsync(T idToLock)
        {
            await LockDictionary.GetOrAdd(idToLock, new InnerSemaphore(1, 1)).WaitAsync();
        }

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID) until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        /// <param name="millisecondsTimeout"></param>
        public async Task<bool> WaitAsync(T idToLock, int millisecondsTimeout)
        {
            var semaphore = LockDictionary.GetOrAdd(idToLock, new InnerSemaphore(1, 1));
            if (!await semaphore.WaitAsync(millisecondsTimeout))
            {
                if (!semaphore.HasWaiters && LockDictionary.TryRemove(idToLock, out semaphore))
                    semaphore.Dispose();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID) until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="token"></param>
        public async Task<bool> WaitAsync(T idToLock, int millisecondsTimeout, CancellationToken token)
        {
            var semaphore = LockDictionary.GetOrAdd(idToLock, new InnerSemaphore(1, 1));
            if (!await semaphore.WaitAsync(millisecondsTimeout, token))
            {
                if (!semaphore.HasWaiters && LockDictionary.TryRemove(idToLock, out semaphore))
                    semaphore.Dispose();
                return false;
            }
            return true;
        }

        public void Release(T idToUnlock)
        {
            if (LockDictionary.TryGetValue(idToUnlock, out var semaphore))
            {
                semaphore.Release();
                if (!semaphore.HasWaiters && LockDictionary.TryRemove(idToUnlock, out semaphore))
                    semaphore.Dispose();
            }
        }
    }

    public class InnerSemaphore : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private int _waiters;

        public InnerSemaphore(int initialCount, int maxCount)
        {
            _semaphore = new SemaphoreSlim(initialCount, maxCount);
            _waiters = 0;
        }

        public void Wait()
        {
            Interlocked.Increment(ref _waiters);
            _semaphore.Wait();
        }

        public bool Wait(int millisecondsTimeout)
        {
            Interlocked.Increment(ref _waiters);
            if (!_semaphore.Wait(millisecondsTimeout))
            {
                Interlocked.Decrement(ref _waiters);
                return false;
            }
            return true;
        }

        public void Wait(CancellationToken token)
        {
            Interlocked.Increment(ref _waiters);
            _semaphore.Wait(token);
        }

        public bool Wait(int millisecondsTimeout, CancellationToken token)
        {
            Interlocked.Increment(ref _waiters);
            if (!_semaphore.Wait(millisecondsTimeout, token))
            {
                Interlocked.Decrement(ref _waiters);
                return false;
            }
            return true;
        }

        public async Task WaitAsync()
        {
            Interlocked.Increment(ref _waiters);
            await _semaphore.WaitAsync();
        }

        public async Task<bool> WaitAsync(int millisecondsTimeout)
        {
            Interlocked.Increment(ref _waiters);
            if (!await _semaphore.WaitAsync(millisecondsTimeout))
            {
                Interlocked.Decrement(ref _waiters);
                return false;
            }
            return true;
        }

        public async Task WaitAsync(CancellationToken token)
        {
            Interlocked.Increment(ref _waiters);
            await _semaphore.WaitAsync(token);
        }

        public async Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken token)
        {
            Interlocked.Increment(ref _waiters);
            if (!await _semaphore.WaitAsync(millisecondsTimeout, token))
            {
                Interlocked.Decrement(ref _waiters);
                return false;
            }
            return true;
        }

        public void Release()
        {
            _semaphore.Release();
            Interlocked.Decrement(ref _waiters);
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
        public bool HasWaiters => _waiters > 0;
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
            var success = _concurrentDictionary.TryGetValue(key, out var lazyResult);
            value = success ? lazyResult.Value : default;
            return success;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            var success = _concurrentDictionary.TryRemove(key, out var lazyResult);
            value = success ? lazyResult.Value : default;
            return success;
        }
    }
}
