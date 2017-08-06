namespace CIRCBot.Comparators
{
    public class BaseComparator
    {

        #region Vowel and consonant checks

        /// <summary>
        /// String containing valid vowels.
        /// </summary>
        protected string Vowels = TXT.Vowels;

        /// <summary>
        /// Checks if the first character of the parameter is a vowel.
        /// </summary>
        /// <param name="character">Letter to check</param>
        /// <returns>True if contained in the Vowels string, false if not or empty</returns>
        protected bool IsVowel(string character)
        {
            if (character.Length < 1)
            {
                return false;
            }
            return Vowels.Contains(character.Substring(0, 1).ToUpper());
        }

        /// <summary>
        /// Checks if the first character of the parameter is a consonant
        /// </summary>
        /// <param name="character">Letter to check</param>
        /// <returns>True if a letter but not contained in the vowels string</returns>
        protected bool IsConsonant(string character)
        {
            if (character.Length > 0)
            {
                if (char.IsLetter(character[0]))
                {
                    if (!IsVowel(character[0].ToString()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion Vowel and consonant checks

    }
}
