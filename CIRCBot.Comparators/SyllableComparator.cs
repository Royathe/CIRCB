using System;
using System.Collections.Generic;

namespace CIRCBot.Comparators
{
    public class SyllableComparator : BaseComparator
    {

        private ConsonantComparator ConComparator { get; }

        private SyllableReplacer SylReplacer { get; } 

        /// <summary>
        /// Current command broken down to syllables.
        /// </summary>
        private string[] CommandSyllables { get; set; }

        private string Command { get; set; }

        /// <summary>
        /// Should a syllable replacer be run?
        /// </summary>
        private bool RunSyllableReplacer { get; set; }

        public SyllableComparator()
        {
            ConComparator = new ConsonantComparator();
            SylReplacer = new SyllableReplacer();
        }

        /// <summary>
        /// Break command down to syllables. Check if the replacer should be run
        /// </summary>
        /// <param name="command">Command to break down</param>
        public void CommandToSyllables(string command)
        {
            if(Command == null)
            {
                Command = command;
                RunSyllableReplacer = SylReplacer.Run(command);
                CommandSyllables = getSyllables(command);
            }
        }

        /// <summary>
        /// Reset comparator's command
        /// </summary>
        public void Reset()
        {
            Command = null;
        }

        /// <summary>
        /// Try to match the current command to the key.
        /// </summary>
        /// <param name="key">Key to match check for</param>
        /// <returns>True if succesfully matched</returns>
        public bool Match(string key)
        {
            // Get the syllables of the base command
            string[] baseSyls = getSyllables(key);

            if(RunSyllableReplacer)
            {
                var commandReps = SylReplacer.GetReplacements(CommandSyllables, "gay");
                var baseReps = SylReplacer.GetReplacements(baseSyls, "gay");

                for(int i = 0; i < commandReps.Length; i++)
                {
                    for(int j = 0; j < baseReps.Length; j++)
                    {
                        //Console.WriteLine("Comparing: " + commandReps[i] + " | " + baseReps[j]);

                        // Check if syllable comparison is enough
                        if(commandReps[i] == baseReps[j])
                        {
                            return true;
                        }

                        // Do a consonant based comparison
                        if(ConComparator.Compare(commandReps[i], baseReps[j]))
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                //Console.WriteLine("Comparing: " + Command + " | " + key);

                // Do a consonant based comparison
                if (ConComparator.Compare(Command, key))
                {
                    return true;
                }
            }

            return false;
        }

        #region Private methods

        /// <summary>
        /// Breaks a word down to an array of its syllables.
        /// </summary>
        /// <param name="command">Command to break down</param>
        /// <returns>array of syllables</returns>
        private string[] getSyllables(string command)
        {
            string[] word = wordConverter(command);
            List<string> syllables = new List<string>();

            int firstLetterIndex = 0;   // First letter of syllable
            int lastLetterIndex = 1;    // Last letter of syllable
            int lastVowel = -1;         // Reset to -1 after each syllable addition
            int consonantsInARow = 0;   // Number of consecutive consonants

            for (int i = 0; i < word.Length; i++)
            {
                // If current letter is a consonant
                if (IsConsonant(word[i]) && lastVowel != -1)
                {
                    consonantsInARow++;

                    // If next letter is a vowel or already two consonants in a row
                    if (IsVowel(word.NextIndex(i)) || consonantsInARow > 2)
                    {
                        // Break it apart here
                        lastLetterIndex = i - 1;

                        syllables.Add(word.Substring(firstLetterIndex, lastLetterIndex));

                        firstLetterIndex = i;
                        consonantsInARow = 0;
                        lastVowel = -1;
                    }
                }
                else if (IsVowel(word[i]) && i != 0)
                {
                    lastVowel = i;
                }
            }
            syllables.Add(word.Substring(firstLetterIndex));

            return syllables.ToArray();
        }

        /// <summary>
        /// Convert word to an array. 
        /// Treat certain combinations of letters as a single letter.
        /// </summary>
        /// <param name="word">Word to convert</param>
        /// <returns>Word as an array with certain letter combinations merged to a single index</returns>
        private string[] wordConverter(string word)
        {
            List<string> wordArray = new List<string>();

            for (int i = 0; i < word.Length; i++)
            {
                // Combine current and next letter.
                string combination = "" + word[i] + word.NextLetter(i);

                // If combination matches a case, add that combination instead of the current letter, and skip the next letter.
                switch (combination)
                {
                    case "ch":
                        wordArray.Add(combination);
                        i++;
                        break;

                    default:
                        wordArray.Add(word[i].ToString());
                        break;
                }
            }

            return wordArray.ToArray();
        }

        #endregion Private methods

    }
}
