using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using StackExchange.Redis;

namespace Redis
{
    [TestClass]
    public class Test_30_Lists
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
        public void Demo_30_Lists()
        {
            var keyUserList = "SMB:Users";

            _redis.ListLeftPush(keyUserList, new RedisValue[] { "ormikopo" });

            var keyOrmikopoPosts = "SMB:{ormikopo}:Posts"; // the part in {} will be the one that changes to identify the user

            RedisValue[] myValues = new RedisValue[] { "hello world", "welcome to short blog post message one" };

            _redis.ListLeftPush(
                keyOrmikopoPosts, 
                myValues
            );

            var numberOfPosts = _redis.ListLength(keyOrmikopoPosts);

            Check.That(numberOfPosts).IsEqualTo(2);

            var allPosts = _redis.ListRange(keyOrmikopoPosts, 0, -1);

            Check.That(allPosts.Length).IsEqualTo(2);
        }
    }
}
