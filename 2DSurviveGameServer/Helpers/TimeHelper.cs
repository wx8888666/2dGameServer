namespace _2DSurviveGameServer.Helpers
{
    public class TimeHelper
    {
        private static readonly DateTime utcStart = new DateTime(1970, 1, 1);

        public static long GetUTCStartMilliseconds()
        {
            return (long)(DateTime.UtcNow - utcStart).TotalMilliseconds;
        }
    }
}
