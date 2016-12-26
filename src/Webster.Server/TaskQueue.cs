using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Webster.Server
{
    public class TaskQueue
    {
        private readonly SemaphoreSlim _semaphore;

        public TaskQueue()
        {
            _semaphore = new SemaphoreSlim(1);
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> task)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await task();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task Enqueue(Func<Task> task)
        {
            await _semaphore.WaitAsync();

            try
            {
                await task();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
