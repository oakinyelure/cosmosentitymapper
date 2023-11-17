using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Entity.Mapper.Extensions.Tests
{
    [TestFixture()]
    public class TaskExtensionsTests
    {
        [Test()]
        public async Task ExecuteInparallelAsync_Can_Execute_All_Tasks_In_Without_Batch()
        {
            var actual = new ConcurrentBag<int>();
            IEnumerable<Task> actualTasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                actualTasks = actualTasks.Append(Task.Run(async () =>
                {
                    await Task.Delay(1);
                    actual.Add(i);
                }));
            }
            await actualTasks.ExecuteInParallelAsync();
            Assert.That(actual.Count, Is.EqualTo(100));
        }

        [Test()]
        public async Task ExecuteInparallelAsync_Can_Execute_All_Tasks_In_With_Batch()
        {
            var actual = new ConcurrentBag<int>();
            IEnumerable<Task> actualTasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                actualTasks = actualTasks.Append(Task.Run(async () =>
                {
                    await Task.Delay(1);
                    actual.Add(i);
                }));
            }
            await actualTasks.ExecuteInParallelAsync(10);
            Assert.That(actual.Count, Is.EqualTo(100));
        }

        [Test()]
        public async Task ExecuteInParallelAsync_Can_Execute_Delegates_In_Parallel_Without_Batch()
        {
            var actual = new ConcurrentBag<int>();
            IEnumerable<Func<Task>> actualTasks = new List<Func<Task>>();
            for (int i = 0; i < 100; i++)
            {
                Task func() => Task.Run(async () =>
                {
                    await Task.Delay(1);
                    actual.Add(i);
                });
                actualTasks = actualTasks.Append(func);
            }
            await actualTasks.ExecuteInParallelAsync();
            Assert.That(actual.Count, Is.EqualTo(100));
        }

        [Test()]
        public async Task ExecuteInParallelAsync_Can_Execute_Delegates_In_Parallel_With_Batch()
        {
            var actual = new ConcurrentBag<int>();
            IEnumerable<Func<Task>> actualTasks = new List<Func<Task>>();
            for (int i = 0; i < 100; i++)
            {
                Task func() => Task.Run(async () =>
                {
                    await Task.Delay(1);
                    actual.Add(i);
                });
                actualTasks = actualTasks.Append(func);
            }
            await actualTasks.ExecuteInParallelAsync(10);
            Assert.That(actual.Count, Is.EqualTo(100));
        }
    }
}