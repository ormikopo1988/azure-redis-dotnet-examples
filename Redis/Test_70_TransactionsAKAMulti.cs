using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using StackExchange.Redis;
using System;

namespace Redis
{
    [TestClass]
    public class Test_70_TransactionsAkaMulti
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
        public void Demo_70_TransactionsAKAMulti()
        {
            // Atomic Operations in Redis
            _redis.StringSet("key1", 5, when: When.NotExists);

            _redis.StringIncrement("key1", 5);

            // Another way to control the way the operations are going to execute on Redis server is by using transactions
            var transaction = _redis.CreateTransaction(); // equivalent to MULTI statement from cli

            transaction.StringIncrementAsync("keyA"); // here we just take a confirmation that the command is queued on the server
            transaction.StringIncrementAsync("keyB");

            // equivalent to EXEC statement from cli
            transaction.Execute(); // Now it is guaranteed that the two increments on keyA and keyB will happen together
        }

        [TestMethod]
        public void Demo_71_HomePage()
        {
            var today = DateTime.UtcNow.Ticks;
            var yesterday = DateTime.UtcNow.AddDays(-1).Ticks;

            var keyOrmikopoPosts = "SMB:{ormikopo}:Posts";

            _redis.SortedSetAdd(keyOrmikopoPosts, new SortedSetEntry[]
            {
                new SortedSetEntry("hello world", yesterday),
                new SortedSetEntry("welcome to SMB service", today)
            });

            var keyUserXPosts = "SMB:{userX}:Posts";

            _redis.SortedSetAdd(keyUserXPosts, new SortedSetEntry[]
            {
                new SortedSetEntry("userX says hello world", yesterday + 1), // + 1 to model that userX posted that a little after of ormikopo
                new SortedSetEntry("userX says welcome to SMB service", today + 1)
            });

            //// Dear redis get me the timeline of the two sets combined
            var homePagePosts = _redis.SortedSetRangeByRank("homepage", 0, -1);

            if(homePagePosts.Length == 0)
            {
                var transaction = _redis.CreateTransaction(); // MULTI

                transaction.AddCondition(Condition.KeyNotExists("homepage")); // WATCH - Optimistic Concurrency

                transaction.SortedSetCombineAndStoreAsync(
                    SetOperation.Union,
                    "homepage", // new key for the new set that will be created
                    keyOrmikopoPosts,
                    keyUserXPosts
                );

                transaction.SortedSetRemoveRangeByRankAsync("homepage", 0, -3); // Keep the TOP 2 posts

                transaction.Execute(); // EXEC
            }

            homePagePosts = _redis.SortedSetRangeByRank("homepage", 0, -1);

            Check.That(homePagePosts.Length).IsStrictlyGreaterThan(0);
        }
    }
}
