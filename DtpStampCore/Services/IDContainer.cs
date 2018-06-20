using NBitcoin.DataEncoders;
using System;
using DtpCore.Extensions;


namespace DtpStampCore.Services
{
    public abstract class IDContainer
    {
        public string TextID { get; set; }
        public byte[] Hash { get; set; }

        public static IDContainer Parse(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ApplicationException("Value cannot be empty");

            if (HexEncoder.IsWellFormed(id))
                return new HexID(id);

            if (id.IsBase64String())
                return new Base64ID(id);

            throw new ApplicationException("ID format unknown");
        }
    }

    public class HexID : IDContainer
    {
        public HexID(string hex)
        {
            TextID = hex;
            Hash = TextID.ConvertFromHex();
        }
    }

    public class Base64ID : IDContainer
    {
        public Base64ID(string base64)
        {
            TextID = base64;
            Hash = Convert.FromBase64String(base64);
        }
    }

}
