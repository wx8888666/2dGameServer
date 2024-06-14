using StackExchange.Redis;

namespace _2DSurviveGameServer._01Common
{
    public static class RedisManager
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var config = ConfigurationOptions.Parse("localhost"); // 根据你的 Redis 服务器地址修改
            config.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(config);
        });

        public static ConnectionMultiplexer Connection => lazyConnection.Value;

        public static IDatabase GetDatabase()
        {
            return Connection.GetDatabase();
        }
    }
}
