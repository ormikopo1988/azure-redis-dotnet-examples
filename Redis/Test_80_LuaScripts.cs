using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using StackExchange.Redis;

namespace Redis
{
    [TestClass]
    public class Test_80_LuaScripts
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
        public void Demo_80_LuaSimpleScript()
        {
            string myLuaScript = " local name='ormikopo' return name ";

            var ls = LuaScript.Prepare(myLuaScript);

            _redis.ScriptEvaluate(ls);
        }

        [TestMethod]
        public void Demo_81_LuaScriptWithArguments()
        {
            string myLuaScript = " local name=ARGV[1] return name ";
            
            var result = _redis.ScriptEvaluate(
                myLuaScript,
                new RedisKey[] { },
                new RedisValue[] { "ormikopo" }
            );

            Check.That(result.ToString()).IsEqualTo("ormikopo");
        }

        [TestMethod]
        public void Demo_82_LuaScriptWithCallsToRedis()
        {
            string myLuaScript = @" 
                redis.call('SET', KEYS[1], ARGV[1])
                redis.call('SET', KEYS[2], ARGV[2])
                local firstname=redis.call('GET', KEYS[1]) 
                local lastname=redis.call('GET', KEYS[2])
                return firstname..' '..lastname
            ";

            var result = _redis.ScriptEvaluate(
                myLuaScript,
                new RedisKey[] { "keyFn", "keyLn" },
                new RedisValue[] { "Orestis", "Meikopoulos" }
            );

            Check.That(result.ToString()).IsEqualTo("Orestis Meikopoulos");
        }

        [TestMethod]
        public void Demo_83_LuaScriptWithParameterizedQueriesForCleanerScripts()
        {
            string myLuaScript = @" 
                local firstname=@FirstName 
                local lastname=@LastName
                return firstname..' '..lastname
            ";

            var ls = LuaScript.Prepare(myLuaScript);

            var result = _redis.ScriptEvaluate(
                ls,
                new
                {
                    FirstName = "Orestis", 
                    LastName = "Meikopoulos" 
                }
            );

            Check.That(result.ToString()).IsEqualTo("Orestis Meikopoulos");
        }
    }
}
