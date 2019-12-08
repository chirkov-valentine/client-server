using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// Методы разширения
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Метод расширения для отмены задачи
        /// </summary>
        /// <typeparam name="T">Тип задачи</typeparam>
        /// <param name="task">Задача</param>
        /// <param name="cancellationToken">Токен отмены задачи</param>
        /// <returns>Задачу, к кторой применен</returns>
        public static async Task<T> WithWaitCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {

            var tcs = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(
                        s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);

            return await task;
        }
    }
}
