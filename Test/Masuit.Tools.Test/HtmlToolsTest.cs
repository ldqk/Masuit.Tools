using Masuit.Tools.Html;
using Xunit;

namespace Masuit.Tools.Test
{
    public class HtmlToolsTest
    {
        [Theory]
        [InlineData("<p><img src=\"https://ww1.sinaimg.cn/large/007iUjdily1fxefem7juyj30ld037q35\"/></p><p><img src=\"https://ww1.sinaimg.cn/large/007iUjdily1fxeffei83dj30l90nbgr8\"/></p><p><img src=\"https://ww1.sinaimg.cn/large/007iUjdily1fxeffz9u1kj30lc0n90y1\"/></p><p><img src=\"https://ww1.sinaimg.cn/large/007iUjdily1fxefgh6xxoj30lc09aq4q\"/></p>")]
        [InlineData("<p><img title=\"title\" src=\"https://ww1.sinaimg.cn/large/007iUjdily1fxefem7juyj30ld037q35\"/></p><p><img title=\"title\" src=\"https://ww1.sinaimg.cn/large/007iUjdily1fxeffei83dj30l90nbgr8\"/></p><p><img src=\"https://ww1.sinaimg.cn/large/007iUjdily1fxeffz9u1kj30lc0n90y1\"/></p><p><img title=\"title\" src=\"https://ww1.sinaimg.cn/large/007iUjdily1fxefgh6xxoj30lc09aq4q\"/></p>")]
        public void MatchRandomImgSrc_NormalHtml_ReturnAImgUrl(string html)
        {
            string src = html.MatchRandomImgSrc();
            Assert.True(src.StartsWith("http"));
        }

        [Fact]
        public void MatchRandomImgSrc_NoImgHtml_ReturnNull()
        {
            string html = "<p>如果你是通过某宝、或是其他论坛等渠道，通过任何付费的方式获取到本资源，请直接退款并在相应的平台举报，如果是论坛的，请将本页链接分享到你购买源的评论区，告诫他人谨防上当，或者直接将本页链接转发分享给有需要的人，感谢您的支持和监督。</p>";
            string src = html.MatchRandomImgSrc();
            Assert.True(string.IsNullOrEmpty(src));
        }
    }
}