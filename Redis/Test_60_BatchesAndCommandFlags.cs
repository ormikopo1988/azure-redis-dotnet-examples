using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;

namespace Redis
{
    [TestClass]
    public class Test_60_BatchesAndCommandFlags
    {
        private static string _redisConnectionString = null;

        private static TestContext _context = null;

        private static IDatabase _redis = null;

        [ClassInitialize]
        public static void Class_Init(TestContext context)
        {
            _context = context;

            _redisConnectionString = _context.Properties["AzureRedisPrimaryConnectionString"] as string;
            
            var configuration = ConfigurationOptions.Parse(_redisConnectionString);

            configuration.AllowAdmin = true;

            var connectionMultiplexer = ConnectionMultiplexer.Connect(configuration);

            var server = connectionMultiplexer.GetServer(_context.Properties["AzureRedisHostNameWithPort"] as string);

            server.FlushDatabase(); // Clear the redis cache in order for all tests to run with a clear state

            _redis = connectionMultiplexer.GetDatabase();
        }

        [TestMethod]
        public void Demo_60_CommandFlags()
        {
            for (int i = 0; i < 100; i++)
            {
                var userKey = $"User{i+1}";

                _redis.HashSet(userKey, new HashEntry[]
                {
                    new HashEntry("emailAddress", $"user{i+1}@test.com"),
                    new HashEntry("id", (i+1).ToString())
                }, CommandFlags.FireAndForget); // here we explicitly say send all the requests in a fire and forget manner - dont wait for a response - this improves the speed significatly
            }
        }

        [TestMethod]
        public void Demo_61_Batches()
        {
            var batch = _redis.CreateBatch();

            for (int i = 0; i < 100; i++)
            {
                var userKey = $"User{i + 1}";

                batch.HashSetAsync(userKey, new HashEntry[]
                {
                    new HashEntry("emailAddress", $"user{i+1}@test.com"),
                    new HashEntry("id", (i+1).ToString())
                });
            }

            batch.Execute(); // Now go over the network with the batch to send to redis with all the commands as one
        }
    }
}
