using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DtpCore.Attributes
{
    public class ByteBase58CheckConverter : JsonConverter
    {
        private Base58CheckEncoder _base58CheckEncoder;
        private Network _network;

        public ByteBase58CheckConverter() : base()
        {
            _base58CheckEncoder = new Base58CheckEncoder();
            _network = Network.Main;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new InvalidOperationException(); // This converter should only be applied directly to a property.
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var address = string.Empty;
            
            if (value != null) {
                var key = new KeyId((byte[])value);
                address = key.GetAddress(_network).ToString();
            }

            writer.WriteValue(address);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.String)
                return null;

            
            var jValue = new JValue(reader.Value);
            var data = (string)jValue;
            if (data == null || data.Length == 0)
                return null;

            var address = BitcoinAddress.Create(data, _network);
            //address
            //if (address.Length > 24 && address.Length < 35)
            //    return _base58CheckEncoder.DecodeData(address);

            //var base64Encoder = new Base64Encoder();
            //var hash = base64Encoder.DecodeData(address);
            var result = address.ScriptPubKey.Hash.ToBytes();
            return result;
        }
    }

}
