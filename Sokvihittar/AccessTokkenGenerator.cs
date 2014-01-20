using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Sokvihittar
{
    public class AccessTokkenGenerator
    {

            private const string DefaultRsaParameters = "<RSAKeyValue>" +
               "<Modulus>ygy7Mob/VCWRFlwB9mm2Ntmtaq/svZrDOjx08GayifkCnoQAT/BB8FtBdZwUYhzzSoC84K8oEp08g01mm8t7ec9mkZNa1I3rEw9glVMAwWGUXslb5qIjdl6h2S6QDJh7pjPH1KWU0aAEmsV9jPM/zmYuCT4Ocz+3gd3ArJO2kh8=</Modulus>" +
               "<Exponent>AQAB</Exponent>" +
               "<P>/GXQoCKAA2P+Tl9G6Gsi2GhDlyQga8eDHGXSCWr3m4sBJAC1GT+b3TzjsmVRQtGpT+1+UpAoiJg9zMweFvKmmQ==</P>" +
               "<Q>zO71wDpuAE2Cz+LUwXXAe96NJadW1NESNAdCJ5TXVZPJrLCTl/8X4QyTVwiXZxM4vwhmWfXcsJB+TzdF0CXJdw==</Q>" +
               "<DP>Ob7JlnmONDhibGfb/zzTwhNIs4GucTo6QvsArOruL4YEAsqupFIrRNizd5M6nkD9ra22YwlcXLQIH5zrnXBysQ==</DP>" +
               "<DQ>U1/43Y8oB7mBOeCYHGkyuXKOzD3rhsPUexRk5sOYY/mveDGSqqke5vF91E/rgQUB9j6NnZX4hmES8lmTbp6g+w==</DQ>" +
               "<InverseQ>XVCYP3Omndfeo7h9Ye4csrUikQh7kep1CCICd7U4NRulmvJfUNQVplmGXdH95g4qxhXmn+PoXf8a7mIaq9ADew==</InverseQ>" +
               "<D>BO9zAnoeB4SpZXCW6Fl5sXT3ZvPoJlN+8aUOEtyMqjvHuW6DrBreyXZj2XHCul4hqpANd7bflYt7XGu0wMPOKXGLLWRP+Q8m9oPQw5ZYKG45FuoDUdfXSBsMOpmaVInfhfOxgSFVRIKXNc6/WmXOT5frz6eHERpI54INfmvKVbk=</D>" +
               "</RSAKeyValue>";



            public static string GenerateKey(LicenseType type, int duration, string rsaParameters)
            {
                var key = new StringBuilder();
                key.Append((int)type).Append("-").Append(duration).Append("-").Append(
                    DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                var provider = new RSACryptoServiceProvider();
                provider.FromXmlString(rsaParameters);
                var param = provider.ExportParameters(true);
                return CryptoHelper.SignLicenseKey(param, key.ToString());
            }


            public static string GenerateKey(LicenseType type, int duration)
            {
                return GenerateKey(type, duration, DefaultRsaParameters);
            }
        }
}
