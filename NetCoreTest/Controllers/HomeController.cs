using Masuit.Tools.Core.NoSQL;
using Masuit.Tools.NoSQL;
using Microsoft.AspNetCore.Mvc;

namespace NetCoreTest.Controllers
{
    public class HomeController : Controller
    {
        public RedisHelper RedisHelper { get; set; }
        public HomeController(RedisHelperFactory redisHelper)
        {
            RedisHelper = redisHelper.Create("aa");
        }
        // GET
        public IActionResult Index()
        {
            RedisHelper.SetString("cc", 1);
            return Content("111");
        }
    }
}