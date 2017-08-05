namespace CIRCBot.Bot
{
    class IRCSettings
    {
        public string Name { get; }

        public string Server { get; }

        public string Channel { get; }

        public int Port { get; }

        public string User { get; }

        public string RealName { get; }
        
        public IRCSettings()
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;

            Name = appSettings["Nick"];
            User = appSettings["User"];
            Server = "port80b.se.quakenet.org";//appSettings["Server"];
            Port = int.Parse(appSettings["Port"]);
            RealName = appSettings["RealName"];
            Channel = appSettings["MainChannel"];
#if DEBUG
            //Name += "_";
            //User += "_";
            //RealName += "_";
            Channel = appSettings["TestChannel"];
#endif
        }
    }
}
