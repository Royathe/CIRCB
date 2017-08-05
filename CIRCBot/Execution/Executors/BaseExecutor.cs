using System;
using System.Linq;
using System.Collections.Generic;

namespace CIRCBot.Execution
{
    /// <summary>
    /// Base class of the Execution pipeline classes. 
    /// 
    /// <para>Command execution logic:</para>
    /// <para>
    /// CmdActions dictionary contains the keys for all the valid commands in the Message's parameters array,  
    /// paired with their respective CommandAction object.
    /// </para> 
    /// <para>
    /// The CommandAction object contains the method the command is meant to invoke, as well as
    /// the information on whether or not the command requires admin privileges to be executed.
    /// </para>
    /// <para>
    /// The getter "RunCommand", runs the Dictionary extension method "Execute" with the current parameter and the admin rights from current Message.
    /// </para>
    /// <para>
    /// The extension checks if the parameter is contained in the dictionary. If it is, it calls the Run method of the CommandAction object and
    /// returns True. If not, returns False.
    /// </para>
    /// <para>
    /// The getter "RunCommand" takes the result of the "Execute" extension, and returns a CmdResult type.
    /// </para>
    /// <para>
    /// Returns "Success" if the extension's return value was True,
    /// "InvalidCommand" if the return value was False, and "Error" if the Try/Catch caught an error while invoking.
    /// </para>
    /// 
    /// </summary>
    class BaseExecutor
    {

        #region Enumerations

        /// <summary>
        /// Enum of possible results for a Command Action execution.
        /// </summary>
        protected enum CmdResult
        {
            Success,
            InvalidCommand,
            Error
        }

        #endregion Enumerations

        #region Private readonly variables
        
        private readonly string className;

        #endregion Private readonly variables

        #region Public readonly variables

        /// <summary>
        /// String that identifies this Executor instance.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// String that describes the purpose of this Executor instance.
        /// </summary>
        public string Description { get; }

        public List<string> Commands
        {
            get
            {
                return CmdActions.Keys.ToList();
            }
        }

        #endregion Public readonly variables

        #region Protected accessors

        /// <summary>
        /// Current message being executed.
        /// </summary>
        protected Msg Message { get; set; }

        /// <summary>
        /// Dictionary of available commands for this executor.
        /// </summary>
        protected Dictionary<string, CommandAction> CmdActions { get; }

        /// <summary>
        /// Message to send to the admin.
        /// </summary>
        protected string MessageToAdmin { get; set; }

        /// <summary>
        /// Message relayer.
        /// </summary>
        protected BotMessager Bot { get; }

        #endregion Protected accessors

        #region Constructor

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="messager"></param>
        public BaseExecutor(BotMessager messager)
        {
            // Set the message relayer.
            Bot = messager;

            // Get the name of the class from the Class Name Attribute.
            var nameAttr = GetType().GetCustomAttributes(typeof(ClassNameAttribute), true).FirstOrDefault() as ClassNameAttribute;
            if (nameAttr != null)
            {
                // Set class name based on the attribute.
                className = nameAttr.Name;

                // Set the publicly visible identifier of this instance.
                Identifier = className.ToLower();

                // Set the description of the executor.
                Description = nameAttr.Description;
            }
            else
            {
                // If attribute not found, set the name to the last part of this object's namespace.
                string[] classNameParts = this.ToString().Split('.');
                className = classNameParts[classNameParts.Length-1];
            }

            // All instances of BaseExecutor can report their available commands.
            // The command itself for doing so is not logged.
            CmdActions = new Dictionary<string, CommandAction>();
            Add(LogCommands,
                "komennot", "commands");
            Add(ComponentCall,
                Identifier);
        }

        #endregion Constructor

        #region Public methods
        
        /// <summary>
        /// Check if command is apart of this executor's dictionary.
        /// </summary>
        public bool IsCommand(string command)
        {
            if (CmdActions.ContainsKey(command))
            {
                return true;
            }
            return false;
        }

        #endregion Public methods

        #region Command execution

        /// <summary>
        /// Base command execution method.
        /// </summary>
        /// <param name="message">Message the executor executes.</param>
        public void Execute(Msg message)
        {
            Message = message;
            MessageToAdmin = String.Empty;
            if(message.Command == Library.EMPTY_COMMAND)
            {
                return;
            }
        }

        /// <summary>
        /// Tries to execute the CommandAction in the CommandAction dictionary that matches the current parameter.
        /// Returns the succesfulness of the execution attempt.
        /// </summary>
        protected CmdResult RunCommand(string command = "")
        {
            // If command wasn't given, run current command
            if(command == "")
            {
                command = Message.Command;
            }
            try
            {
                if (CmdActions.Execute(command, Message.IsAdmin))
                {
                    return CmdResult.Success;
                }
                else
                {
                    return CmdResult.InvalidCommand;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine();
                Console.WriteLine(ex);
                Console.WriteLine();

                return CmdResult.Error;
            }
        }

        #endregion Command execution

        #region Invoked methods

        /// <summary>
        /// Report all valid commands to the admin.
        /// </summary>
        protected void LogCommands()
        {
            string sender = Message.From.Username;

            Bot.Say(sender, "_________________________________________________________________");
            Bot.Say(sender, className + " -komponentin komennot.");
            Bot.Say(sender, " - ");
            foreach(var entry in CmdActions.Loggables())
            {
                Bot.Say(sender, entry.Key + " ||| " + entry.Value.Description);
            }
            Bot.Say(sender, "_________________________________________________________________");
        }

        /// <summary>
        /// Command was the name of the component. Execute the command that follows the component name.
        /// </summary>
        protected void ComponentCall()
        {
            LogResults(
                RunCommand(Message.NextCommand())
                );
        }

        #endregion Invoked methods

        #region Execution loggers

        /// <summary>
        /// Log the results of the command execution.
        /// </summary>
        /// <param name="result">Result of the command execution.</param>
        protected void LogResults(CmdResult result)
        {
            switch(result)
            {
                case CmdResult.Success:
                    // Execution of command succesful.
                    break;

                case CmdResult.InvalidCommand:
                    Bot.Notice(Message.From.Username, Message.Command + " ei ole validi komento komponentille: " + className);
                    break;

                default:
                    logToAdmin();
                    break;
            }
        }

        /// <summary>
        /// Send a private message to all admins.
        /// </summary>
        private void logToAdmin()
        {
            if (MessageToAdmin != String.Empty)
            {
                MessageToAdmin = " -> Admin log: " + MessageToAdmin;
                Console.WriteLine(MessageToAdmin);
                foreach (User admin in Users.Admins)
                {
                    Bot.Notice(admin.Username, MessageToAdmin);
                }
            }
        }

        #endregion Execution loggers

        #region Dictionary additions

        protected CommandAction[] Add(string desc, Action invocation, params string[] keys)
        {
            List<CommandAction> CommandActions = new List<CommandAction>();

            // Only the first key is marked as loggable. Variations are not logged.
            bool isFirst = true;
            foreach(string key in keys)
            {
                if(isFirst)
                {
                    CmdActions.Add(key, new CommandAction(invocation, desc, false));
                    CommandActions.Add(CmdActions[key]);
                }
                else
                {
                    CmdActions.Add(key, new CommandAction(invocation, false));
                    CommandActions.Add(CmdActions[key]);
                }
                isFirst = false;
            }

            return CommandActions.ToArray();
        }

        protected CommandAction Add(Action invocation, params string[] keys)
        {
            var command = new CommandAction(invocation, false);
            foreach (string key in keys)
            {
                CmdActions.Add(key, command);
            }
            return command;
        }

        protected CommandAction Admin(string desc, Action invocation, params string[] keys)
        {
            var command = new CommandAction(invocation, desc, true);
            foreach (string key in keys)
            {
                CmdActions.Add(key, command);
            }
            return command;
        }

        protected CommandAction Admin(Action invocation, params string[] keys)
        {
            var command = new CommandAction(invocation, true);
            foreach (string key in keys)
            {
                CmdActions.Add(key, command);
            }
            return command;
        }

        #endregion Dictionary additions

    }
}
