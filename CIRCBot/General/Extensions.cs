using System;
using System.Collections.Generic;

namespace CIRCBot
{
    public static class Extensions
    {

        #region CommandAction related extensions

        /// <summary>
        /// Get a copy of this dictionary with just the commands in it that are marked as "Loggable".
        /// </summary>
        /// <param name="cmdDict"></param>
        /// <returns>Copy of this dictionary with only loggable commands</returns>
        public static Dictionary<string, CommandAction> Loggables(this Dictionary<string, CommandAction> cmdDict)
        {
            Dictionary<string, CommandAction> loggableEntries = new Dictionary<string, CommandAction>();
            foreach (KeyValuePair<string, CommandAction> entry in cmdDict)
            {
                if (entry.Value.Loggable)
                {
                    loggableEntries.Add(entry.Key, entry.Value);
                }
            }
            return loggableEntries;
        }

        /// <summary>
        /// Find the CommandAction that matches the given parameter, and execute it with the given admin privileges.
        /// </summary>
        /// <param name="cmdDict"></param>
        /// <param name="currentParam">Key to find</param>
        /// <param name="isAdmin">Is caller an admin or not</param>
        /// <returns>False if no command found with given key. True if found.</returns>
        public static bool Execute(this Dictionary<string, CommandAction> cmdDict, string currentParam, bool isAdmin)
        {
            currentParam = currentParam.ToLower();
            if (cmdDict.ContainsKey(currentParam))
            {
                cmdDict[currentParam].Run(isAdmin);
                return true;
            }
            return false;
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

        #endregion CommandAction related extensions
    }
}
