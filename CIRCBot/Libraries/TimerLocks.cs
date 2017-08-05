using System.Collections.Generic;

namespace CIRCBot
{
    /// <summary>
    /// Library of currently active temporary locks.
    /// Each lock has a unique key. 
    /// </summary>
    public static class TimerLocks
    {
        /// <summary>
        /// Get the lock dictionary.
        /// </summary>
        private static Dictionary<string, LockTimer> Timers
        {
            get
            {
                return TimerLockList.Timers;
            }
        }

        /// <summary>
        /// Stop and remove the timer matching the given key.
        /// </summary>
        /// <param name="key">Unique key of the timer lock</param>
        public static void Stop(string key)
        {
            if (Timers.ContainsKey(key))
            {
                Timers[key].Stop();
                Timers.Remove(key);
            }
        }

        /// <summary>
        /// Is a lock currently active for the given key?
        /// If is, returns true. If not, a new lock timer is added with the given key, and method returns false
        /// </summary>
        /// <param name="lockKey">Unique key of the timer lock</param>
        /// <param name="seconds">Amount of seconds the timer lock should stay active, if a new one is created</param>
        /// <returns>True if locked, false if unlocked</returns>
        public static bool Locked(string lockKey, int seconds)
        {
            if (IsLocked(lockKey))
            {
                return true;
            }
            else
            {
                Set(lockKey, seconds);

                return false;
            }
        }

        /// <summary>
        /// Only checks if a given key is locked
        /// </summary>
        /// <param name="lockKey">Key to check</param>
        /// <returns>True if locked, false if not</returns>
        public static bool IsLocked(string lockKey)
        {
            if (Timers.ContainsKey(lockKey))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Directly sets/resets a lock timer
        /// </summary>
        /// <param name="lockKey">Unique key of the timer lock</param>
        /// <param name="seconds">Amount of seconds the timer lock should stay active</param>
        public static void Set(string lockKey, int seconds)
        {
            if (IsLocked(lockKey))
            {
                Stop(lockKey);
            }

            var timer = new LockTimer();
            timer.Set(seconds, lockKey);
            Timers.Add(
                lockKey,
                timer
                );
        }

        #region Repository

        /// <summary>
        /// Container of the timer locks.
        /// </summary>
        private static class TimerLockList
        {
            /// <summary>
            /// Raw dictionary.
            /// </summary>
            private static Dictionary<string, LockTimer> _timers { get; set; }

            /// <summary>
            /// Dictionary getter.
            /// </summary>
            internal static Dictionary<string, LockTimer> Timers
            {
                get
                {
                    if(_timers == null)
                    {
                        _timers = new Dictionary<string, LockTimer>();
                    }
                    return _timers;
                }
            }
        }

        #endregion Repository

    }
}
