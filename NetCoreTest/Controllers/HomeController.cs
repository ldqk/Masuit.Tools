using Masuit.Tools.Core.NoSQL;
using Masuit.Tools.NoSQL;
using Microsoft.AspNetCore.Mvc;

namespace NetCoreTest.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(RedisHelperFactory redisHelper)
        {
            RedisHelper = redisHelper.Create("aa");
        }

        public RedisHelper RedisHelper { get; set; }

        // GET
        public IActionResult Index()
        {
            RedisHelper.SetString("cc", 1);
            return Content("111");
        }
    }
}
