using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Tests.Utilities
{
    public class TaskUtilities
    {
        public static async Task<T[]> WhenAllOrTimeout<T>(TimeSpan timeout, params Task<T>[] tasks)
        {
            var resultsTask = Task.WhenAll(tasks);
            var delayTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(resultsTask, delayTask);
            if (completedTask == delayTask)
                throw new TimeoutException();
            return await resultsTask;
        }
    }
}
