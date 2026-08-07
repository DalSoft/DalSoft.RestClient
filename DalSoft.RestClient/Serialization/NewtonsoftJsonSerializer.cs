using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DalSoft.RestClient.Serialization
{
    internal class NewtonsoftJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings _settings;

        internal JsonSerializerSettings Settings => _settings;

        public NewtonsoftJsonSerializer(JsonSerializerSettings settings = null)
        {
            _settings = settings;
        }

        public bool SupportsUtf8 => false; //Json.NET works with strings, callers fall back to the string members

        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, _settings);
        }

        public bool TrySerializeToUtf8Bytes(object value, out byte[] utf8Json)
        {
            utf8Json = null;
            return false; //Json.NET works with strings, callers fall back to Serialize
        }

        public object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, _settings);
        }

        public object DeserializeUtf8(byte[] utf8Json, Type type)
        {
            throw new NotSupportedException("Guarded by SupportsUtf8");
        }

        public bool TryParse(string json, out IJsonNode result)
        {
            try
            {
                var jToken = JsonConvert.DeserializeObject<JToken>(json, _settings);
                result = jToken == null || jToken.Type == JTokenType.Null ? null : new NewtonsoftJsonNode(jToken);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false; //Eat invalid json
            }
        }

        public bool TryParseUtf8(byte[] utf8Json, out IJsonNode result)
        {
            throw new NotSupportedException("Guarded by SupportsUtf8");
        }
    }
}
