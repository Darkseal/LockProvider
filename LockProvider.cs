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
        static readonly ConcurrentDictionary<T, InnerSemaphore> lockDictionary = new ConcurrentDictionary<T, InnerSemaphore>();

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
                semaphore.Release();
        }
    }

    public class InnerSemaphore
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
            if (_waiters == 0)
                _semaphore.Dispose();
        }
    }
}
