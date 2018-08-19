using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System.Net;

namespace Redis
{
    [TestClass]
    public class Test_01_ConnectTo
    {
        private static string _config = null;

        private static TestContext _context = null;

        [ClassInitialize]
        public static void Class_Init(TestContext context)
        {
            _context = context;
            _config = _context.Properties["AzureRedisPrimaryConnectionString"] as string;
        }

        [TestMethod]
        public void Demo_01_Connect()
        {
            var ipAddresses = Dns.GetHostAddresses(_context.Properties["AzureRedisHostName"] as string);
            var port = 6380; // SSL

            var configuration = new ConfigurationOptions
            {
                EndPoints = { new IPEndPoint(ipAddresses[0], port) },
                Ssl = true,
                Password = _context.Properties["AzureRedisPrimaryKey"] as string, // AUTH command
                ClientName = nameof(Test_01_ConnectTo), // for debugging
                AllowAdmin = true,
                AbortOnConnectFail = false
            };

            var cm = ConnectionMultiplexer.Connect(configuration);

            var redis = cm.GetDatabase();
        }
    }
}
