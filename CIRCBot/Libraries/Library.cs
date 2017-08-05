namespace CIRCBot
{
    static class Library
    {

        #region General

        /// <summary>
        /// Identifies a raw StreamReader input as being sent by a user.
        /// </summary>
        public const string MESSAGE_IDENT = " PRIVMSG ";

        public const string COMMAND_IDENT = "!";

        /// <summary>
        /// Dictionary entry for an invalid command.
        /// </summary>
        public const string EMPTY_COMMAND = "none";

        /// <summary>
        /// Text indent.
        /// </summary>
        public const string IND = "    ";

        /// <summary>
        /// Default description of an Command-Action pair's function.
        /// </summary>
        public const string NO_DESCRIPTION = "No description.";

        /// <summary>
        /// Centigrade character.
        /// </summary>
        public const string CENTIGRADE = "°";

        #endregion General

        #region JSON files
        
        /// <summary>
        /// Path to the JSON folder.
        /// </summary>
        public const string JSON_path = @"P:\VS2015 Projects\CIRCBot\CIRCBot\JSON\";

        /// <summary>
        /// Path to the city.list.json file.
        /// </summary>
        public const string JSON_City_List = JSON_path + "city.list.json";

        #endregion JSON files

        #region Resource texts

        public const string Vowels = "AEIOUYÅÄÖ";

        public const string Hv = "凸( ͡° ͜ʖ ͡°)凸";

        public const string Lenny = "( ͡° ͜ʖ ͡°)";

        public static readonly string[] Overwatch = new string[]
        {
            "ny {0}Overwatchii",
            "{0}ylikellot tulille",
            "{0}päivän turpasaunaan",
            "{0}röllii overgay",
            "tappakaa se vitun parah",
            "Kids face! t. reinisydän"
        };

        #endregion Resource texts

    }
}
