using NBitcoin.DataEncoders;
using System;
using System.Text.RegularExpressions;

namespace DtpCore.Extensions
{
    public static class EncoderExtensions
    {
        public static byte[] ConvertFromHex(this string hex)
        {
            return Encoders.Hex.DecodeData(hex);
        }

        public static string ConvertToHex(this byte[] data)
        {
            return Encoders.Hex.EncodeData(data);
        }

        public static string ConvertToBase64(this byte[] data)
        {
            return Encoders.Base64.EncodeData(data);  
        }

        /// <summary>
        /// From: http://stackoverflow.com/questions/6309379/how-to-check-for-a-valid-base-64-encoded-string-in-c-sharp
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsBase64String(this string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }
    }
}
