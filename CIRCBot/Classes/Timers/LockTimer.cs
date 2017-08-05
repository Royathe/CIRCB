using System.Timers;

namespace CIRCBot
{
    /// <summary>
    /// A TimerLocks timer.
    /// Used to lock commands for certain periods of time.
    /// </summary>
    class LockTimer : TimerBase
    {
        /// <summary>
        /// Unique key for this LockTimer.
        /// </summary>
        private string LockKey { get; set; }

        /// <summary>
        /// Set up a new LockTimer.
        /// </summary>
        /// <param name="seconds">How many seconds the lock is active</param>
        /// <param name="key">Unique key for this LockTimer</param>
        public void Set(float seconds, string key)
        {
            LockKey = key;

            SetupTimer(seconds);

            timer.Elapsed += unlock;
        }

        /// <summary>
        /// Removes the LockTimer from the TimerLocks dictionary
        /// </summary>
        private void unlock(object source, ElapsedEventArgs e)
        {
            TimerLocks.Stop(LockKey);
        }
    }

}
