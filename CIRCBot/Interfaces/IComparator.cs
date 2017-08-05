namespace CIRCBot.Execution.Comparators
{
    /// <summary>
    /// Text comparator.
    /// </summary>
    interface IComparator
    {
        /// <summary>
        /// Try to deduce a given command to a valid command. Returns an Invalid command if deduction fails.
        /// </summary>
        /// <param name="command">Command to deduce</param>
        /// <returns>Deduced command or Invalid</returns>
        string Compare(string command);
    }
}
