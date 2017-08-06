using System.Collections.Generic;

namespace CIRCBot.Comparators
{
    class ConsonantComparator : BaseComparator
    {
        /// <summary>
        /// Compare the consonant structure of the first and second commands
        /// </summary>
        /// <param name="command">Command from channel</param>
        /// <param name="baseCommand">Key from executor</param>
        /// <returns>True if consonant structures match</returns>
        public bool Compare(string command, string baseCommand)
        {
            // Must start with same letter
            if (command[0] == baseCommand[0])
            {
                // Get breakdown of base word
                var baseCBD = getConsonantBreakdown(baseCommand);

                // Get last character of base word
                var lastChar = baseCBD[baseCBD.Length - 1];

                // Word has to contain first and last character
                if (command.Contains(lastChar) && command.Contains(baseCBD[0]))
                {
                    // Grab part of word between the first instance of the first letter and the last instance of the last letter
                    command = command.SubstringIndex(command.IndexOf(baseCBD[0]), command.LastIndexOf(lastChar));

                    // Get breakdown of command
                    var commandCBD = getConsonantBreakdown(command);

                    // breakdowns must be same length
                    if (commandCBD.Length == baseCBD.Length)
                    {
                        for (int i = 0; i < commandCBD.Length; i++)
                        {
                            // Compare letters
                            if (!letterCompare(commandCBD[i], baseCBD[i]))
                            {
                                return false;
                            }
                        }
                        return true;
                    }

                }

            }
            return false;
        }

        /// <summary>
        /// Checks if two letters should be considered the same. When not exactly the same, the check is done with the "getAlternateLetter".
        /// </summary>
        /// <param name="letter1"></param>
        /// <param name="letter2"></param>
        /// <returns></returns>
        private bool letterCompare(string letter1, string letter2)
        {
            if (letter1 == letter2)
            {
                return true;
            }

            if (getAlternateLetter(letter1) == letter2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a "synonym" for the letter
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        private string getAlternateLetter(string letter)
        {
            switch (letter)
            {
                case "t":
                    return "d";

                case "d":
                    return "t";

                case "c":
                    return "k";

                case "k":
                    return "c";

                case "p":
                    return "b";

                case "b":
                    return "p";

                default:
                    return letter;
            }
        }

        /// <summary>
        /// Break word down to an array of its consonants
        /// </summary>
        /// <param name="word">Word to break down</param>
        /// <returns>Array of the word's consonants</returns>
        private string[] getConsonantBreakdown(string word)
        {
            // Must be all lower case for comparison
            word = word.ToLower();

            // Contains the consonants of the word in the correct order
            List<string> consonantBreakdown = new List<string>();

            foreach(char c in word.ToCharArray())
            {
                if(IsConsonant(c.ToString()))
                {
                    consonantBreakdown.Add(c.ToString());
                }
            }

            return consonantBreakdown.ToArray();
        }

    }
}
