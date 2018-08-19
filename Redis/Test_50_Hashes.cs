using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;

namespace Redis
{
    [TestClass]
    public class Test_50_Hashes
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

            _redis.StringSet("next_user_id", 0);
            _redis.StringSet("next_post_id", 0);
        }

        [TestMethod]
        public void Demo_50_Hashes()
        {
            var nextId = _redis.StringIncrement("next_user_id");

            var keyUserList = "SMB:Users";

            _redis.HashSet(keyUserList, new HashEntry[] 
            {
                new HashEntry("ormikopo", nextId)
            });

            var keyOrmikopo = "SMB:{ormikopo}:UserInformation";

            _redis.HashSet(keyOrmikopo, new HashEntry[] 
            {
                new HashEntry("emailAddress", "ormikopo@test.com"),
                new HashEntry("phoneNumber", "6999999999"),
                new HashEntry("id", nextId)
            });
        }
    }
}
