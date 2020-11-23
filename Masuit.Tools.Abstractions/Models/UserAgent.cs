using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Common
{
    public class UserAgent
    {
        private static readonly IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions()
        {
            SizeLimit = 10000
        });

        internal static readonly Dictionary<string, string> Platforms = new Dictionary<string, string>() {
            {"windows nt 10.0", "Windows 10"},
            {"windows nt 6.3", "Windows 8.1"},
            {"windows nt 6.2", "Windows 8"},
            {"windows nt 6.1", "Windows 7"},
            {"windows nt 6.0", "Windows Vista"},
            {"windows nt 5.2", "Windows 2003"},
            {"windows nt 5.1", "Windows XP"},
            {"windows nt 5.0", "Windows 2000"},
            {"windows nt 4.0", "Windows NT 4.0"},
            {"winnt4.0", "Windows NT 4.0"},
            {"winnt 4.0", "Windows NT"},
            {"winnt", "Windows NT"},
            {"windows 98", "Windows 98"},
            {"win98", "Windows 98"},
            {"windows 95", "Windows 95"},
            {"win95", "Windows 95"},
            {"windows phone", "Windows Phone"},
            {"windows", "Unknown Windows OS"},
            {"android", "Android"},
            {"blackberry", "BlackBerry"},
            {"iphone", "iOS"},
            {"ipad", "iOS"},
            {"ipod", "iOS"},
            {"os x", "Mac OS X"},
            {"ppc mac", "Power PC Mac"},
            {"freebsd", "FreeBSD"},
            {"ppc", "Macintosh"},
            {"linux", "Linux"},
            {"debian", "Debian"},
            {"sunos", "Sun Solaris"},
            {"beos", "BeOS"},
            {"apachebench", "ApacheBench"},
            {"aix", "AIX"},
            {"irix", "Irix"},
            {"osf", "DEC OSF"},
            {"hp-ux", "HP-UX"},
            {"netbsd", "NetBSD"},
            {"bsdi", "BSDi"},
            {"openbsd", "OpenBSD"},
            {"gnu", "GNU/Linux"},
            {"unix", "Unknown Unix OS"},
            {"symbian", "Symbian OS"},
        };

        internal static readonly Dictionary<string, string> Browsers = new Dictionary<string, string>()
        {
            {"OPR", "Opera"},
            {"Flock", "Flock"},
            {"Edge", "Spartan"},
            {"MQQ", "手机QQ浏览器"},
            {"QQ", "QQ浏览器"},
            {"MicroMessenger", "微信内置浏览器"},
            {"Baidu", "百度浏览器"},
            {"Chrome", "Chrome"},
            {"Opera.*?Version", "Opera"},
            {"Opera", "Opera"},
            {"MSIE", "Internet Explorer"},
            {"Internet Explorer", "Internet Explorer"},
            {"Trident.* rv" , "Internet Explorer"},
            {"Shiira", "Shiira"},
            {"Firefox", "Firefox"},
            {"Chimera", "Chimera"},
            {"Phoenix", "Phoenix"},
            {"Firebird", "Firebird"},
            {"Camino", "Camino"},
            {"Netscape", "Netscape"},
            {"OmniWeb", "OmniWeb"},
            {"Safari", "Safari"},
            {"Mozilla", "Mozilla"},
            {"Konqueror", "Konqueror"},
            {"icab", "iCab"},
            {"Lynx", "Lynx"},
            {"Links", "Links"},
            {"hotjava", "HotJava"},
            {"amaya", "Amaya"},
            {"IBrowse", "IBrowse"},
            {"Maxthon", "Maxthon"},
            {"Ubuntu", "Ubuntu Web Browser"},
        };

        internal static readonly Dictionary<string, string> Mobiles = new Dictionary<string, string>()
        {
            // Legacy
            {"mobileexplorer", "Mobile Explorer"},
            {"palmsource", "Palm"},
            {"palmscape", "Palmscape"},
            // Phones and Manufacturers
            {"motorola", "Motorola"},
            {"nokia", "Nokia"},
            {"palm", "Palm"},
            {"iphone", "Apple iPhone"},
            {"ipad", "iPad"},
            {"ipod", "Apple iPod Touch"},
            {"sony", "Sony Ericsson"},
            {"ericsson", "Sony Ericsson"},
            {"blackberry", "BlackBerry"},
            {"cocoon", "O2 Cocoon"},
            {"blazer", "Treo"},
            {"lg", "LG"},
            {"amoi", "Amoi"},
            {"xda", "XDA"},
            {"mda", "MDA"},
            {"vario", "Vario"},
            {"htc", "HTC"},
            {"samsung", "Samsung"},
            {"sharp", "Sharp"},
            {"sie-", "Siemens"},
            {"alcatel", "Alcatel"},
            {"benq", "BenQ"},
            {"ipaq", "HP iPaq"},
            {"mot-", "Motorola"},
            {"playstation portable", "PlayStation Portable"},
            {"playstation 3", "PlayStation 3"},
            {"playstation vita", "PlayStation Vita"},
            {"hiptop", "Danger Hiptop"},
            {"nec-", "NEC"},
            {"panasonic", "Panasonic"},
            {"philips", "Philips"},
            {"sagem", "Sagem"},
            {"sanyo", "Sanyo"},
            {"spv", "SPV"},
            {"zte", "ZTE"},
            {"sendo", "Sendo"},
            {"nintendo dsi", "Nintendo DSi"},
            {"nintendo ds", "Nintendo DS"},
            {"nintendo 3ds", "Nintendo 3DS"},
            {"wii", "Nintendo Wii"},
            {"open web", "Open Web"},
            {"openweb", "OpenWeb"},
            {"vivo", "Vivo"},
            {"oppo", "OPPO"},
            {"xiaomi", "小米"},
            {"miui", "小米"},
            {"SKR-", "小米黑鲨"},
            {"huawei", "华为"},
            {"HONOR", "华为荣耀"},
            {"ONEPLUS", "一加"},
            {"GM19", "一加"},
            {"Nexus", "Nexus"},
            {"ASUS", "ASUS"},

            // Operating Systems
            {"android", "Android"},
            {"symbian", "Symbian"},
            {"SymbianOS", "SymbianOS"},
            {"elaine", "Palm"},
            {"series60", "Symbian S60"},
            {"windows ce", "Windows CE"},
            // Browsers
            {"obigo", "Obigo"},
            {"netfront", "Netfront Browser"},
            {"openwave", "Openwave Browser"},
            {"mobilexplorer", "Mobile Explorer"},
            {"operamini", "Opera Mini"},
            {"opera mini", "Opera Mini"},
            {"opera mobi", "Opera Mobile"},
            {"fennec", "Firefox Mobile"},
            // Other
            {"digital paths", "Digital Paths"},
            {"avantgo", "AvantGo"},
            {"xiino", "Xiino"},
            {"novarra", "Novarra Transcoder"},
            {"vodafone", "Vodafone"},
            {"docomo", "NTT DoCoMo"},
            {"o2", "O2"},
            // Fallback
            {"mobile", "Generic Mobile"},
            {"wireless", "Generic Mobile"},
            {"j2me", "Generic Mobile"},
            {"midp", "Generic Mobile"},
            {"cldc", "Generic Mobile"},
            {"up.link", "Generic Mobile"},
            {"up.browser", "Generic Mobile"},
            {"smartphone", "Generic Mobile"},
            {"cellphone", "Generic Mobile"},
        };

        internal static readonly Dictionary<string, string> Robots = new Dictionary<string, string>()
        {
            {"googlebot", "Googlebot"},
            {"msnbot", "MSNBot"},
            {"baiduspider", "Baiduspider"},
            {"bingbot", "Bing"},
            {"slurp", "Inktomi Slurp"},
            {"yahoo", "Yahoo"},
            {"ask jeeves", "Ask Jeeves"},
            {"fastcrawler", "FastCrawler"},
            {"infoseek", "InfoSeek Robot 1.0"},
            {"lycos", "Lycos"},
            {"yandex", "YandexBot"},
            {"mediapartners-google", "MediaPartners Google"},
            {"CRAZYWEBCRAWLER", "Crazy Webcrawler"},
            {"adsbot-google", "AdsBot Google"},
            {"feedfetcher-google", "Feedfetcher Google"},
            {"curious george", "Curious George"},
            {"ia_archiver", "Alexa Crawler"},
            {"MJ12bot", "Majestic-12"},
            {"Uptimebot", "Uptimebot"},
            {"Sogou web spider", "Sogou Web Spider"},
            {"TelegramBot", "Telegram Bot"},
            {"DNSPod", "DNSPod"}
        };

        protected string agent;
        public bool IsBrowser { get; set; }
        public bool IsRobot { get; set; }
        public bool IsMobile { get; set; }

        // Current values
        public string Platform { get; set; } = "";
        public string Browser { get; set; } = "";
        public string BrowserVersion { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string Robot { get; set; } = "";

        internal UserAgent(string userAgentString = null)
        {
            if (userAgentString != null)
            {
                agent = userAgentString.Length > 512 ? userAgentString[..512] : userAgentString;
                SetPlatform();
                if (SetRobot()) return;
                if (SetBrowser()) return;
            }
        }

        internal bool SetPlatform()
        {
            foreach (var item in Platforms.Where(item => Regex.IsMatch(agent, $"{Regex.Escape(item.Key)}", RegexOptions.IgnoreCase)))
            {
                Platform = item.Value;
                return true;
            }
            Platform = "Unknown Platform";
            return false;
        }

        internal bool SetBrowser()
        {
            foreach (var item in Browsers)
            {
                var match = Regex.Match(agent, $@"{item.Key}.*?([0-9\.]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    IsBrowser = true;
                    BrowserVersion = match.Groups[1].Value;
                    Browser = item.Value;
                    SetMobile();
                    return true;
                }
            }
            return false;
        }

        internal bool SetRobot()
        {
            foreach (var item in Robots.Where(item => Regex.IsMatch(agent, $"{Regex.Escape(item.Key)}", RegexOptions.IgnoreCase)))
            {
                IsRobot = true;
                Robot = item.Value;
                SetMobile();
                return true;
            }

            return false;
        }

        internal bool SetMobile()
        {
            foreach (var item in Mobiles.Where(item => agent.IndexOf(item.Key, StringComparison.OrdinalIgnoreCase) != -1))
            {
                IsMobile = true;
                Mobile = item.Value;
                return true;
            }

            return false;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return agent;
        }

        public static UserAgent Parse(string userAgentString)
        {
            return Cache.GetOrCreate(userAgentString, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                entry.Size = 1;
                return new UserAgent(userAgentString);
            });
        }
    }
}