using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ListaSessji01
{
    public class SessionData
    {
        [JsonProperty("result")]
        public List<string> Result { get; set; }
    }

    public class SessionDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SessionData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var sessionData = new SessionData
            {
                Result = jsonObject["result"].Select(x => x.ToString()).ToList()
            };
            return sessionData;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var sessionData = (SessionData)value;
            writer.WriteStartObject();
            writer.WritePropertyName("result");
            writer.WriteStartArray();
            foreach (var sessionId in sessionData.Result)
            {
                writer.WriteValue(sessionId);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }


}
