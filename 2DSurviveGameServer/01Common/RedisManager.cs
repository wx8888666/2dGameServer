using StackExchange.Redis;

namespace _2DSurviveGameServer._01Common
{
    public class RedisManager
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect("localhost:6379"); // 确保 Redis 服务器正在 localhost 的 6379 端口上运行
        });

        public static ConnectionMultiplexer Connection => lazyConnection.Value;

        private RedisManager() { }

        public static IDatabase GetDatabase() => Connection.GetDatabase();
    }
}
