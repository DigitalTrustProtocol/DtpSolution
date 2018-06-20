using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DtpCore.Attributes
{

    public class JsonDictionaryToListConverter<TKey,TValue> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dict = (Dictionary<TKey, TValue>)value;
            var list = dict.Select(p => p.Value).ToArray();
            
            var ja = JArray.FromObject(list);
            ja.WriteTo(writer);
        }

        public override bool CanConvert(Type objectType)
        {
            //return typeof().GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo().);
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanRead
        {
            get { return false; }
        }
    }
}
