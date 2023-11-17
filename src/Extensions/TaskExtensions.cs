using Cosmos.Entity.Mapper.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Entity.Mapper.Extensions
{
    /// <summary>
    /// This is an internal API that supports the Cosmos Entity Mapper core infrastructure and not subject to the same compatibility
    /// standards as public APIs. The use in production environment is highly discouraged. Changes or updates can occur without notice
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// This is an internal API that supports the Cosmos Entity Mapper core infrastructure and not subject to the same compatibility
        /// standards as public APIs. The use in production environment is highly discouraged. Changes or updates can occur without notice
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public static async Task ExecuteInParallelAsync(this IEnumerable<Task> tasks, int? batchSize = 300)
        {
            EntityValidationBase.NotEmptyOrThrow(tasks);
            using SemaphoreSlim semaphore = new((int)batchSize);
            await Task.WhenAll(tasks.Select(async task =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await task;
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        /// <summary>
        /// This is an internal API that supports the Cosmos Entity Mapper core infrastructure and not subject to the same compatibility
        /// standards as public APIs. The use in production environment is highly discouraged. Changes or updates can occur without notice
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public static async Task ExecuteInParallelAsync(this IEnumerable<Func<Task>> tasks, int? batchSize = 300)
        {
            EntityValidationBase.NotEmptyOrThrow(tasks);
            using SemaphoreSlim semaphore = new((int)batchSize);
            await Task.WhenAll(tasks.Select(async function =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await function.Invoke();
                }
                finally
                {
                    semaphore.Release();
                }
                return function;
            }));
        }
    }
}
