using System;
using System.Collections.Generic;

namespace CIRCBot
{
    /// <summary>
    /// Extensions methods for Dictionaries.
    /// </summary>
    static class DictionaryExtensions
    {

        #region General Dictionary extensions

        /// <summary>
        /// Add multiple Keys with the same Value to the dictionary.
        /// </summary>
        /// <typeparam name="T">Type of the Key</typeparam>
        /// <typeparam name="T2">Type of the Value</typeparam>
        /// <param name="dict"></param>
        /// <param name="value">Value for the dictionary entries</param>
        /// <param name="keys">Array of Keys for the dictionary entries</param>
        public static void AddKeys<T, T2>(this Dictionary<T2, T> dict, T value, params T2[] keys)
        {
            foreach (T2 key in keys)
            {
                dict.Add(key, @value);
            }
        }

        #endregion General Dictionary extensions

        #region CommandAction Dictionary extensions

        //#region Admin

        ///// <summary>
        ///// Add a new Command-Action pair to the dictionary without a description. Method invocation requires caller to have admin privileges.
        ///// </summary>
        ///// <param name="cmdDict"></param>
        ///// <param name="key"></param>
        ///// <param name="invocation"></param>
        //public static void Admin(this Dictionary<string, CommandAction> cmdDict, string key, Action invocation)
        //{
        //    cmdDict.Admin(Constants.NO_DESCRIPTION, key, invocation);
        //}

        ///// <summary>
        ///// Add a new Command-Action pair to the dictionary without a description. Method invocation requires caller to have admin privileges.
        ///// </summary>
        ///// <param name="cmdDict"></param>
        ///// <param name="key"></param>
        ///// <param name="invocation"></param>
        //public static void Admin(this Dictionary<string, CommandAction> cmdDict, Commands.Valid key, Action invocation)
        //{
        //    cmdDict.Admin(Constants.NO_DESCRIPTION, key.ToString(), invocation);
        //}

        ///// <summary>
        ///// Add a new Command-Action pair to the dictionary with a description. Method invocation requires caller to have admin privileges.
        ///// </summary>
        ///// <param name="cmdDict"></param>
        ///// <param name="desc"></param>
        ///// <param name="key"></param>
        ///// <param name="invocation"></param>
        //public static void Admin(this Dictionary<string, CommandAction> cmdDict, string desc, string key, Action invocation)
        //{
        //    cmdDict.Add(key, new CommandAction(invocation, desc, true));
        //}

        ///// <summary>
        ///// Add a new Command-Action pair to the dictionary with a description. Method invocation requires caller to have admin privileges.
        ///// </summary>
        ///// <param name="cmdDict"></param>
        ///// <param name="desc"></param>
        ///// <param name="key"></param>
        ///// <param name="invocation"></param>
        //public static void Admin(this Dictionary<string, CommandAction> cmdDict, string desc, Commands.Valid key, Action invocation)
        //{
        //    cmdDict.Add(key.ToString(), new CommandAction(invocation, desc, true));
        //}

        //#endregion Admin

        //#region Add

        ///// <summary>
        ///// Add a new Command-Action pair to the dictionary without a description. Method invocation does not require admin privileges.
        ///// </summary>
        ///// <param name="cmdDict"></param>
        ///// <param name="key"></param>
        ///// <param name="invocation"></param>
        //public static void Add(this Dictionary<string, CommandAction> cmdDict, string key, Action invocation)
        //{
        //    cmdDict.Add(Constants.NO_DESCRIPTION, key, invocation);
        //}
        ///// <summary>
        ///// Add a new Command-Action pair to the dictionary without a description. Method invocation does not require admin privileges.
        ///// </summary>
        ///// <param name="cmdDict"></param>
        ///// <param name="key"></param>
        ///// <param name="invocation"></param>
        //public static void Add(this Dictionary<string, CommandAction> cmdDict, Commands.Valid key, Action invocation)
        //{
        //    cmdDict.Add(Constants.NO_DESCRIPTION, key.ToString(), invocation);
        //}

        ///// <summary>
        ///// Add a new Command-Action pair to the dictionary with a description. Method invocation does not require admin privileges.
        ///// </summary>
        ///// <param name="cmdDict"></param>
        ///// <param name="desc"></param>
        ///// <param name="key"></param>
        ///// <param name="invocation"></param>
        //public static void Add(this Dictionary<string, CommandAction> cmdDict, string desc, string key, Action invocation)
        //{
        //    cmdDict.Add(key, new CommandAction(invocation, desc, false));
        //}

        ///// <summary>
        ///// Add a new Command-Action pair to the dictionary with a description. Method invocation does not require admin privileges.
        ///// </summary>
        ///// <param name="cmdDict"></param>
        ///// <param name="desc"></param>
        ///// <param name="key"></param>
        ///// <param name="invocation"></param>
        //public static void Add(this Dictionary<string, CommandAction> cmdDict, string desc, Commands.Valid key, Action invocation)
        //{
        //    cmdDict.Add(key.ToString(), new CommandAction(invocation, desc, false));
        //}

        //#endregion Add

        //#region NonLoggable

        ///// <summary>
        ///// Add a new Command-Action pair to the dictionary that will not be neither loggable, nor contain a description.
        ///// </summary>
        ///// <param name="cmdDict"></param>
        ///// <param name="key"></param>
        ///// <param name="invocation"></param>
        //public static void NonLoggable(this Dictionary<string, CommandAction> cmdDict, string key, Action invocation)
        //{
        //    cmdDict.Add(key, new CommandAction(invocation, false));
        //}
        ///// <summary>
        ///// Add a new Command-Action pair to the dictionary that will not be neither loggable, nor contain a description.
        ///// </summary>
        ///// <param name="cmdDict"></param>
        ///// <param name="key"></param>
        ///// <param name="invocation"></param>
        //public static void NonLoggable(this Dictionary<string, CommandAction> cmdDict, Commands.Valid key, Action invocation)
        //{
        //    cmdDict.Add(key.ToString(), new CommandAction(invocation, false));
        //}

        //#endregion NonLoggable

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

        #endregion CommandAction Dictionary extensions

    }
}
