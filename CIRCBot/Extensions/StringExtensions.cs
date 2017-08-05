using System;

namespace CIRCBot
{
    /// <summary>
    /// Extensions methods for strings and arrays of strings.
    /// </summary>
    static class StringExtensions
    {
        
        #region String extensions

        public static int CountOf(this string str, string key)
        {
            int loopCount = 0;
            int count = 0;
            int lastIndex = 0;
            string sub = str;

            while(lastIndex >= 0 && loopCount < 100)
            {
                lastIndex = sub.IndexOf(key);
                if (lastIndex >= 0)
                {
                    count++;
                    sub = sub.Substring(lastIndex + 1);
                }
                loopCount++;
            }

            return count;
        }

        public static string SubstringMax(this string str, int startIndex, int length = 0)
        {
            if(length == 0)
            {
                length = str.Length;
            }

            if(startIndex + length > str.Length)
            {
                length = str.Length - startIndex;
            }

            if(length < 1)
            {
                return str;
            }
            return str.Substring(startIndex, length);
        }

        /// <summary>
        /// Converts an object to an SQL parameter string. Booleans converted to "1" and "0".
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="obj">Object to convert</param>
        /// <returns>Object as an SQL string</returns>
        public static string ToSQLString<T>(this T obj)
        {
            if (obj is bool)
            {
                return (bool)(object)obj ? "1" : "0";
            }
            else if(obj is DateTime)
            {
                DateTime time = (DateTime)(object)obj;
                return "'" + time.Year + "-" + time.Month + "-" + time.Day + "'";
            }
            return obj.ToString();
        }

        /// <summary>
        /// Get the next letter in the string from the given index.
        /// </summary>
        /// <param name="str">String to get the letter from</param>
        /// <param name="index">Current letter index to get the next for</param>
        /// <returns>Next letter after the given index. Empty if index greater than length</returns>
        public static string NextLetter(this string str, int index)
        {
            if((index + 1) >= str.Length)
            {
                return "";
            }
            return "" + str[index + 1];
        }

        /// <summary>
        /// Get a substring from first index to the last index.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="firstIndex"></param>
        /// <param name="lastIndex"></param>
        /// <returns></returns>
        public static string SubstringIndex(this string str, int firstIndex, int lastIndex)
        {
            int diff = lastIndex - firstIndex;

            // +1 so lastIndex letter is included
            return str.Substring(firstIndex, diff + 1);
        }

        /// <summary>
        /// Adds an i to the end of the text if it ends in a consonant.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AddI(this string text)
        {
            string lastLetter = text[text.Length - 1].ToString();
            if (!Library.Vowels.ToLower().Contains(lastLetter.ToLower()))
            {
                return text + "i";
            }
            return text;
        }

        public static string Position(this string text, int position, string insertValue)
        {
            int difference = position - text.Length;
            if(difference > 0)
            {
                for(int i = 0; i <= difference; i++)
                {
                    text += " ";
                }
            }
            return text.Insert(position, insertValue);
        }
        
        public static string ToFixed(this decimal dec, int decimalPlaces = 2)
        {
            string format = "0";

            if(decimalPlaces > 0)
            {
                format += ".";
                for(int i = 0; i < decimalPlaces; i++)
                {
                    format += "0";
                }
            }
            return dec.ToString(format);
        }

        public static string Colorize(this string text, MircColor color)
        {
            return color + text + Color.Clear;
        }
        
        public static string GetElement(this string str, string elemToLookFor)
        {
            int len = elemToLookFor.Length;

            string elemPrefix = elemToLookFor.Substring(0, elemToLookFor.IndexOf(' '));
            string elemEndPrefix = elemPrefix.Insert(1, "/");

            int startIndex = str.IndexOf(elemToLookFor) + len;

            string parsedElem = str.Substring(startIndex);

            int endIndex = parsedElem.IndexOf(elemEndPrefix);

            parsedElem = parsedElem.Substring(0, endIndex);

            return parsedElem;
        }

        public static string Between(this string text, string start, string end)
        {
            int length = start.Length;
            
            int startIndex = text.IndexOf(start) + length;

            string parsed = text.Substring(startIndex);

            parsed = parsed.Substring(0, parsed.IndexOf(end));

            return parsed;
        }

        public static string BetweenById(this string text, string start, string end)
        {
            start = "div id=\"" + start + "\">";
            return text.Between(start, end);
        }

        #endregion String extensions

        #region String array extensions

        /// <summary>
        /// Get the string from the next index after the given index.
        /// </summary>
        /// <param name="arr">String array to get the string from</param>
        /// <param name="index">Current array index to get next of</param>
        /// <returns>String after the given index. Empty string if index greater than length</returns>
        public static string NextIndex(this string[] arr, int index)
        {
            if((index + 1) >= arr.Length)
            {
                return "";
            }
            return arr[index + 1];
        }

        /// <summary>
        /// Split a string to string array with a maximum length.
        /// </summary>
        /// <param name="input">String being split.</param>
        /// <param name="splitMark">Character at which to split</param>
        /// <param name="length">Maximum length of the array</param>
        /// <returns>String split into an array.</returns>
        public static string[] SplitMax(this string input, char splitMark, int length)
        {
            string[] baseSplit = input.Split(splitMark);
            string[] limitedSplit = new string[baseSplit.Length < length ? baseSplit.Length : length];
            for(int i = 0; i < limitedSplit.Length; i++)
            {
                limitedSplit[i] = baseSplit[i];
            }
            return limitedSplit;
        }

        /// <summary>
        /// Returns a subsection of the array from the start point to the end point.
        /// </summary>
        /// <param name="baseArray">Array to subsection</param>
        /// <param name="start">Starting index of the subsection</param>
        /// <param name="end">Last index of the subsection</param>
        /// <returns>Subsection of the array</returns>
        public static string[] Subarray(this string[] baseArray, int start, int? end = null)
        {
            // If starting index is greater than the last index of the baseArray,
            // return an empty string array.
            if(start > baseArray.Length - 1)
            {
                return new string[0];
            }

            // Create new array for the subsection.
            // If the 'end' parameter wasn't given, 
            // or if it is greater than the last index of the base array,
            // the new array is only trimmed from the start by the start index.
            // If given and less than the last index, the 'end' value is used to trim the end of the array.
            string[] subArray = new string[
                (!end.HasValue || end >= baseArray.Length) ? baseArray.Length - start : ((int)end+1) - start
                ];
            
            // First index that is being read from the base array.
            int baseArrayIndex = start;
            for (int i = 0; i < subArray.Length; i++)
            {
                subArray[i] = baseArray[baseArrayIndex];
                baseArrayIndex++;
            }

            return subArray;
        }

        /// <summary>
        /// Converts an array to a substring and then returns a substring of it from the first index to the last index.
        /// </summary>
        /// <param name="array">Array to merge to string</param>
        /// <param name="firstIndex">First letter index of the substring</param>
        /// <param name="lastIndex">Last letter index of the substring</param>
        /// <returns>Substring of an array as a word</returns>
        public static string Substring(this string[] array, int firstIndex, int? lastIndex = null)
        {
            //string word = "";
            //for(int i = firstIndex; i < lastIndex; i++)
            //{
            //    if(i >= array.Length)
            //    {
            //        break;
            //    }
            //    word = word + array[i];
            //}
            string word = "";
            foreach(string s in array.Subarray(firstIndex, lastIndex))
            {
                word = word + s;
            }
            return word;
            //string word = "";
            //foreach(string s in array)
            //{
            //    word = word + s;
            //}
            //int len = lastIndex - firstIndex;
            //return word.Substring(firstIndex, len);
        }

        public static string ToConcatString<T>(this System.Collections.Generic.IEnumerable<T> array)
        {
            string concat = "";

            foreach(var item in array)
            {
                concat += item + ", ";
            }

            return concat.Substring(0, concat.Length - 2);
        }

        /// <summary>
        /// Checks the string for instances of color codes. 
        /// If the colorcode is two digits, a space is removed from the final text, and thus and extra space needs to be added for each color code that's 10 or larger.
        /// </summary>
        /// <returns>The number of spaces required to correct the color code caused missing spaces.</returns>
        public static int ColorSpacingCorrectionRequirement(this string sub)
        {
            int correction = 0;

            while (sub.Contains(Color.Clear))
            {
                int clrIndex = sub.IndexOf(Color.Clear);

                if(clrIndex > -1)
                {
                    clrIndex++;
                    if (clrIndex < sub.Length)
                    {
                        int i;
                        string num = sub[clrIndex].ToString();
                        if (int.TryParse(num, out i))
                        {
                            clrIndex++;
                            num = sub[clrIndex].ToString();
                            if (int.TryParse(num, out i))
                            {
                                correction++;
                            }
                        }
                        sub = sub.Substring(clrIndex);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return correction;
        }

        /// <summary>
        /// Inserts text at a specific index. If the index is out of bounds of the string, the string is extended to include the index.
        /// </summary>
        public static string InsertAbsolute(this string str, int index, string text)
        {
            if(index < str.Length)
            {
                return str.Insert(index, text);
            }
            else
            {
                int diff = index - str.Length;

                int corr = str.ColorSpacingCorrectionRequirement(); 

                diff += corr;
                index += corr;

                //for(int i = 0; i < diff; i++)
                //{
                //    str = str + " ";
                //}

                str = str.Extend(diff);

                return str.Insert(index, text);
            }
        }

        /// <summary>
        /// Extends the string length with given number of empty spaces.
        /// </summary>
        public static string Extend(this string str, int extension)
        {
            for (int i = 0; i < extension; i++)
            {
                str = str + " ";
            }
            return str;
        }

        #endregion String array extensions

    }
}
