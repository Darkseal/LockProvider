# LockProvider
A lightweight C# class that can be used to selectively lock objects, resources or statement blocks according to given unique IDs.

## Introduction
If you're working with ASP.NET or ASP.NET Core C#  and you need to prevent (or restrict) the access and/or execution of certain methods from multiple threads, the best approach you can arguably use is a semaphore-based implementation using one of the various techniques provided by the framework's `System.Threading` namespace, such as the `lock` statement or the `Semaphore` and `SemaphoreSlim` classes. More specifically:

* **The lock statement** acquires the mutual-exclusion lock for a given object, executes a statement block, and then releases the lock: while a lock is held, the thread that holds the lock can again acquire and release the lock. Any other thread is blocked from acquiring the lock and waits until the lock is released.
* **The Semaphore class** represents a named (systemwide) or local semaphore: it is a thin wrapper around the Win32 semaphore object. Win32 semaphores are counting semaphores, which can be used to control access to a pool of resources.
* **The SemaphoreSlim class** represents a lightweight, fast semaphore that can be used for waiting within a single process when wait times are expected to be very short. SemaphoreSlim relies as much as possible on synchronization primitives provided by the common language runtime (CLR): however, it also provides lazily initialized, kernel-based wait handles as necessary to support waiting on multiple semaphores. 

## Why LockProvider?
What I found to be missing in both `lock` and `SemaphoreSlim` approaches was a method to conditionally lock the thread based upon an arbitrary unique ID, which could be very useful in a number of common web-related scenarios, such as whenever we want to lock multiple threads executed by the same user from entering a given statement block, without restricting that block to other users/threads.

For that very reason I've put togheter **LockProvider**, a simple class that can be used to selectively lock objects, resources or statement blocks according to given unique IDs. The class works with multiple SemaphoreSlim objects, hence can be used to perform synchronous or asynchronous locks.

## Use case
Here's a sample usage that shows how the LockProvider class can be used to asynchronously lock a statement block:

    public static class SampleClass
    {
        private static LockProvider<int> LockProvider = 
          new LockProvider<int>();

        public static async Task DoSomething(currentUserId)
        {
            // lock for currentUserId only
            await LockProvider.WaitAsync(currentUserId);
        
            try
            {
                // access objects, execute statements, etc.
                result = await DoStuff();
            }
            finally
            {
                // release the lock
                LockProvider.Release(currentUserId);
            }
            return result;
        }
    }
    
To perform a synchronous lock, just use the `Wait()` method instead of `WaitAsync()` in the following way:

    public static class SampleClass
    {
        private static LockProvider<int> LockProvider = 
          new LockProvider<int>();

        public static async void DoSomething(currentUserId)
        {
            // lock for currentUserId only
            LockProvider.Wait(currentUserId);
        
            try
            {
                // access objects, execute statements, etc.
                result = DoStuff();
            }
            finally
            {
                // release the lock
                LockProvider.Release(currentUserId);
            }
            return result;
        }
    }

That's about it.
    
## Additional Resources
* [ASP.NET Core C# - How to lock an async method according to custom IDs](https://www.ryadel.com/en/asp-net-core-lock-threads-async-custom-ids-lockprovider/), a post that introduces LockProvider and explains how it works.
* [Async, Await, Lock, Wait and SynchronizationContext in ASP.NET Core](https://www.ryadel.com/en/async-await-lock-wait-synchronizationcontext-asp-net-core/), a general overview of the ASP.NET Core Task Asynchronous Programming model (TAP) and some basic concepts to asynchronous programming.
