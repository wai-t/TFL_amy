namespace tfl_stats.Server.Services.Cache
{
    public static class CacheKeys
    {
        public const string AllStopPoints = "allStopPoints";

        public static string Autocomplete(string query) =>
            $"autocomplete:{query.ToLower()}";
    }

}
