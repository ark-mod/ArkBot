using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Threading
{
    public class SingleRunningTaskCancelPrevious
    {
        private object _updateContextTaskLock = new object();
        private Task _updateContextTaskLastIn;
        private Task _updateContextTask;
        private CancellationTokenSource _updateContextTaskCts;

        // debug vars
        private int _nextNumber = 1;
        private static DateTime _started = DateTime.Now;

        public async Task<bool> Execute(Func<CancellationToken, Task> action)
        {
            var number = _nextNumber++;
            Task waiting = null;
            lock (_updateContextTaskLock)
            {
                if (_updateContextTask != null)
                {
                    _updateContextTaskCts.Cancel();
                    waiting = _updateContextTaskLastIn = Task.WhenAll(_updateContextTask);
                }
            }

            if (waiting != null)
            {
                try
                {
                    await waiting;
                }
                catch (AggregateException ae)
                {
                    ae.Handle(ex => ex is OperationCanceledException);
                }
                catch (OperationCanceledException) { }
            }

            lock (_updateContextTaskLock)
            {
                if (waiting != _updateContextTaskLastIn)
                {
                    DebugWriteStatus("Skipped", number);
                    return false;
                }
            }

            _updateContextTaskCts = new CancellationTokenSource();
            var task = _updateContextTask = Task.Factory.StartNew(async () => await action(_updateContextTaskCts.Token), 
                _updateContextTaskCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

            try
            {
                DebugWriteStatus("Start", number);
                await task;
                DebugWriteStatus("End", number, task.IsCanceled ? "cancelled" : null);
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (!(ex is OperationCanceledException)) return false;
                    DebugWriteStatus("End", number, "cancelled");
                    return true;
                });
            }
            catch (OperationCanceledException)
            {
                DebugWriteStatus("End", number, "cancelled");
            }

            return !(task.IsCanceled || task.IsFaulted);
        }

        private void DebugWriteStatus(string at, int n, string extra = null)
        {
            Debug.WriteLine($"{(DateTime.Now - _started).TotalSeconds:N1}s {at} {n}{(extra != null ? $" ({extra})" : "")}");
        }
    }
}
