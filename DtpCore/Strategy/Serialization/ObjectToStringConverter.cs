﻿using Newtonsoft.Json;
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
            if (token.Type == JTokenType.String)
                return token.Value<string>();

            return token?.ToString(Formatting.None);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                var token = JToken.Parse(value.ToString());
                token.WriteTo(writer);
            }
            catch 
            {
                writer.WriteValue(value);
            }

            //var str = value.ToString();
            //var check = str.Trim();
            //if (check.StartsWith("{") && check.EndsWith("}"))
            //{
            //    var token = JToken.Parse(str);
            //    token.WriteTo(writer);
            //}
            //else
            //{
            //    writer.WriteValue(value);
            //}
        }
    }
}
