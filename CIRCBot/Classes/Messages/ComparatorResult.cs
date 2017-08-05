namespace CIRCBot.Execution
{
    class ComparatorResult
    {

        public bool Matched { get; set; }

        public string OrigCommand { get; set; }

        public string ValidCommand { get; set; }

        public ComparatorResult(string command)
        {
            Matched = false;
            OrigCommand = command;
            ValidCommand = Library.EMPTY_COMMAND;
        }

        public void Match(string validCommand)
        {
            Matched = true;
            ValidCommand = validCommand;
        }
    }
}
