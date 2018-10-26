using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DtpCore.Strategy.Serialization
{
    public class ObjectToStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(JTokenType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            return token?.ToString(Formatting.None);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var token = JToken.Parse(value.ToString());
            token.WriteTo(writer);
            //writer.WriteValue(token. value.ToString());
        }
    }
}
