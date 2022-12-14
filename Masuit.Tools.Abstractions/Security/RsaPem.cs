using Masuit.Tools.Systems;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Security
{
    /// <summary>
    /// RSA PEM格式密钥对的解析和导出
    /// </summary>
    public class RsaPem
    {
        /// <summary>
        /// modulus 模数n，公钥、私钥都有
        /// </summary>
        public byte[] KeyModulus;

        /// <summary>
        /// publicExponent 公钥指数e，公钥、私钥都有
        /// </summary>
        public byte[] KeyExponent;

        /// <summary>
        /// privateExponent 私钥指数d，只有私钥的时候才有
        /// </summary>
        public byte[] KeyD;

        //以下参数只有私钥才有 https://docs.microsoft.com/zh-cn/dotnet/api/system.security.cryptography.rsaparameters?redirectedfrom=MSDN&view=netframework-4.8
        /// <summary>
        /// prime1
        /// </summary>
        public byte[] ValP;

        /// <summary>
        /// prime2
        /// </summary>
        public byte[] ValQ;

        /// <summary>
        /// exponent1
        /// </summary>
        public byte[] ValDp;

        /// <summary>
        /// exponent2
        /// </summary>
        public byte[] ValDq;

        /// <summary>
        /// coefficient
        /// </summary>
        public byte[] ValInverseQ;

        private RsaPem()
        {
        }

        /// <summary>
        /// 通过RSA中的公钥和私钥构造一个PEM，如果convertToPublic含私钥的RSA将只读取公钥，仅含公钥的RSA不受影响
        /// </summary>
        public RsaPem(RSACryptoServiceProvider rsa, bool convertToPublic = false)
        {
            var isPublic = convertToPublic || rsa.PublicOnly;
            var param = rsa.ExportParameters(!isPublic);
            KeyModulus = param.Modulus;
            KeyExponent = param.Exponent;

            if (!isPublic)
            {
                KeyD = param.D;
                ValP = param.P;
                ValQ = param.Q;
                ValDp = param.DP;
                ValDq = param.DQ;
                ValInverseQ = param.InverseQ;
            }
        }

        /// <summary>
        /// 通过全量的PEM字段数据构造一个PEM，除了模数modulus和公钥指数exponent必须提供外，其他私钥指数信息要么全部提供，要么全部不提供（导出的PEM就只包含公钥）
        /// 注意：所有参数首字节如果是0，必须先去掉
        /// </summary>
        public RsaPem(byte[] modulus, byte[] exponent, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] inverseQ)
        {
            KeyModulus = modulus;
            KeyExponent = exponent;
            KeyD = d;

            ValP = p;
            ValQ = q;
            ValDp = dp;
            ValDq = dq;
            ValInverseQ = inverseQ;
        }

        /// <summary>
        /// 通过公钥指数和私钥指数构造一个PEM，会反推计算出P、Q但和原始生成密钥的P、Q极小可能相同
        /// 注意：所有参数首字节如果是0，必须先去掉
        /// 出错将会抛出异常
        /// </summary>
        /// <param name="modulus">必须提供模数</param>
        /// <param name="exponent">必须提供公钥指数</param>
        /// <param name="dOrNull">私钥指数可以不提供，导出的PEM就只包含公钥</param>
        public RsaPem(byte[] modulus, byte[] exponent, byte[] dOrNull)
        {
            KeyModulus = modulus; //modulus
            KeyExponent = exponent; //publicExponent

            if (dOrNull != null)
            {
                KeyD = dOrNull; //privateExponent

                //反推P、Q
                BigInteger n = BigX(modulus);
                BigInteger e = BigX(exponent);
                BigInteger d = BigX(dOrNull);
                BigInteger p = FindFactor(e, d, n);
                BigInteger q = n / p;
                if (p.CompareTo(q) > 0)
                {
                    (p, q) = (q, p);
                }

                BigInteger exp1 = d % (p - BigInteger.One);
                BigInteger exp2 = d % (q - BigInteger.One);
                BigInteger coeff = BigInteger.ModPow(q, p - 2, p);

                ValP = BigB(p); //prime1
                ValQ = BigB(q); //prime2
                ValDp = BigB(exp1); //exponent1
                ValDq = BigB(exp2); //exponent2
                ValInverseQ = BigB(coeff); //coefficient
            }
        }

        /// <summary>
        /// 密钥位数
        /// </summary>
        public int KeySize => KeyModulus.Length * 8;

        /// <summary>
        /// 是否包含私钥
        /// </summary>
        public bool HasPrivate => KeyD != null;

        /// <summary>
        /// 将PEM中的公钥私钥转成RSA对象，如果未提供私钥，RSA中就只包含公钥
        /// </summary>
        public RSACryptoServiceProvider GetRSA()
        {
            //var rsaParams = System.Security.Cryptography.RSA.Create();
            //rsaParams.Flags = CspProviderFlags.UseMachineKeyStore;
            var rsa = new RSACryptoServiceProvider();

            var param = new RSAParameters
            {
                Modulus = KeyModulus,
                Exponent = KeyExponent
            };
            if (KeyD != null)
            {
                param.D = KeyD;
                param.P = ValP;
                param.Q = ValQ;
                param.DP = ValDp;
                param.DQ = ValDq;
                param.InverseQ = ValInverseQ;
            }

            rsa.ImportParameters(param);
            return rsa;
        }

        /// <summary>
        /// 转成正整数，如果是负数，需要加前导0转成正整数
        /// </summary>
        public static BigInteger BigX(byte[] bigb)
        {
            if (bigb[0] > 127)
            {
                byte[] c = new byte[bigb.Length + 1];
                Array.Copy(bigb, 0, c, 1, bigb.Length);
                bigb = c;
            }

            return new BigInteger(bigb.Reverse().ToArray()); //C#的二进制是反的
        }

        /// <summary>
        /// BigInt导出byte整数首字节>0x7F的会加0前导，保证正整数，因此需要去掉0
        /// </summary>
        public static byte[] BigB(BigInteger bigx)
        {
            byte[] val = bigx.ToByteArray().Reverse().ToArray(); //C#的二进制是反的
            if (val[0] == 0)
            {
                byte[] c = new byte[val.Length - 1];
                Array.Copy(val, 1, c, 0, c.Length);
                val = c;
            }

            return val;
        }

        /// <summary>
        /// 由n e d 反推 P Q 
        /// </summary>
        private static BigInteger FindFactor(BigInteger e, BigInteger d, BigInteger n)
        {
            BigInteger edMinus1 = e * d - BigInteger.One;
            int s = -1;
            if (edMinus1 != BigInteger.Zero)
            {
                s = (int)(BigInteger.Log(edMinus1 & -edMinus1) / BigInteger.Log(2));
            }

            BigInteger t = edMinus1 >> s;

            long now = DateTime.Now.Ticks;
            for (int aInt = 2; ; aInt++)
            {
                if (aInt % 10 == 0 && DateTime.Now.Ticks - now > 3000 * 10000)
                {
                    throw new Exception("推算RSA.P超时"); //测试最多循环2次，1024位的速度很快 8ms
                }

                BigInteger aPow = BigInteger.ModPow(new BigInteger(aInt), t, n);
                for (int i = 1; i <= s; i++)
                {
                    if (aPow == BigInteger.One)
                    {
                        break;
                    }

                    if (aPow == n - BigInteger.One)
                    {
                        break;
                    }

                    BigInteger aPowSquared = aPow * aPow % n;
                    if (aPowSquared == BigInteger.One)
                    {
                        return BigInteger.GreatestCommonDivisor(aPow - BigInteger.One, n);
                    }

                    aPow = aPowSquared;
                }
            }
        }


        /// <summary>
        /// 用PEM格式密钥对创建RSA，支持PKCS#1、PKCS#8格式的PEM
        /// 出错将会抛出异常
        /// </summary>
        public static RsaPem FromPEM(string pem)
        {
            RsaPem param = new RsaPem();

            var base64 = PemCode.Replace(pem, "");
            byte[] data = null;
            try
            {
                data = Convert.FromBase64String(base64);
            }
            catch
            {
            }

            if (data == null)
            {
                throw new Exception("PEM内容无效");
            }

            var idx = 0;

            //读取长度
            Func<byte, int> readLen = (first) =>
            {
                if (data[idx] == first)
                {
                    idx++;
                    if (data[idx] == 0x81)
                    {
                        idx++;
                        return data[idx++];
                    }

                    if (data[idx] == 0x82)
                    {
                        idx++;
                        return ((data[idx++]) << 8) + data[idx++];
                    }

                    if (data[idx] < 0x80)
                    {
                        return data[idx++];
                    }
                }

                throw new Exception("PEM未能提取到数据");
            };
            //读取块数据
            Func<byte[]> readBlock = () =>
            {
                var len = readLen(0x02);
                if (data[idx] == 0x00)
                {
                    idx++;
                    len--;
                }

                var val = new byte[len];
                for (var i = 0; i < len; i++)
                {
                    val[i] = data[idx + i];
                }

                idx += len;
                return val;
            };
            //比较data从idx位置开始是否是byts内容
            Func<byte[], bool> eq = byts =>
            {
                for (var i = 0; i < byts.Length; i++, idx++)
                {
                    if (idx >= data.Length)
                    {
                        return false;
                    }

                    if (byts[i] != data[idx])
                    {
                        return false;
                    }
                }

                return true;
            };


            if (pem.Contains("PUBLIC KEY"))
            {
                //使用公钥
                //读取数据总长度
                readLen(0x30);

                //看看有没有oid
                var idx2 = idx;
                if (eq(SeqOid))
                {
                    //读取1长度
                    readLen(0x03);
                    idx++; //跳过0x00
                    //读取2长度
                    readLen(0x30);
                }
                else
                {
                    idx = idx2;
                }

                //Modulus
                param.KeyModulus = readBlock();

                //Exponent
                param.KeyExponent = readBlock();
            }
            else if (pem.Contains("PRIVATE KEY"))
            {
                //使用私钥
                //读取数据总长度
                readLen(0x30);

                //读取版本号
                if (!eq(Ver))
                {
                    throw new Exception("PEM未知版本");
                }

                //检测PKCS8
                var idx2 = idx;
                if (eq(SeqOid))
                {
                    //读取1长度
                    readLen(0x04);
                    //读取2长度
                    readLen(0x30);

                    //读取版本号
                    if (!eq(Ver))
                    {
                        throw new Exception("PEM版本无效");
                    }
                }
                else
                {
                    idx = idx2;
                }

                //读取数据
                param.KeyModulus = readBlock();
                param.KeyExponent = readBlock();
                param.KeyD = readBlock();
                param.ValP = readBlock();
                param.ValQ = readBlock();
                param.ValDp = readBlock();
                param.ValDq = readBlock();
                param.ValInverseQ = readBlock();
            }
            else
            {
                throw new Exception("pem需要BEGIN END标头");
            }

            return param;
        }

        private static readonly Regex PemCode = new Regex(@"--+.+?--+|\s+");

        private static readonly byte[] SeqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

        private static readonly byte[] Ver = { 0x02, 0x01, 0x00 };


        /// <summary>
        /// 将RSA中的密钥对转换成PEM格式，usePKCS8=false时返回PKCS#1格式，否则返回PKCS#8格式，如果convertToPublic含私钥的RSA将只返回公钥，仅含公钥的RSA不受影响
        /// </summary>
        public string ToPEM(bool convertToPublic, bool usePKCS8)
        {
            var ms = new PooledMemoryStream();
            //写入一个长度字节码
            Action<int> writeLenByte = len =>
            {
                if (len < 0x80)
                {
                    ms.WriteByte((byte)len);
                }
                else if (len <= 0xff)
                {
                    ms.WriteByte(0x81);
                    ms.WriteByte((byte)len);
                }
                else
                {
                    ms.WriteByte(0x82);
                    ms.WriteByte((byte)(len >> 8 & 0xff));
                    ms.WriteByte((byte)(len & 0xff));
                }
            };
            //写入一块数据
            Action<byte[]> writeBlock = byts =>
            {
                var addZero = (byts[0] >> 4) >= 0x8;
                ms.WriteByte(0x02);
                var len = byts.Length + (addZero ? 1 : 0);
                writeLenByte(len);

                if (addZero)
                {
                    ms.WriteByte(0x00);
                }

                ms.Write(byts, 0, byts.Length);
            };
            //根据后续内容长度写入长度数据
            Func<int, byte[], byte[]> writeLen = (index, byts) =>
            {
                var len = byts.Length - index;

                ms.SetLength(0);
                ms.Write(byts, 0, index);
                writeLenByte(len);
                ms.Write(byts, index, len);

                return ms.ToArray();
            };
            Action<Stream, byte[]> writeAll = (stream, byts) =>
            {
                stream.Write(byts, 0, byts.Length);
            };
            Func<string, int, string> TextBreak = (text, line) =>
            {
                var idx = 0;
                var len = text.Length;
                var str = new StringBuilder();
                while (idx < len)
                {
                    if (idx > 0)
                    {
                        str.Append('\n');
                    }

                    str.Append(idx + line >= len ? text.Substring(idx) : text.Substring(idx, line));
                    idx += line;
                }

                return str.ToString();
            };


            if (KeyD == null || convertToPublic)
            {
                //生成公钥
                //写入总字节数，不含本段长度，额外需要24字节的头，后续计算好填入
                ms.WriteByte(0x30);
                var index1 = (int)ms.Length;

                //固定内容
                writeAll(ms, SeqOid);

                //从0x00开始的后续长度
                ms.WriteByte(0x03);
                var index2 = (int)ms.Length;
                ms.WriteByte(0x00);

                //后续内容长度
                ms.WriteByte(0x30);
                var index3 = (int)ms.Length;

                //写入Modulus
                writeBlock(KeyModulus);

                //写入Exponent
                writeBlock(KeyExponent);

                //计算空缺的长度
                var bytes = ms.ToArray();
                bytes = writeLen(index3, bytes);
                bytes = writeLen(index2, bytes);
                bytes = writeLen(index1, bytes);
                return "-----BEGIN PUBLIC KEY-----\n" + TextBreak(Convert.ToBase64String(bytes), 64) + "\n-----END PUBLIC KEY-----";
            }
            else
            {
                /****生成私钥****/

                //写入总字节数，后续写入
                ms.WriteByte(0x30);
                int index1 = (int)ms.Length;

                //写入版本号
                writeAll(ms, Ver);

                //PKCS8 多一段数据
                int index2 = -1, index3 = -1;
                if (usePKCS8)
                {
                    //固定内容
                    writeAll(ms, SeqOid);

                    //后续内容长度
                    ms.WriteByte(0x04);
                    index2 = (int)ms.Length;

                    //后续内容长度
                    ms.WriteByte(0x30);
                    index3 = (int)ms.Length;

                    //写入版本号
                    writeAll(ms, Ver);
                }

                //写入数据
                writeBlock(KeyModulus);
                writeBlock(KeyExponent);
                writeBlock(KeyD);
                writeBlock(ValP);
                writeBlock(ValQ);
                writeBlock(ValDp);
                writeBlock(ValDq);
                writeBlock(ValInverseQ);


                //计算空缺的长度
                var byts = ms.ToArray();

                if (index2 != -1)
                {
                    byts = writeLen(index3, byts);
                    byts = writeLen(index2, byts);
                }

                byts = writeLen(index1, byts);


                var flag = " PRIVATE KEY";
                if (!usePKCS8)
                {
                    flag = " RSA" + flag;
                }

                return "-----BEGIN" + flag + "-----\n" + TextBreak(Convert.ToBase64String(byts), 64) + "\n-----END" + flag + "-----";
            }
        }


        /// <summary>
        /// 将XML格式密钥转成PEM，支持公钥xml、私钥xml
        /// 出错将会抛出异常
        /// </summary>
        public static RsaPem FromXML(string xml)
        {
            var rtv = new RsaPem();
            var xmlM = XmlExp.Match(xml);
            if (!xmlM.Success)
            {
                throw new Exception("XML内容不符合要求");
            }

            var tagM = XmlTagExp.Match(xmlM.Groups[1].Value);
            while (tagM.Success)
            {
                string tag = tagM.Groups[1].Value;
                string b64 = tagM.Groups[2].Value;
                byte[] val = Convert.FromBase64String(b64);
                switch (tag)
                {
                    case "Modulus":
                        rtv.KeyModulus = val;
                        break;
                    case "Exponent":
                        rtv.KeyExponent = val;
                        break;
                    case "D":
                        rtv.KeyD = val;
                        break;

                    case "P":
                        rtv.ValP = val;
                        break;
                    case "Q":
                        rtv.ValQ = val;
                        break;
                    case "DP":
                        rtv.ValDp = val;
                        break;
                    case "DQ":
                        rtv.ValDq = val;
                        break;
                    case "InverseQ":
                        rtv.ValInverseQ = val;
                        break;
                }

                tagM = tagM.NextMatch();
            }

            if (rtv.KeyModulus == null || rtv.KeyExponent == null)
            {
                throw new Exception("XML公钥丢失");
            }

            if (rtv.KeyD != null)
            {
                if (rtv.ValP == null || rtv.ValQ == null || rtv.ValDp == null || rtv.ValDq == null || rtv.ValInverseQ == null)
                {
                    return new RsaPem(rtv.KeyModulus, rtv.KeyExponent, rtv.KeyD);
                }
            }

            return rtv;
        }

        private static readonly Regex XmlExp = new Regex("\\s*<RSAKeyValue>([<>\\/\\+=\\w\\s]+)</RSAKeyValue>\\s*");
        private static readonly Regex XmlTagExp = new Regex("<(.+?)>\\s*([^<]+?)\\s*</");


        /// <summary>
        /// 将RSA中的密钥对转换成XML格式
        /// ，如果convertToPublic含私钥的RSA将只返回公钥，仅含公钥的RSA不受影响
        /// </summary>
        public string ToXML(bool convertToPublic)
        {
            StringBuilder str = new StringBuilder();
            str.Append("<RSAKeyValue>");
            str.Append("<Modulus>" + Convert.ToBase64String(KeyModulus) + "</Modulus>");
            str.Append("<Exponent>" + Convert.ToBase64String(KeyExponent) + "</Exponent>");
            if (KeyD != null && !convertToPublic)
            {
                /****生成私钥****/
                str.Append("<P>" + Convert.ToBase64String(ValP) + "</P>");
                str.Append("<Q>" + Convert.ToBase64String(ValQ) + "</Q>");
                str.Append("<DP>" + Convert.ToBase64String(ValDp) + "</DP>");
                str.Append("<DQ>" + Convert.ToBase64String(ValDq) + "</DQ>");
                str.Append("<InverseQ>" + Convert.ToBase64String(ValInverseQ) + "</InverseQ>");
                str.Append("<D>" + Convert.ToBase64String(KeyD) + "</D>");
            }

            str.Append("</RSAKeyValue>");
            return str.ToString();
        }
    }
}