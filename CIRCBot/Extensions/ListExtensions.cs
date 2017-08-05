using System;
using System.Collections.Generic;

namespace CIRCBot
{
    public static class ListExtensions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while(n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        
        /// <summary>
        /// Gives all the command actions in the array a timer lock
        /// </summary>
        /// <param name="cas"></param>
        /// <param name="lockKey"></param>
        /// <param name="seconds"></param>
        public static void TimerLocked(this CommandAction[] cas, string lockKey, int seconds)
        {
            foreach (CommandAction ca in cas)
            {
                ca.TimerLocked(lockKey, seconds);
            }
        }
        /// <summary>
        /// Gives all the command actions in the array a timer lock
        /// </summary>
        /// <param name="cas"></param>
        /// <param name="lockKey"></param>
        /// <param name="seconds"></param>
        public static void TimerLocked(this CommandAction[] cas, int seconds)
        {
            foreach (CommandAction ca in cas)
            {
                ca.TimerLocked(seconds);
            }
        }
    }
}
