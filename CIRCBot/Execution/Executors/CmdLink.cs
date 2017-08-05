using System;
using System.Collections.Generic;
using System.Linq;

namespace CIRCBot.Execution.Executors
{
    [ClassName("Link", "Yksinkertaiset linkkikomennot")]
    class CmdLink : BaseExecutor, IExecutor
    {

        private readonly Random rnd;

        public new bool IsCommand(string command)
        {
            return Cmd.Simple.ContainsKey(command);
        }

        public new List<string> Commands
        {
            get
            {
                return Cmd.Simple.Keys.ToList();
            }
        }

        public CmdLink(BotMessager messager) : base(messager)
        {
            rnd = new Random();
        }

        public new void Execute(Msg message)
        {
            base.Execute(message);

            // Get the dictionary entry that matches the given command
            string botmsg = Cmd.Simple[message.Command];

            // Split bot message with |. If multiple results, choose one at random.
            string[] variations = botmsg.Split('|');

            if(variations.Length > 1)
            {
                botmsg = variations[rnd.Next(0, variations.Length)];
            }

            Bot.Say(botmsg);
        }
    }
}
