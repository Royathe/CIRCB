using System.Timers;

namespace CIRCBot.XTimers
{
    /// <summary>
    /// Base class for action -timers
    /// </summary>
    public class TimerBase
    {
        /// <summary>
        /// The actual timer
        /// </summary>
        protected Timer timer { get; set; }

        /// <summary>
        /// Basic setup for the timer
        /// </summary>
        /// <param name="seconds"></param>
        protected void SetupTimer(float seconds)
        {
            if (timer != null)
            {
                Stop();
            }

            timer = new Timer(seconds * 1000);
            timer.AutoReset = false;
            timer.Enabled = true;
        }

        /// <summary>
        /// Stop the timer and release resources
        /// </summary>
        public void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }
    }
}
