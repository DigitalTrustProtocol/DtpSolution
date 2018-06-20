using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DtpCore.Extensions
{
    public static class JsonExtensions
    {
        public static void SetProperty(this JObject token, string name, object val)
        {
            if (token == null)
                return;

            if (token.Type != JTokenType.Object)
                return;
                
            token[name] = new JValue(val);
        }

        public static string GetStringIgnoreCase(this JObject obj, string name, string defaultValue = null)
        {
            if (obj == null)
                return defaultValue;

            var token = obj.GetValue(name, StringComparison.OrdinalIgnoreCase);
            return token.ToStringValue(defaultValue);
        }

        public static JValue EnsureProperty(this JToken token, string name, object val)
        {
            if (token == null)
                return null;

            if (token.Type != JTokenType.Object)
                return null;

            if (token[name] != null)
                return (JValue)token[name];
            ((JObject)token).Add(new JProperty(name, val));
            return (JValue)token[name];
        }

        public static JObject EnsureObject(this JToken token, string name)
        {
            if (token == null)
                return null;


            if (token.Type != JTokenType.Object)
                return null;

            return (token[name] == null) ? (JObject)(token[name] = new JObject()) : (JObject)token[name];
        }

        public static string SerializeObject(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None);
        }

        public static T DeserializeObject<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        //public static string CustomRender(this JToken token)
        //{
        //    var serializer = new JsonSerializer();

        //    //var sb = new StringBuilder();
        //    var sw = new StringWriter();
        //    //var jtw = new JsonTextWriter(sw);

        //    serializer.Converters.Add(new BytesToHexConverter());
        //    serializer.Serialize(sw, token);
        //    return sw.ToString();
        //}

        public static string ToStringValue(this JToken token, string defaultValue = "")
        {
            if (token == null)
                return defaultValue;

            if (token.Type == JTokenType.Null)
                return defaultValue;

            if (token.Type == JTokenType.String)
                return (string)token;

            return defaultValue;
        }

        public static bool ToBoolean(this JToken token, bool defaultValue = false)
        {
            if (token == null)
                return defaultValue;

            if (token.Type == JTokenType.Null)
                return defaultValue;

            if (token.Type == JTokenType.Boolean)
                return (bool)token;

            return defaultValue;
        }

        public static int ToInteger(this JToken token, int defaultValue = 0)
        {
            if (token == null)
                return defaultValue;

            if (token.Type == JTokenType.Null)
                return defaultValue;

            if (token.Type == JTokenType.Integer)
                return (int)token;

            return defaultValue;
        }

        public static byte[] ToBytes(this JToken token)
        {
            var defaultValue = new byte[0];
            if (token == null)
                return defaultValue;

            if (token.Type == JTokenType.Null)
                return defaultValue;

            if (token.Type == JTokenType.Bytes)
                return (byte[])token;

            return defaultValue;
        }

        public static DateTime ToDateTime(this JToken token, DateTime defaultValue)
        {
            if (token == null)
                return defaultValue;

            if (token.Type == JTokenType.Null)
                return defaultValue;

            if (token.Type == JTokenType.Date)
                return (DateTime)token;

            return defaultValue;
        }

    }
}
