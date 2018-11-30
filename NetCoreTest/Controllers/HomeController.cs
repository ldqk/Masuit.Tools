using Masuit.Tools.Files;
using Microsoft.AspNetCore.Mvc;

namespace NetCoreTest.Controllers
{
    public class HomeController : Controller
    {
        public void Zip()
        {
            ClassZip.Zip(@"H:\Dism++", @"H:\1.zip", 9);
        }
    }
}
