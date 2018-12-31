using Masuit.Tools.Core.NoSQL;
using Masuit.Tools.Files;
using Masuit.Tools.NoSQL;
using Microsoft.AspNetCore.Mvc;

namespace NetCoreTest.Controllers
{
    public class HomeController : Controller
    {
        public RedisHelper RedisHelper { get; set; }
        public HomeController(RedisHelperFactory redisHelperFactory)
        {
            RedisHelper=redisHelperFactory.Create("aa",0);// 创建命名为aa的RedisHelper，指定数据库0
            RedisHelper=redisHelperFactory.CreateDefault(0); // 创建默认的RedisHelper，指定数据库0
            RedisHelper=redisHelperFactory.CreateLocal(0); // 创建连接本机的RedisHelper，指定数据库0
        }
    }
}
