using CIRCBot.Execution.Comparators;
using System;
using System.Collections.Generic;

namespace CIRCBot.Execution
{
    /// <summary>
    /// Processes a command to a valid command type.
    /// </summary>
    class MasterComparator
    {

        private SyllableComparator SylComparator { get; }

        public MasterComparator()
        {
            SylComparator = new SyllableComparator();

            AdvancedChecks = new Dictionary<string, Func<string, bool>>();
            
            AdvancedChecks.Add(Cmd.Check, check);
            AdvancedChecks.Add(Cmd.Blackjack, blackjack);
            AdvancedChecks.Add(Cmd.Holdem, holdem);
            AdvancedChecks.Add(Cmd.Overwatch, overwatch);
        }

        public void Reset()
        {
            SylComparator.Reset();
        }

        private Dictionary<string, Func<string, bool>> AdvancedChecks { get; }

        public ComparatorResult Process(string command, List<string> executorKeys)
        {
            ComparatorResult results = new ComparatorResult(command);

            // Is command a part of the executor's keys.
            if (executorKeys.Contains(command))
            {
                results.Match(command);
                return results;
            }

            // Invoke each advanced check that matches a command in the current executor's command dictionary.
            foreach(string key in executorKeys)
            {
                if(AdvancedChecks.ContainsKey(key))
                {
                    // If the command is matched by the advanced matcher, return results with the current key from the executor's dictionary.
                    if(AdvancedChecks[key].Invoke(command))
                    {
                        results.Match(key);
                        return results;
                    }
                }
            }

            // Start running syllable comparator
            SylComparator.CommandToSyllables(command);

            // Run comparator with each key in the comparables.
            foreach(string key in Cmd.Comparables)
            {
                if(executorKeys.Contains(key))
                {
                    //If matched, set the current key as the result's valid command.
                    if (SylComparator.Match(key))
                    {
                        results.Match(key);
                        return results;
                    }
                }
            }

            return results;
        }



        #region Private actions | Advanced checks

        private bool check(string command)
        {
            // Parse out first 12 characters
            command = command.SubstringMax(0, 12);

            // Valid if contains "kek" without "top"
            if (command.Contains("kek") && !command.Contains("top"))
            {
                return true;
            }

            // Has to be atleast 5 characters long
            if (command.Length >= 5)
            {
                if (command.IndexOf("c") == 0)
                {
                    // Must have first indexes of "c" before "h" before "e"
                    if (command.IndexOf("c") < command.IndexOf("h") && command.IndexOf("h") < command.IndexOf("e"))
                    {
                        // Must have either (atleast 2 k's) or (atleast 2 c's AND a k)
                        if (
                            command.CountOf("k") >= 2
                            ||
                            (command.CountOf("c") >= 2 && command.CountOf("k") >= 1)
                          )
                        {
                            return true;
                        }
                    }
                }
            }
            // Also accpet any mispelling of "check" or "chekk"
            if (command.Length == 5)
            {
                if (

                    (
                        (command.CountOf("c") >= 2 && command.CountOf("k") >= 1)
                        ||
                        (command.CountOf("k") >= 2 && command.CountOf("c") >= 1)
                    )
                    &&
                    command.CountOf("e") >= 1
                    &&
                    command.CountOf("h") >= 1
                )
                {
                    return true;
                }
            }

            return false;
        }

        private bool holdem(string command)
        {
            // Parse out first 9 characters
            command = command.SubstringMax(0, 9);

            // Has to be atleast 6 characters long
            if (command.Length >= 6)
            {
                // Must start with "h"
                if (command.IndexOf("h") == 0)
                {
                    if (
                        // Must have the first "m" after first "d" after first "l" after first "h"
                        (command.IndexOf("h") < command.IndexOf("l") && command.IndexOf("l") < command.IndexOf("d") && command.IndexOf("d") < command.IndexOf("m"))
                        ||
                        // Or an "lm" after a "d" that's after the "h" (ex. hedelm)
                        (command.IndexOf("lm") > command.IndexOf("d") && command.IndexOf("d") > command.IndexOf("h"))
                    )
                    {
                        return true;
                    }
                }
                // Must start with "gay"
                if (command.IndexOf("gay") == 0)
                {
                    if (
                        // Must have the first "m" after first "d" after first "l" after first "h"
                        (command.IndexOf("gay") < command.IndexOf("d") && command.IndexOf("d") < command.IndexOf("m"))
                        ||
                        // Or an "lm" after a "d" that's after the "h" (ex. hedelm)
                        (command.IndexOf("lm") > command.IndexOf("d") && command.IndexOf("d") > command.IndexOf("h"))
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool blackjack(string command)
        {
            // Parse out first 12 characters
            command = command.SubstringMax(0, 12);

            string start;
            string end;
            var splitIndex = 5;

            if (command.IndexOf("j") > 0)
            {
                splitIndex = command.IndexOf("j");
            }
            else if (command.IndexOf("d") > 0)
            {
                splitIndex = command.IndexOf("d");
            }

            start = command.SubstringMax(0, splitIndex);
            end = command.SubstringMax(splitIndex);

            if (end.IndexOf("n") > 0)
            {
                end = command.SubstringMax(4);
                splitIndex = end.IndexOf("n") + 4;

                start = command.SubstringMax(0, splitIndex);
                end = command.SubstringMax(splitIndex);
            }

            if (command.Length > 6 && end.Length > 3)
            {
                if (
                    (   // Similar to nigga, musta, black
                        (   // Starts with nigga, negge...
                            start.IndexOf("g") > 0
                            &&
                            start.CountOf("g") >= 2
                            &&
                            start.IndexOf("n") == 0
                        )
                        ||
                        (   // Starts with musta, meste...
                            start.IndexOf("st") > 1
                            &&
                            start.IndexOf("m") == 0
                        )
                        ||
                        (   // Starts with black, blekc...
                            start.IndexOf("k") > 0
                            &&
                            start.CountOf("l") == 1
                            &&
                            start.IndexOf("b") == 0
                        )
                    )
                    &&
                    (
                        (   // Continues with jaakko, jack, jekk...
                            end.IndexOf("j") == 0
                            &&
                            end.IndexOf("k") > 1
                        )
                        ||
                        (   // Continues with dick, dikk, dekk...
                            end.IndexOf("d") == 0
                            &&
                            end.IndexOf("k") > 1
                        )
                        ||
                        (   // Continues with giggel, geggel, giguli...
                            end.IndexOf("g") == 0
                            &&
                            end.CountOf("g") >= 2
                            &&
                            end.IndexOf("l") > end.IndexOf("g")
                        )
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }

        private bool overwatch(string command)
        {
            // Parse out first 12 characters
            command = command.SubstringMax(0, 12);

            // Check for a type of "gayerii"
            if (
                (   // Various combinations fo gay/homo with ver, yli, watch and kello
                    (
                        command.IndexOf("gay") >= 0
                        ||
                        command.IndexOf("homo") >= 0
                    )
                    &&
                    (
                        command.IndexOf("ver") >= 0
                        ||
                        command.IndexOf("yl") >= 0
                        ||
                        command.IndexOf("watch") >= 0
                        ||
                        command.IndexOf("kello") >= 0
                    )
                )
                ||
                (command.IndexOf("ver") >= 1 && command.CountOf("i") > 0) // överii
                ||
                (command.IndexOf("gayer") != -1) // gayerii
            )
            {
                return true;
            }

            return false;
        }

        #endregion Private actions | Advanced checks

    }
}
