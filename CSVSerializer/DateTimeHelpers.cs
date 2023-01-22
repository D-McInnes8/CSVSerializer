namespace CSVSerializer
{
    public static class DateTimeHelpers
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToEpochTime(DateTime date)
        {
            return (long)Math.Round((date - _epoch).TotalSeconds, 0);
        }

        public static DateTime FromEpochTime(long seconds)
        {
            return _epoch.AddSeconds(seconds);
        }
    }
}
