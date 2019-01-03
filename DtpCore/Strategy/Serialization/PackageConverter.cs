using DtpCore.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DtpCore.Strategy.Serialization
{
    public class PackageConverter : JsonConverter<Package>
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override Package ReadJson(JsonReader reader, Type objectType, Package existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            JArray templates = jsonObject["templates"] as JArray;
            if (templates != null && templates.Count > 0)
            {
                var settings = new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Merge,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore
                };

                var claims = (JArray)jsonObject["claims"];
                foreach (JObject claim in claims)
                {

                    var index = claim["templateIndex"].HasValues ? (int)((JProperty)claim["templateIndex"]).Value : 0;
                    if (index > templates.Count)
                        continue;

                    var template = templates[index];

                    claim.Merge(template, settings);
                }
            }

            var package = new Package();
            serializer.Populate(jsonObject.CreateReader(), package);
            return package;
        }

        public override void WriteJson(JsonWriter writer, Package value, JsonSerializer serializer)
        {
            //serializer.Serialize(writer, value);
        }
    }
}
