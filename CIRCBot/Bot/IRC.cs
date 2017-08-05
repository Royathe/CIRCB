using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace CIRCBot.Bot
{

    #region Delegates
    public delegate void CommandReceived(string IrcCommand, string Channel);
    public delegate void TopicSet(string IrcChannel, string IrcTopic);
    public delegate void TopicOwner(string IrcChannel, string IrcUser, string TopicDate);
    public delegate void NamesList(string UserNames);
    public delegate void ServerMessage(string ServerMessage);
    public delegate void Join(string IrcChannel, string IrcUser);
    public delegate void Part(string IrcChannel, string IrcUser);
    public delegate void Mode(string IrcChannel, string IrcUser, string UserMode);
    public delegate void NickChange(string UserOldNick, string UserNewNick);
    public delegate void Kick(string IrcChannel, string UserKicker, string UserKicked, string KickMessage);
    public delegate void Quit(string UserQuit, string QuitMessage);
    public delegate void ConnectedToServer();
    public delegate void ConnectedToChannel();
    public delegate void NickInUse(string OldNick);
    public delegate void Banned();
    public delegate void Error();
    #endregion Delegates

    class IRC
    {
        #region Events
        public event CommandReceived eventReceiving;
        public event TopicSet eventTopicSet;
        public event TopicOwner eventTopicOwner;
        public event NamesList eventNamesList;
        public event ServerMessage eventServerMessage;
        public event Join eventJoin;
        public event Part eventPart;
        public event Mode eventMode;
        public event NickChange eventNickChange;
        public event Kick eventKick;
        public event Quit eventQuit;
        public event ConnectedToServer eventConnectedToServer;
        public event ConnectedToChannel eventConnectedToChannel;
        public event NickInUse eventNickInUse;
        public event Banned eventBanned;
        public event Error eventError;
        #endregion Events

        #region Private Variables
        private IRCSettings Settings { get; }
        private TcpClient IrcConnection;
        private NetworkStream IrcStream;
        private StreamReader IrcReader;
        private StreamWriter IrcWriter;
        private bool Connected;
        #endregion Private Variables

        #region Properties

        private string name;

        public string Name
        {
            get
            {
                if(name == null)
                {
                    return this.Settings.Name;
                }
                return name;
            }
            set
            {
                name = value;
            }
        }

        //public string Name {
        //    get { return this.Settings.Name; }
        //}
        //public string Name { get; set; }

        public string Channel {
            get { return this.Settings.Channel; }
        }

        #endregion Properties

        #region Constructor

        public IRC()
        {
            Settings = new IRCSettings();
            Connected = false;
            Console.WriteLine(Settings);
        }

        #endregion Constructor

        #region Public methods

        public void Connect()
        {
            // Connect with the IRC server.
            this.IrcConnection = new TcpClient(this.Settings.Server, this.Settings.Port);
            this.IrcStream = this.IrcConnection.GetStream();
            this.IrcReader = new StreamReader(this.IrcStream);
            this.IrcWriter = new StreamWriter(this.IrcStream);

            SetUser();
            SetNick();
            Join();

            try
            {
                Listen();
            }
            catch(Exception ex)
            {
                Disconnect();
                Console.WriteLine(ex);
                Thread.Sleep(5000);
                Connect();
            }
        }

        public void SetUser()
        {
            this.IrcWriter.WriteLine(String.Format("USER {0} 0 * :{1}", this.Settings.Name, this.Settings.RealName));
            this.IrcWriter.Flush();
        }

        public void SetNick(string nick = "")
        {
            if(nick == "")
            {
                nick = Settings.Name;
            }
            this.IrcWriter.WriteLine(String.Format("NICK {0}", nick));
            this.IrcWriter.Flush();
        }

        public void Join()
        {
            this.IrcWriter.WriteLine(String.Format("JOIN {0}", this.Settings.Channel));
            this.IrcWriter.Flush();
        }

        public void Reconnect(int waitSeconds = 5)
        {
            //Disconnect();
            //Connect();
            Thread.Sleep(waitSeconds * 1000);
            Join();
        }

        public void Send(string message)
        {
            this.IrcWriter.WriteLine(message);
            this.IrcWriter.Flush();
        }

        public void Notice(string message, string receiver)
        {
            this.Send(String.Format("NOTICE {0} :{1}", receiver, message));
        }

        public void Say(string message, string receiver)
        {
            this.Send(String.Format("PRIVMSG {0} :{1}", receiver, message));
        }

        public void Say(string message)
        {
            this.Say(message, this.Channel);
        }

        public void Action(string message)
        {
            this.Send(String.Format("PRIVMSG {0} :\u0001ACTION {1}\u0001", this.Channel, message));
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// Close connection.
        /// </summary>
        private void Disconnect()
        {
            this.IrcWriter.Close();
            this.IrcReader.Close();
            this.IrcConnection.Close();
        }

        private void Listen()
        {
            string ircCommand;

            // Listen for commands
            while (true)
            {
                while((ircCommand = this.IrcReader.ReadLine()) != null)
                {
                    // Split command by spaces
                    string[] commandParts = new string[ircCommand.Split(' ').Length];
                    commandParts = ircCommand.Split(' ');

                    //Console.WriteLine(" - - -> " + ircCommand);

                    // Remove the ':' from the beginning of the first part
                    if (commandParts[0].Substring(0, 1) == ":")
                    {
                        commandParts[0] = commandParts[0].Remove(0, 1);
                    }

                    if(commandParts[0] == "ERROR")
                    {
                        this.IrcError();
                    }

                    if (commandParts[0] == this.Settings.Server)
                    {
                        // Server message
                        switch (commandParts[1])
                        {
                            case "001": this.IrcConnectedToServer(commandParts); break;
                            case "332": this.IrcTopic(commandParts); break;
                            case "333": this.IrcTopicOwner(commandParts); break;
                            case "353": this.IrcNamesList(commandParts); break;
                            case "366": this.IrcConnectedToChannel(commandParts); break;
                            case "372": /*this.IrcMOTD(commandParts);*/ break;
                            case "376": /*this.IrcEndMOTD(commandParts);*/ break;
                            case "433": this.IrcNickInUse(); break;
                            case "474": this.IrcBanned(); break;
                            default:
                                this.IrcServerMessage(commandParts);
                                break;
                        }
                    }
                    else if (commandParts[0] == "PING")
                    {
                        // Server PING, send PONG back
                        this.IrcPing(commandParts);
                    }
                    else
                    {
                        // Normal message
                        string commandAction = commandParts[1];
                        switch (commandAction)
                        {
                            case "JOIN": this.IrcJoin(commandParts); break;
                            case "PART": this.IrcPart(commandParts); break;
                            case "MODE": this.IrcMode(commandParts); break;
                            case "NICK": this.IrcNickChange(commandParts); break;
                            case "KICK": this.IrcKick(commandParts); break;
                            case "QUIT": this.IrcQuit(commandParts); break;
                            default:
                                if (eventReceiving != null && Connected) { this.eventReceiving(ircCommand, this.Channel); }
                                break;
                        }
                    }
                }

                Disconnect();
            }
        } // Listen

        #region Server messages

        private void IrcError()
        {
            if(eventError != null) { this.eventError(); }
        }

        private void IrcNickInUse()
        {
            if(eventNickInUse != null) { this.eventNickInUse(this.Name); }
        }

        private void IrcBanned()
        {
            if(eventBanned != null) { this.eventBanned(); }
        }

        private void IrcConnectedToServer(string[] IrcCommand)
        {
            if (eventConnectedToServer != null) { this.eventConnectedToServer(); }
        } /* IrcConnectedToServer */

        private void IrcConnectedToChannel(string[] IrcCommand)
        {
            this.Connected = true;
            if (eventConnectedToChannel != null) { this.eventConnectedToChannel(); }
        } /* IrcConnectedToChannel */

        private void IrcTopic(string[] IrcCommand)
        {
            string IrcChannel = IrcCommand[3];
            string IrcTopic = "";
            for (int intI = 4; intI < IrcCommand.Length; intI++)
            {
                IrcTopic += IrcCommand[intI] + " ";
            }
            if (eventTopicSet != null) { this.eventTopicSet(IrcChannel, IrcTopic.Remove(0, 1).Trim()); }
        } /* IrcTopic */

        private void IrcTopicOwner(string[] IrcCommand)
        {
            string IrcChannel = IrcCommand[3];
            string IrcUser = IrcCommand[4].Split('!')[0];
            string TopicDate = IrcCommand[5];
            if (eventTopicOwner != null) { this.eventTopicOwner(IrcChannel, IrcUser, TopicDate); }
        } /* IrcTopicOwner */

        private void IrcNamesList(string[] IrcCommand)
        {
            string UserNames = "";
            for (int intI = 5; intI < IrcCommand.Length; intI++)
            {
                UserNames += IrcCommand[intI] + " ";
            }
            if (eventNamesList != null) { this.eventNamesList(UserNames.Remove(0, 1).Trim()); }
        } /* IrcNamesList */

        private void IrcServerMessage(string[] IrcCommand)
        {
            string ServerMessage = "";
            for (int intI = 1; intI < IrcCommand.Length; intI++)
            {
                ServerMessage += IrcCommand[intI] + " ";
            }
            if (eventServerMessage != null) { this.eventServerMessage(ServerMessage.Trim()); }
        } /* IrcServerMessage */

        #endregion Server messages
        
        #region Ping

        /// <summary>
        /// Receieved a PING message from the server, send PONG response.
        /// </summary>
        /// <param name="commandParts"></param>
        private void IrcPing(string[] IrcCommand)
        {
            string PingHash = "";
            for (int intI = 1; intI < IrcCommand.Length; intI++)
            {
                PingHash += IrcCommand[intI] + " ";
            }
            this.Send("PONG " + PingHash);
        } /* IrcPing */

        #endregion Ping

        #region User messages

        private void IrcJoin(string[] IrcCommand)
        {
            string IrcChannel = IrcCommand[2];
            string IrcUser = IrcCommand[0].Split('!')[0];
            if (eventJoin != null) { this.eventJoin(IrcChannel.Remove(0, 1), IrcUser); }
        } /* IrcJoin */

        private void IrcPart(string[] IrcCommand)
        {
            string IrcChannel = IrcCommand[2];
            string IrcUser = IrcCommand[0].Split('!')[0];
            if (eventPart != null) { this.eventPart(IrcChannel, IrcUser); }
        } /* IrcPart */

        private void IrcMode(string[] IrcCommand)
        {
            string IrcChannel = IrcCommand[2];
            string IrcUser = IrcCommand[0].Split('!')[0];
            string UserMode = "";
            for (int intI = 3; intI < IrcCommand.Length; intI++)
            {
                UserMode += IrcCommand[intI] + " ";
            }
            if (UserMode.Substring(0, 1) == ":")
            {
                UserMode = UserMode.Remove(0, 1);
            }
            if (eventMode != null) { this.eventMode(IrcChannel, IrcUser, UserMode.Trim()); }
        } /* IrcMode */

        private void IrcNickChange(string[] IrcCommand)
        {
            string UserOldNick = IrcCommand[0].Split('!')[0];
            string UserNewNick = IrcCommand[2].Remove(0, 1);
            if (eventNickChange != null) { this.eventNickChange(UserOldNick, UserNewNick); }
        } /* IrcNickChange */

        private void IrcKick(string[] IrcCommand)
        {
            string UserKicker = IrcCommand[0].Split('!')[0];
            string UserKicked = IrcCommand[3];
            string IrcChannel = IrcCommand[2];
            string KickMessage = "";
            for (int intI = 4; intI < IrcCommand.Length; intI++)
            {
                KickMessage += IrcCommand[intI] + " ";
            }
            if (eventKick != null) { this.eventKick(IrcChannel, UserKicker, UserKicked, KickMessage.Remove(0, 1).Trim()); }
        } /* IrcKick */

        private void IrcQuit(string[] IrcCommand)
        {
            string UserQuit = IrcCommand[0].Split('!')[0];
            string QuitMessage = "";
            for (int intI = 2; intI < IrcCommand.Length; intI++)
            {
                QuitMessage += IrcCommand[intI] + " ";
            }
            if (eventQuit != null) { this.eventQuit(UserQuit, QuitMessage.Remove(0, 1).Trim()); }
        } /* IrcQuit */

        #endregion User messages

        #endregion Private methods

    }
}
