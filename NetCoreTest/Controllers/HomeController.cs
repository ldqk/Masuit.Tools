using Masuit.Tools.Security;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace NetCoreTest.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("rsaenc")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Rsa(string str)
        {
            var rsaKey = RsaCrypt.GenerateRsaKeys();
            var enc = str.RSAEncrypt();
            var dec = enc.RSADecrypt();
            return Ok(dec);
        }
    }
}
