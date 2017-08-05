using System.Collections.Generic;

namespace CIRCBot.Execution.Comparators
{
    class SyllableReplacer : BaseComparator
    {
        /// <summary>
        /// Check if the word should be run with a syllable replacer.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool Run(string command)
        {
            if(Gay(command))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns an array of words, each with a different syllable replaced
        /// </summary>
        /// <param name="syllables">Syllables of the word</param>
        /// <param name="sylReplacement">Replacement syllable</param>
        /// <returns>Array of words, each with a different syllable replaced</returns>
        public string[] GetReplacements(string[] syllables, string sylReplacement)
        {
            List<string> allSyllableReplacements = new List<string>();

            for (int i = 0; i < syllables.Length; i++)
            {
                string newWord = "";
                for (int j = 0; j < syllables.Length; j++)
                {
                    if (j == i)
                    {
                        newWord = newWord + sylReplacement.ToLower();
                    }
                    else
                    {
                        newWord = newWord + syllables[j];
                    }
                }
                allSyllableReplacements.Add(newWord);
            }

            return allSyllableReplacements.ToArray();
        }

        /// <summary>
        /// Check command for anything akin to G_Y/I/IJ (Gay: Starts with G, end with Y/I/IJ, all letters between are vowels ex. geeeij)
        /// </summary>
        /// <param name="command"></param>
        /// <returns>True if found</returns>
        private bool Gay(string command)
        {
            bool isMatch = false;

            while (!isMatch && command.IndexOf("g") >= 0)
            {
                if (command.IndexOf("g") >= 0)
                {
                    // part of string after G
                    string part = command.Substring(command.IndexOf("g") + 1);

                    int letterIndex = 0;

                    // Check for instances of Y, I and IJ. If none found, exit.
                    if (part.IndexOf("y") >= 0)
                    {
                        letterIndex = part.IndexOf("y");
                    }
                    else if (part.IndexOf("ij") >= 0)
                    {
                        letterIndex = part.IndexOf("ij");
                    }
                    else if (part.IndexOf("i") >= 0)
                    {
                        letterIndex = part.IndexOf("i");
                    }
                    else
                    {
                        return false;
                    }

                    // If there weren't any character between G and Y/I, exit.
                    if (letterIndex <= 0)
                    {
                        isMatch = false;
                    }
                    else
                    {
                        // Iterate from first letter after start letter to last before ending letter
                        for (int i = 0; i < letterIndex; i++)
                        {
                            // Make sure all letters between start and end are vowels.
                            if (!IsVowel("" + part[i]))
                            {
                                isMatch = false;
                            }
                        }
                        isMatch = true;
                    }

                }
                else
                {
                    // Exit right away if no G
                    return false;
                }
                if (!isMatch)
                {
                    // Get the section of the string starting from the next G
                    command = command.Substring(command.IndexOf("g") + 1);
                }
            }
            return isMatch;
        }
    }
}
