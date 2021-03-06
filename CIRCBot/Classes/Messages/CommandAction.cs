﻿using System;
using CIRCBot.XTimers;

namespace CIRCBot
{
    /// <summary>
    /// Command invoker.
    /// </summary>
    public class CommandAction
    {
        /// <summary>
        /// Command the Cmd invokes.
        /// </summary>
        private Action target { get; }

        /// <summary>
        /// Does the command require admin rights.
        /// </summary>
        private bool requiresAdminRights { get; set; }

        /// <summary>
        /// Description of what this Command-Action pair does.
        /// </summary>
        private string description { get; set; }

        /// <summary>
        /// This command's key for the timer lock
        /// </summary>
        private string LockKey { get; set; }

        /// <summary>
        /// The amount of time this command is locked
        /// </summary>
        private int LockTimeInSeconds { get; set; }

        /// <summary>
        /// Sets a timer lock for this command with a manually specified lock key
        /// </summary>
        /// <param name="lockKey">Timer lock key</param>
        /// <param name="seconds">Amount of seconds the command is locked</param>
        public void TimerLocked(string lockKey, int seconds)
        {
            LockKey = lockKey;
            LockTimeInSeconds = seconds;
        }

        /// <summary>
        /// Sets a timer lock for this command with an autogenerated lock key
        /// </summary>
        /// <param name="seconds">Amount of seconds the command is locked</param>
        public void TimerLocked(int seconds)
        {
            LockKey = target.Target.GetType().Name + "-" + target.Method.Name;
            LockTimeInSeconds = seconds;
        }

        /// <summary>
        /// Invoke this command.
        /// </summary>
        /// <param name="callerIsAdmin">Is the caller an admin.</param>
        public void Run(bool callerIsAdmin)
        {
            if(LockKey != null)
            {
                if (TimerLocks.Locked(LockKey, LockTimeInSeconds)) return;
            }
            if(requiresAdminRights && !callerIsAdmin)
            {
                return;
            }
            target.Invoke();
        }

        /// <summary>
        /// Command description. Does not exist if command was created as a Non-Loggable command.
        /// </summary>
        public string Description
        {
            get
            {
                if(Loggable)
                {
                    return description;
                }
                return String.Empty;
            }
        }

        /// <summary>
        /// Will the command be logged by an Executor's command logger.
        /// </summary>
        public bool Loggable { get; }
        
        /// <summary>
        /// Create a new Command-Action pair with a description. 
        /// If requireAdmin parameter not given, command will not require admin privileges to run.
        /// </summary>
        /// <param name="action">The action to invoke by the Run method</param>
        /// <param name="desc">Description of this Command-Action pair</param>
        /// <param name="requireAdmin">Whether or not invoking this CommandAction's action requires the invoker to have admin privileges</param>
        public CommandAction(Action action, string desc, bool requireAdmin = false)
        {
            requiresAdminRights = requireAdmin;
            target = action;
            description = desc;
            Loggable = true;
        }

        /// <summary>
        /// Create a new Command-Action pair that has no description. Will be marked as a Non-Loggable command.
        /// </summary>
        /// <param name="action">The action to invoke by the Run method</param>
        /// <param name="requireAdmin">Whether or not invoking this CommandAction's action requires the invoker to have admin privileges</param>
        public CommandAction(Action action, bool requireAdmin = false)
        {
            requiresAdminRights = requireAdmin;
            target = action;
            Loggable = false;
        }

        /// <summary>
        /// Check if this command's target is equal to the given action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool Is(Action action)
        {
            return this.target == action;
        }
    }
}
