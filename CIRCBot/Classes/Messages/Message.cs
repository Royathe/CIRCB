using System;

namespace CIRCBot
{
    class Msg
    {
        #region Constants

        /// <summary>
        /// The beginning of a command
        /// </summary>
        public const string COMMAND_IDENT = "!";

        /// <summary>
        /// Identifies a raw StreamReader input as being sent by a user.
        /// </summary>
        public const string MESSAGE_IDENT = " PRIVMSG ";

        /// <summary>
        /// Dictionary entry for an invalid command.
        /// </summary>
        public const string EMPTY_COMMAND = "none";

        #endregion Constants

        #region Command indexing

        /// <summary>
        /// The next parameter to check the value of.
        /// </summary>
        public int CurrentCommandIndex { get; set; }

        /// <summary>
        /// Get the current parameter. If index is greater than number of params, return empty string.
        /// </summary>
        public string Command
        {
            get
            {
                if (CurrentCommandIndex >= CommandParts.Length)
                {
                    return String.Empty;
                }
                return CommandParts[CurrentCommandIndex];
            }
            set
            {
                CommandParts[CurrentCommandIndex] = value;
            }
        }

        /// <summary>
        /// Increase param index and return the new "current parameter".
        /// </summary>
        public string NextCommand()
        {
            CurrentCommandIndex++;
            return Command;
        }

        #endregion Command indexing

        /// <summary>
        /// Deduced command
        /// </summary>
        public string ValidatedCommand { get; set; }

        public string[] CommandParts { get; }

        public string Raw { get; }

        public string Text { get;}
        
        public User From { get; }

        public string To { get; }

        public string Address { get; }

        public bool IsAdmin { get; }

        public bool IsPrivate { get; }

        public bool IsValid
        {
            get
            {
                if(IsValidUser && IsValidCommand)
                {
                    return true;
                }
                return false;
            }
        }

        private bool IsValidUser
        {
            get
            {
                return ((From != null && From.IsValid && (From.IsAuthorized || IsAdmin)) || IsAdmin);
            }
        }

        private bool IsValidCommand
        {
            get
            {
                if (CommandParts.Length > 0 && !Cmd.Invalid.Contains(Command) && Command.TrimEnd(' ') != String.Empty)
                {
                    return true;
                }
                return false;
            }
        }

        public Msg(string rawInput, string channel)
        {

            CurrentCommandIndex = 0;

            Raw = rawInput;

            rawInput = rawInput.ToLower();

            CommandParts = new string[]
            {
                EMPTY_COMMAND
            };

            string[] inputParts = Raw.Split(' ');

            string[] senderParts = inputParts[0].Split('!');

            if (senderParts[0].Substring(0, 1) == ":")
            {
                senderParts[0] = senderParts[0].Remove(0, 1);
            }

            string from = senderParts[0];

            Address = senderParts[1].Substring(senderParts[1].IndexOf("@") + 1).TrimEnd();

            To = inputParts[2].Trim();

            IsPrivate = To == channel ? false : true;

            Text = Raw.Substring(Raw.IndexOf("PRIVMSG"));
            Text = Text.Substring(Text.IndexOf(":")).Remove(0, 1);

            IsAdmin = Users.IsAdmin(Address);

            // Get user by admin address if admin. Otherwise get through sender name.
            //if (IsAdmin)
            //{
            //    From = Users.GetAdmin(Address);
            //}
            //else
            //{
            //    From = Users.Get(from);
            //}
            From = Users.Get(from);

            // If sender is not valid, stop parsing message.
            if (!IsValidUser)
            {
                From = null;
                return;
            }

            if (Text.Contains(COMMAND_IDENT))
            {
                // Take part of the text that starts after the '!' character. 
                // Split resulting text by spaces to a maximum of 6 strings.
                string[] commandParts = Text.Substring(Text.IndexOf(COMMAND_IDENT) + 1).SplitMax(' ', 6);

                // Commands parameters are string in the array after the first string.
                CommandParts = commandParts;
            }

            string cmdParamString = "";

            if (CommandParts.Length > 0)
            {
                foreach (string cmd in CommandParts)
                {
                    cmdParamString += cmd + ", ";
                }

                cmdParamString = cmdParamString.Substring(0, cmdParamString.Length - 2);
            }

            Console.WriteLine(
                (IsAdmin ? "Admin | " : "") +
                (IsPrivate ? "Private" : "Public") +
                " message from " + From.Username + ": " + Text);
            if (Command != EMPTY_COMMAND) { Console.WriteLine("    Command: " + Command + " | Params: " + cmdParamString); }
            Console.WriteLine();
        }
    }
}
