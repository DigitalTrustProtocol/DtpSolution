using Newtonsoft.Json;
using System;
using DtpCore.Extensions;

namespace DtpStampCore.Service
{
    public class BytesToHexConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            byte[] arr = (byte[])value;
            writer.WriteValue(arr.ConvertToHex());
        }
    }
}
