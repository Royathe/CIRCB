using System;

using CIRCBot.Bot;
using CIRCBot.Sql;
using CIRCBot.Execution;
using System.Linq;

namespace CIRCBot
{
    class cIRC
    {
        private IRC IrcObject;

        private MasterExecutor Executor;

        private BotMessager Messager;

        static void Main(string[] args)
        {
            if (DataBaseLoad())
            {
                cIRC IrcApp = new cIRC();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Fatal error in loading information from database. Exiting.");
            }
        }

        /// <summary>
        /// Load data from the database.
        /// </summary>
        private static bool DataBaseLoad()
        {
            try
            {
                Query.LoadDatabase();
                Console.WriteLine("Users and admins addresses loaded.");
                Console.WriteLine("USERS");
                foreach (User user in Users.All)
                {
                    Console.WriteLine(Library.IND + user.Username + " ||| IsAdmin: " + user.IsAdmin);
                }
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                Query.LoadCommands();
                Console.WriteLine("Simple Commands loaded.");
                Console.WriteLine("COMMANDS");
                foreach (var entry in Cmd.Simple)
                {
                    Console.WriteLine(Library.IND + entry.Key + " ||| " + entry.Value);
                }
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                
                Libraries.Cities.Load();
                Console.WriteLine("Cities loaded.");
                Console.WriteLine(Library.IND + Libraries.Cities.CountryCount + " countries.");
                Console.WriteLine(Library.IND + Libraries.Cities.CityCount + " cities.");

                Console.WriteLine();

                Libraries.WeatherConditions.Load();
                Console.WriteLine("Weather conditions loaded.");

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }

        private cIRC()
        {
            IrcObject = new IRC();
            Messager = new BotMessager();

            // Assign events
            IrcObject.eventReceiving += new CommandReceived(IrcCommandReceived);
            IrcObject.eventTopicSet += new TopicSet(IrcTopicSet);
            IrcObject.eventTopicOwner += new TopicOwner(IrcTopicOwner);
            IrcObject.eventNamesList += new NamesList(IrcNamesList);
            IrcObject.eventServerMessage += new ServerMessage(IrcServerMessage);
            IrcObject.eventJoin += new Join(IrcJoin);
            IrcObject.eventPart += new Part(IrcPart);
            IrcObject.eventMode += new Mode(IrcMode);
            IrcObject.eventNickChange += new NickChange(IrcNickChange);
            IrcObject.eventKick += new Kick(IrcKick);
            IrcObject.eventQuit += new Quit(IrcQuit);
            IrcObject.eventConnectedToServer += new ConnectedToServer(IrcConnectedToServer);
            IrcObject.eventConnectedToChannel += new ConnectedToChannel(IrcConnectedToChannel);
            IrcObject.eventNickInUse += new NickInUse(IrcBotChangeNick);
            IrcObject.eventBanned += new Banned(IrcBotBanned);
            IrcObject.eventError += new Error(IrcBotError);

            Messager.eventSay += new BotSay(IrcBotSay);
            Messager.eventAction += new BotAction(IrcBotAction);
            Messager.eventNotice += new BotNotice(IrcBotNotice);

            Executor = new MasterExecutor(Messager);

            // Connect to server
            IrcObject.Connect();
        } /* cIRC */

        private void IrcBotError()
        {
            IrcObject.Reconnect(60);
        }

        private void IrcBotBanned()
        {
            Console.WriteLine("Banned from channel.");
            IrcObject.Reconnect(120);
        }

        private void IrcBotChangeNick(string oldNick)
        {
            IrcObject.SetNick(oldNick + "_");
        } /* IrcBotChangeNick */

        private void IrcBotSay(string message, string to = "")
        {
            if(to == "")
            {
                IrcObject.Say(message);
            }
            else
            {
                IrcObject.Say(message, to);
            }
        } /* IrcBotSay */

        private void IrcBotAction(string message)
        {
            IrcObject.Action(message);
        } /* IrcBotAction */

        private void IrcBotNotice(string message, string to)
        {
            IrcObject.Notice(message, to);
        } /* IrcBotAction */

        private void IrcCommandReceived(string IrcCommand, string Channel)
        {
            if (IrcCommand.Contains(Library.MESSAGE_IDENT) && IrcCommand.Contains(Library.COMMAND_IDENT))
            {
                Msg message = new Msg(IrcCommand, IrcObject.Channel);
                if(message.IsValid)
                {
                    Executor.Execute(message);
                }
                else
                {
                    Console.WriteLine("Invalid or unatuhorized user/command. Ignoring message.");
                }
            }
            else
            {
                Console.WriteLine(" -> " + IrcCommand);
            }
        } /* IrcCommandReceived */

        private void IrcConnectedToServer()
        {
            Console.WriteLine("Connected to Server");
            IrcObject.Send("JOIN " + IrcObject.Channel);
        }

        private void IrcConnectedToChannel()
        {
            Console.WriteLine("Connected to " + IrcObject.Channel);
        }

        private void IrcTopicSet(string IrcChan, string IrcTopic)
        {
            Console.WriteLine(String.Format("Topic of {0} is: {1}", IrcChan, IrcTopic));
        } /* IrcTopicSet */

        private void IrcTopicOwner(string IrcChan, string IrcUser, string TopicDate)
        {
            Console.WriteLine(String.Format("Topic of {0} set by {1} on {2} (unixtime)", IrcChan, IrcUser, TopicDate));
        } /* IrcTopicSet */

        private void IrcNamesList(string UserNames)
        {
            Console.WriteLine(String.Format("Names List: {0}", UserNames));
        } /* IrcNamesList */

        private void IrcServerMessage(string ServerMessage)
        {
            //Console.WriteLine(String.Format("Server Message: {0}", ServerMessage));
        } /* IrcNamesList */

        private void IrcJoin(string IrcChan, string IrcUser)
        {
            Console.WriteLine(String.Format("{0} joins {1}", IrcUser, IrcChan));
            //IrcObject.Send(String.Format("NOTICE {0} :Hello {0}, welcome to {1}!", IrcUser, IrcChan));
        } /* IrcJoin */

        private void IrcPart(string IrcChan, string IrcUser)
        {
            Console.WriteLine(String.Format("{0} parts {1}", IrcUser, IrcChan));
        } /* IrcPart */

        private void IrcMode(string IrcChan, string IrcUser, string UserMode)
        {
            if (IrcUser != IrcChan)
            {
                Console.WriteLine(String.Format("{0} sets {1} in {2}", IrcUser, UserMode, IrcChan));
            }
        } /* IrcMode */

        private void IrcNickChange(string UserOldNick, string UserNewNick)
        {
            Console.WriteLine(String.Format("{0} changes nick to {1}", UserOldNick, UserNewNick));
        } /* IrcNickChange */

        private void IrcKick(string IrcChannel, string UserKicker, string UserKicked, string KickMessage)
        {
            Console.WriteLine(String.Format("{0} kicks {1} out {2} ({3})", UserKicker, UserKicked, IrcChannel, KickMessage));
            IrcObject.Reconnect();
        } /* IrcKick */

        private void IrcQuit(string UserQuit, string QuitMessage)
        {
            Console.WriteLine(String.Format("{0} has quit IRC ({1})", UserQuit, QuitMessage));
        } /* IrcQuit */
    }
}
