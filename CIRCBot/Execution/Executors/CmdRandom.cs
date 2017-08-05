using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIRCBot.Execution.Executors
{
    [ClassName("Random", "Satunnais linkkejä")]
    class CmdRandom : BaseExecutor, IExecutor
    {

        #region Constants

        private const string Wiki = "http://fi.wikipedia.org/wiki/Toiminnot:Satunnainen_sivu";

        private const string Wictionary = "http://fi.wiktionary.org/wiki/Toiminnot:Satunnainen_sivu";

        private const string DailyWord = "http://www.lexisrex.com/Finnish/Daily-Word";

        private const string UrbanDictionary = "http://urbaanisanakirja.com/random/";

        private const string Hikipedia = "http://hikipedia.info/wiki/Toiminnot:Satunnainen_sivu";

        private const string Reddit = "https://www.reddit.com/r/random/";

        #endregion Constants

        public CmdRandom(BotMessager messager) : base(messager)
        {
            Add("Satunnainen runo", cmd_poem, "runo", "poem").TimerLocked(2);
            Add("Satunnainen wiki-linkki", cmd_wiki, "wiki");
            Add("Satunnainen wictionary-linkki", cmd_wictionary, "sana", "wictionary", "wiktionary");
            Add("Päivän sana", cmd_dailyWord, "päivänsana");
            Add("Satunnainen paskanhauska kuva", cmd_funnyShit, "topkek").TimerLocked(2);
            Add("Satunnainen paskanhauska gif", cmd_funnyGif, "gif").TimerLocked(2);
            Add("Satunnainen urbaanin sanakirjan-linkki", cmd_urbanDictionary, "urbaanisanakirja", "urbandictionary", "urban");
            Add("Satunnainen hikipedia-linkki", cmd_hikipedia, "hiki", "hikipedia");
            Add("Satunnainen sub-reddit", cmd_reddit, "reddit", "r");
        }

        public new void Execute(Msg message)
        {
            base.Execute(message);

            LogResults(RunCommand());
        }

        #region Commands

        private void cmd_poem()
        {
            var WP = new Apis.WebParser();

            string poem = WP.GetGeneratedHTML("http://www.runosydan.net/random.php", "<P class=poem>");

            poem = poem.Substring(0, poem.IndexOf("<P"));

            poem = poem.Replace("<BR>", " ");

            Bot.Say(poem);
        }

        private void cmd_funnyShit()
        {
            var WP = new Apis.WebParser();

            string img = WP.GetGeneratedHTML("http://www.funcage.com/", "/photos/", "\"");

            string url = "http://www.funcage.com/photos/" + img;

            Bot.Say(url);
        }

        private void cmd_funnyGif()
        {
            var WP = new Apis.WebParser();

            string img = WP.GetGeneratedHTML("http://www.funcage.com/gif/", "/photos/", "\"");

            string url = "http://www.funcage.com/gif/photos/" + img;

            Bot.Say(url);
        }

        private void cmd_dailyWord()
        {
            Link(DailyWord);
        }

        private void cmd_wictionary()
        {
            Link(Wictionary);
        }

        private void cmd_wiki()
        {
            Link(Wiki);
        }

        private void cmd_urbanDictionary()
        {
            Link(UrbanDictionary);
        }

        private void cmd_hikipedia()
        {
            Link(Hikipedia);
        }

        private void cmd_reddit()
        {
            Link(Reddit);
        }

        #endregion Commands

        #region Private methods

        private void Link(string url)
        {
            Bot.Say(GM.GetRedirectResult(url));
        }

        #endregion Private methods

    }
}
