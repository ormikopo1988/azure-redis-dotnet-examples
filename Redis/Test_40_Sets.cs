using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using StackExchange.Redis;
using System;

namespace Redis
{
    [TestClass]
    public class Test_40_Sets
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
        public void Demo_40_Sets()
        {
            var keyOrmikopoFollowing = "SMB:{ormikopo}:Following";

            _redis.SetAdd(keyOrmikopoFollowing, new RedisValue[] { "UserA", "UserB" });

            var userOrmikopoFollowingCount = _redis.SetLength(keyOrmikopoFollowing);

            Check.That(userOrmikopoFollowingCount).IsEqualTo(2);

            // That means that UserA and UserB Followers must also be updated
            _redis.SetAdd("SMB:{UserA}:Followers", "ormikopo");
            _redis.SetAdd("SMB:{UserB}:Followers", "ormikopo");

            var keyOrmikopoFollowers = "SMB:{ormikopo}:Followers";

            _redis.SetAdd(keyOrmikopoFollowers, new RedisValue[] { "UserC", "UserD" });

            var userOrmikopoFollowersCount = _redis.SetLength(keyOrmikopoFollowers);

            Check.That(userOrmikopoFollowersCount).IsEqualTo(2);

            // That means that UserC and UserD Following must also be updated
            _redis.SetAdd("SMB:{UserC}:Following", "ormikopo");
            _redis.SetAdd("SMB:{UserD}:Following", "ormikopo");
        }

        [TestMethod]
        public void Demo_41_OrderedSets()
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

            // Dear redis get me the timeline of the two sets combined
            _redis.SortedSetCombineAndStore(
                SetOperation.Union,
                "homepage", // new key for the new set that will be created
                keyOrmikopoPosts,
                keyUserXPosts
            );

            var homePagePostCount = _redis.SortedSetLength("homepage");

            Check.That(homePagePostCount).IsEqualTo(4);

            RedisValue[] homePagePosts = _redis.SortedSetRangeByRank("homepage", 0, 1);

            Check.That(homePagePosts.Count()).IsEqualTo(2);
        }
    }
}
