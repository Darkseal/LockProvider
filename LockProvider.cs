using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Ryadel.Components.Threading
{
    /// <summary>
    /// A LockProvider based upon the SemaphoreSlim class to selectively lock objects, resources or statement blocks 
    /// according to given unique IDs in a sync or async way.
    /// </summary>
    public class LockProvider<T>
    {
        static readonly ConcurrentDictionary<T, SemaphoreSlim> lockDictionary = new ConcurrentDictionary<T, SemaphoreSlim>();


        public LockProvider() { }

        /// <summary>
        /// Blocks the current thread (according to the given ID) until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        public void Wait(T idToLock)
        {
            lockDictionary.GetOrAdd(idToLock, new SemaphoreSlim(1, 1)).Wait();
        }

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID) until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        public async Task WaitAsync(T idToLock)
        {
            await lockDictionary.GetOrAdd(idToLock, new SemaphoreSlim(1, 1)).WaitAsync();
        }

        /// <summary>
        /// Releases the lock (according to the given ID)
        /// </summary>
        /// <param name="idToUnlock">the unique ID to unlock</param>
        public void Release(T idToUnlock)
        {
            SemaphoreSlim semaphore;
            if (lockDictionary.TryRemove(idToUnlock, out semaphore))
            {
                semaphore.Release();
            }
        }
    }
}
