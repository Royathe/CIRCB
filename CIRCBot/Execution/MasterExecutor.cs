using System;
using CIRCBot.Execution.Executors;

namespace CIRCBot.Execution
{
    /// <summary>
    /// Main class of the Execution pipeline.
    /// </summary>
    [ClassName("Master")]
    class MasterExecutor : BaseExecutor
    {

        #region Command processor accessors

        private MasterComparator Comparator { get; }

        private IExecutor[] Executors { get; }

        #endregion Command processor accessors

        #region Constructor

        public MasterExecutor(BotMessager messager) : base(messager)
        {
            Comparator = new MasterComparator();

            Add("Kirjaa botin eri komentokäsittelykomponentit.", LogComponents, "komponentit", "components", "käsittelijät");

            Executors = new IExecutor[]
            {
                new CmdAdmin(Bot),
                new CmdStatistics(Bot),
                new CmdLink(Bot),
                new CmdGames(Bot),
                new CmdGeneral(Bot),
                new CmdRandom(Bot)
            };
        }

        #endregion Constructor

        #region Public methods

        /// <summary>
        /// Main message executor. Calls sub-handlers.
        /// </summary>
        /// <param name="message">Message to execute</param>
        public new void Execute(Msg message)
        {
            base.Execute(message);

            // If command is a command of the master executor, return after executing.
            if (RunCommand() == CmdResult.Success)
            {
                return;
            }

            // Clear the Comparator's saved data before processing.
            Comparator.Reset();

            // Run generic executors with command comparator
            foreach (IExecutor executor in Executors)
            {
                if (RunExecutor(executor, Message))
                {
                    Comparator.Reset();
                    return;
                }
            }

            // Clear the saved command in the Comparator
            Comparator.Reset();
        }

        #endregion Public methods

        #region Private methods

        private bool RunExecutor(IExecutor executor, Msg message)
        {
            ComparatorResult results = Comparator.Process(message.Command, executor.Commands);
            if(results.Matched)
            {

                Console.WriteLine(message.Command + " deduced to: " + results.ValidCommand);

                message.Command = results.ValidCommand;
                executor.Execute(message);
                return true;
            }
            return false;
        }

        #endregion Private methods

        #region Commands

        private void LogComponents()
        {
            string sender = Message.From.Username;

            Bot.Say(sender, "_________________________________________________________________");
            Bot.Say(sender, "Botin komponentit. '!(komponentin nimi) commands' kirjaa komponentin eri komennot.");
            Bot.Say(sender, " - ");

            foreach (IExecutor executor in Executors)
            {
                if(executor.Commands.Contains("commands"))
                {
                    Bot.Say(sender, executor.Identifier + " ||| " + executor.Description);
                }
            }
            Bot.Say(sender, "_________________________________________________________________");
        }

        #endregion Commands

    }
}
