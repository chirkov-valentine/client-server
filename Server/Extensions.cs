using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public static class Extensions
    {
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
