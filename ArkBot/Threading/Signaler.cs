//Based on code from http://stackoverflow.com/a/18611391

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Threading
{
    /// <summary>
    /// Basic signal synchronization (wait for next pulse, no pulse state)
    /// </summary>
    public sealed class Signaller<T>
    {
        private readonly object _lock = new object();
        private T _state;

        /// <summary>
        /// Signal all now waiting threads
        /// </summary>
        public void PulseAll(T state = default(T))
        {
            lock (_lock)
            {
                _state = state;
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Signal one waiting thread
        /// </summary>
        public void Pulse(T state = default(T))
        {
            lock (_lock)
            {
                _state = state;
                Monitor.Pulse(_lock);
            }
        }

        /// <summary>
        /// Wait until next signal is sent
        /// </summary>
        public void Wait()
        {
            T state;
            Wait(Timeout.Infinite, out state);
        }

        /// <summary>
        /// Wait until next signal is sent
        /// </summary>
        public void Wait(out T state)
        {
            Wait(Timeout.Infinite, out state);
        }

        /// <summary>
        /// Wait until next signal is sent (with a timeout)
        /// </summary>
        /// <returns>True if signal was received; False if timeout elapsed</returns>
        public bool Wait(int timeoutMilliseconds)
        {
            T state;
            return Wait(TimeSpan.FromMilliseconds(timeoutMilliseconds), out state);
        }

        /// <summary>
        /// Wait until next signal is sent (with a timeout)
        /// </summary>
        /// <returns>True if signal was received; False if timeout elapsed</returns>
        public bool Wait(int timeoutMilliseconds, out T state)
        {
            return Wait(TimeSpan.FromMilliseconds(timeoutMilliseconds), out state);
        }

        /// <summary>
        /// Wait until next signal is sent (with a timeout)
        /// </summary>
        /// <returns>True if signal was received; False if timeout elapsed</returns>
        public bool Wait(TimeSpan timeout)
        {
            T state;
            return Wait(timeout, out state);
        }

        /// <summary>
        /// Wait until next signal is sent (with a timeout)
        /// </summary>
        /// <returns>True if signal was received; False if timeout elapsed</returns>
        public bool Wait(TimeSpan timeout, out T state)
        {
            lock (_lock)
            {
                var result = Monitor.Wait(_lock, timeout);
                state = _state;
                return result;
            }
        }
    }

    /// <summary>
    /// Basic signal synchronization (wait for next pulse, no pulse state)
    /// </summary>
    public sealed class Signaller
    {
        private readonly object _lock = new object();

        /// <summary>
        /// Signal all now waiting threads
        /// </summary>
        public void PulseAll()
        {
            lock (_lock)
            {
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Signal one waiting thread
        /// </summary>
        public void Pulse()
        {
            lock (_lock)
            {
                Monitor.Pulse(_lock);
            }
        }

        /// <summary>
        /// Wait until next signal is sent
        /// </summary>
        public void Wait()
        {
            Wait(Timeout.Infinite);
        }

        /// <summary>
        /// Wait until next signal is sent (with a timeout)
        /// </summary>
        /// <returns>True if signal was received; False if timeout elapsed</returns>
        public bool Wait(int timeoutMilliseconds)
        {
            lock (_lock)
            {
                return Monitor.Wait(_lock, timeoutMilliseconds);
            }
        }

        /// <summary>
        /// Wait until next signal is sent (with a timeout)
        /// </summary>
        /// <returns>True if signal was received; False if timeout elapsed</returns>
        public bool Wait(TimeSpan timeout)
        {
            lock (_lock)
            {
                return Monitor.Wait(_lock, timeout);
            }
        }
    }
}