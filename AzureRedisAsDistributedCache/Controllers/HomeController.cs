using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AzureRedisAsDistributedCache.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace AzureRedisAsDistributedCache.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDistributedCache _redis = null;

        public HomeController(IDistributedCache cache)
        {
            _redis = cache;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            // Lets suppose this data here is very expensive to retrieve every single time.
            // Maybe it is coming from a web service, or a slow database.
            // We might want to cache it because it might not change that often.
            var dataKey = "aspnet:{ormikopo}:bio";

            var bio = _redis.GetString(dataKey);

            if (string.IsNullOrEmpty(bio))
            {
                bio = "Software Engineer"; // here we call the "expensive" / "slow" webservice / db...

                // Once produced, save it to cache for next time
                _redis.SetString(dataKey, bio);
            }

            // Lets interact with the session state now
            var sessionKey = "ormikopo-session-key";

            var userMsg = string.Empty;

            HttpContext.Session.TryGetValue(sessionKey, out byte[] message);

            if (message == null)
            {
                userMsg = "hello redis session!";

                HttpContext.Session.Set(sessionKey, Encoding.UTF8.GetBytes("hello redis session!"));
            }
            else
            {
                userMsg = Encoding.UTF8.GetString(message);
            }

            ViewData["Message"] = $"About Orestis Meikopoulos: {bio} - {userMsg}";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
