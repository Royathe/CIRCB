using System.Collections.Generic;

namespace CIRCBot
{
    interface IExecutor
    {

        string Identifier { get; }

        string Description { get; }

        void Execute(Msg message);

        List<string> Commands { get; }

    }
}
