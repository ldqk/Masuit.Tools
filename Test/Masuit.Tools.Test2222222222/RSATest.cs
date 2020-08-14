using Masuit.Tools.Security;
using Xunit;

namespace Masuit.Tools.Test
{
    public class RSATest
    {
        [Fact]
        public void Can_Encrypt()
        {
            string publicKey = "<RSAKeyValue><Modulus>w1f5E3UyxPseWA0DKU6hpN/4WetQA8llIsO4YQx+m7wGzhbDTMmx7ScGZlyHAXYaSisUzNMIBJOIEsCtQsH/q2r7kOm7tQqnjYdaBgFrY/LnpsvVCS8mkQylbkdiZyusV09H3zcQ4KOwNUUm0m2wyBY5KoT3m8j1pcIk8id97SU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            string origin = "123456";
            string enc = origin.RSAEncrypt(publicKey);
            string dec = enc.RSADecrypt("<RSAKeyValue><Modulus>w1f5E3UyxPseWA0DKU6hpN/4WetQA8llIsO4YQx+m7wGzhbDTMmx7ScGZlyHAXYaSisUzNMIBJOIEsCtQsH/q2r7kOm7tQqnjYdaBgFrY/LnpsvVCS8mkQylbkdiZyusV09H3zcQ4KOwNUUm0m2wyBY5KoT3m8j1pcIk8id97SU=</Modulus><Exponent>AQAB</Exponent><P>0WSGgAK4Xan/2q0oNaVI8CRgLaD9xqK73EkKRumlC44fDTIcXE0vIaPsXeC8A+vHAFjDhC2MEbf30DW2sd2Y+w==</P><Q>7tLqNhL3zQ+8jblh17T9a+nbpQIG54igOlwc3jwa8kUm2rr+QRTUI5MP+diqQoT7z4o1oxFP6+Lsy2dXvv34Xw==</Q><DP>sgvWShcGCa65vYmrPSJUCM4FcgcIgtRxBPieYnndOxwXzzKi5uFCiEpIe/LSLEtZpTPU3BmWlqJld4eU11zj7Q==</DP><DQ>tDZJb6ZegMloIZWKxEeZl02vZVMjPKF3LrKFQhkeyEPwLss9woRiE7oMKx8YUvugPBpxoOwWX8wrnM0NhFyGhw==</DQ><InverseQ>S7ynBGzvO0HQ9+SKiBdz4Sgn7hntSJI7WcHZksfj3R9iN/L1pWVuMSdMg1fI1FUX9heciV2u1QMARYvUNeU5xQ==</InverseQ><D>JqpHFt7fybWa7/rDYW26+ROL6OB22gkHB7aNzEfY16KEBk7jIVPa8AIFdkViQ5vI4F1epJwwvhclm/CfWtNjc4XlhCo09koMnJPvxd0tdhnhtxbUG6gcEQOQUoQoVT7x9Evs33U78307qVT1j67FMUYW0Xanlu79a7Xgt+XyhRE=</D></RSAKeyValue>");
            Assert.Equal(dec, origin);
        }

        [Fact]
        public void Can_EncryptAuto()
        {
            string origin = "123456";
            string enc = origin.RSAEncrypt();
            string dec = enc.RSADecrypt();
            Assert.Equal(dec, origin);
        }
    }
}