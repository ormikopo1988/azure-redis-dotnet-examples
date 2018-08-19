using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using StackExchange.Redis;
using System;
using System.Threading;

namespace Redis
{
    [TestClass]
    public class Test_10_KeysExpiration
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
        public void Demo_10_Expiration()
        {
            var cm = ConnectionMultiplexer.Connect(_redisConnectionString);

            var redis = cm.GetDatabase();

            string myKey = "myKey";
            string myValue = "myValue";

            redis.StringSet(myKey, myValue, TimeSpan.FromSeconds(3));

            var fetchedKey = redis.StringGet(myKey);

            Check.That(fetchedKey.HasValue).IsTrue();

            Thread.Sleep(3000);

            var fetchedKey2 = redis.StringGet(myKey);

            Check.That(fetchedKey2.HasValue).IsFalse();
        }
    }
}
