using System;
using System.Collections.Generic;
using System.Threading;

namespace CoP.Enterprise
{
    public delegate void SyncDelegatesInParallelDelegate<in T>(T item);

    public static class ParallelLinqExtensions
    {
        public static void SyncDelegatesInParallel<T>(
            this IList<T> list,
            SyncDelegatesInParallelDelegate<T> action)
        {
            var foundCriticalException = false;
            Exception exception = null;
            // -------------------------
            var itemCnt = list.Count;
            if (itemCnt == 0) return;
            var signal = new ManualResetEvent(false);
            foreach (var item in list)
            {
                // Temp copy of item for modified closure
                var localItem = item;

                // Temp copy of item for closure
                ThreadPool.QueueUserWorkItem(
                     depTx =>
                     {
                         try { if (!foundCriticalException) action(localItem); }
                         catch (CoPException sX) { exception = sX; }
                         catch (Exception gX)
                         {
                             exception = gX;
                             foundCriticalException = true;
                         }
                         finally { if (Interlocked.Decrement(ref itemCnt) == 0) signal.Set(); }
                     }, null);
            }
            signal.WaitOne();
            if (exception != null) throw exception;
        }
    }
}
