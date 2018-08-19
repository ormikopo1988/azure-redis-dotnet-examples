using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using StackExchange.Redis;

namespace Redis
{
    [TestClass]
    public class Test_20_KeyOperations
    {
        private static string _redisConnectionString = null;

        private static TestContext _context = null;

        [ClassInitialize]
        public static void Class_Init(TestContext context)
        {
            _context = context;
            _redisConnectionString = _context.Properties["AzureRedisPrimaryConnectionString"] as string;
        }

        [TestMethod]
        public void Demo_20_KeyExists()
        {
            var cm = ConnectionMultiplexer.Connect(_redisConnectionString);

            var redis = cm.GetDatabase();

            string myKey = "myKey";
            string myValue = "myValue";

            redis.StringSet(myKey, myValue, null, When.NotExists); // NX - Set only when the key does not already exist

            var fetchedKey = redis.StringGet(myKey);

            Check.That(fetchedKey.HasValue).IsTrue();
            Check.That(fetchedKey.ToString()).IsEqualTo(myValue);

            redis.StringSet(myKey, "ABC", null, When.NotExists);

            var fetchedKey2 = redis.StringGet(myKey); // this should not change the original value

            Check.That(fetchedKey2.HasValue).IsTrue();
            Check.That(fetchedKey2.ToString()).IsEqualTo(myValue);
        }

        [TestMethod]
        public void Demo_20_KeyIncrement()
        {
            var cm = ConnectionMultiplexer.Connect(_redisConnectionString);

            var redis = cm.GetDatabase();

            string myKey = "myKey";
            int myValue = 5;

            redis.StringSet(myKey, myValue);
            
            redis.StringIncrement(myKey); // Process this value SERVER-SIDE on Redis

            var fetchedKey = (int)redis.StringGet(myKey);

            Check.That(fetchedKey).IsEqualTo(6);
        }
    }
}
