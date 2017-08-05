using System;
using System.Timers;

namespace CIRCBot
{
    /// <summary>
    /// Action-calling  timer.
    /// Calls a given action after a set timer period.
    /// </summary>
    class SoloTimer : TimerBase
    {
        /// <summary>
        /// Target action to call after time is elapsed.
        /// </summary>
        private Action Target { get; set; }

        /// <summary>
        /// Set up a new action timer.
        /// </summary>
        public void Set(int seconds, Action target)
        {
            Set(float.Parse(seconds.ToString()), target);
        }

        /// <summary>
        /// Set up a new action timer.
        /// </summary>
        public void Set(float seconds, Action target)
        {
            SetupTimer(seconds);

            Target = target;
            timer.Elapsed += callTarget;
        }

        /// <summary>
        /// Invoke target action.
        /// </summary>
        private void callTarget(object source, ElapsedEventArgs e)
        {
            Target.Invoke();
        }
    }
}
