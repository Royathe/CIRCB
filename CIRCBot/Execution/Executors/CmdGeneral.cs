﻿using System;

namespace CIRCBot.Execution.Executors
{
    /// <summary>
    /// Comparator based simple commands ex. rr, nn, topkek
    /// </summary>
    [ClassName("General", "Yleiset komennot")]
    class CmdGeneral : BaseExecutor, IExecutor
    {

        #region Library
        
        private class Library
        {
            internal const string Hv = "凸( ͡° ͜ʖ ͡°)凸";

            internal const string Lenny = "( ͡° ͜ʖ ͡°)";

            internal static readonly string[] Overwatch = new string[]
            {
            "ny {0}Overwatchii",
            "{0}ylikellot tulille",
            "{0}päivän turpasaunaan",
            "{0}röllii overgay",
            "tappakaa se vitun parah",
            "Kids face! t. reinisydän"
            };
        }

        #endregion Library

        private readonly Random rnd;

        public CmdGeneral(BotMessager messager) : base(messager)
        {

            rnd = new Random();

            Add("Satunnainen riemurasia media", cmd_rr, "rr");
            Add("Satunnainen naurunappula media", cmd_nn, "nn");
            Add("Satunnaista tekstiä", cmd_eijasa, "eijasa");
            Add("Satunnainen Overwatch kirjaston teksti", cmd_ow, "overwatch", "ow");
            Add("Haistata vitut", cmd_hv, "hv");
            Add("Lenny face", cmd_lenny, "lenny", "lennyface", "lf");
        }

        public new void Execute(Msg message)
        {
            base.Execute(message);

            LogResults(RunCommand());
        }

        #region Commands

        private void cmd_rr()
        {
            Bot.Say(Utils.GetRedirectResult("http://www.riemurasia.net/jylppy/random.php"));
        }

        private void cmd_nn()
        {
            Bot.Say(Utils.GetRedirectResult("http://naurunappula.com/random.php?c=1"));
        }

        private void cmd_lenny()
        {
            Bot.Say(Library.Lenny);
        }

        private void cmd_topkek()
        {
            int i = rnd.Next(0,2);
            if(i == 0)
            {
                cmd_rr();
            }
            else
            {
                cmd_nn();
            }
        }

        private void cmd_hv()
        {
            if(Message.CommandParts.Length > 1)
            {
                Bot.Action("| " + Message.CommandParts[1] + " " + Library.Hv);
            }
            else
            {
                Bot.Action("| " + Library.Hv);
            }
        }

        private void cmd_eijasa()
        {
            // String length. Between 25 and 200.
            int letterCount = rnd.Next(25, 200);

            string mes = "";

            for(int i = 0; i < letterCount; i++)
            {
                mes += RandomLetter;
            }

            mes += ".";

            Bot.Say(mes);
        }

        private void cmd_ow()
        {
            int textIndex = rnd.Next(0, Library.Overwatch.Length);

            string names = "";

            if(Message.CommandParts.Length > 1)
            {
                for(int i = 1; i < Message.CommandParts.Length; i++)
                {
                    // If index is the second last index
                    if(i == Message.CommandParts.Length - 2)
                    {
                        names += Message.CommandParts[i] + " ja ";
                    }
                    // If index is the last index
                    else if (i == Message.CommandParts.Length - 1)
                    {
                        names += Message.CommandParts[i] + " ";
                    }
                    else
                    {
                        names += Message.CommandParts[i] + ", ";
                    }
                }
            }
            Bot.Say(
                String.Format(Library.Overwatch[textIndex], names)
                );
        }

        #endregion Commands

        #region Private methods

        private string RandomLetter
        {
            get
            {
                int num = rnd.Next(0, 28); // 25 is the max for a letter.

                switch(num)
                {
                    case 26:
                        return " ";

                    case 27:
                        return ". ";

                    default:
                        char let = (char)('a' + num);
                        return let.ToString();

                }
            }
        }

        #endregion Private methods

    }
}
