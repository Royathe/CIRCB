namespace CIRCBot.Apis.Repositories
{
    public static class RepositoryLoader
    {
        public static void Load()
        {
            Cities.Load();
            WeatherConditions.Load();
        }
    }
}
