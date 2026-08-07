using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DalSoft.RestClient.Serialization
{
    internal class NewtonsoftJsonNode : IJsonNode
    {
        private readonly JToken _jToken;
        private readonly JsonNodeKind _kind;

        public NewtonsoftJsonNode(JToken jToken)
        {
            _jToken = jToken;
            _kind = jToken is JObject ? JsonNodeKind.Object : jToken is JArray ? JsonNodeKind.Array : JsonNodeKind.Value;
        }

        public JsonNodeKind Kind => _kind;

        public IJsonNode GetMember(string name)
        {
            var member = _jToken is JObject jObject ? jObject[name] : null;
            return member == null ? null : new NewtonsoftJsonNode(member);
        }

        public IJsonNode GetIndex(int index)
        {
            return new NewtonsoftJsonNode(((JArray)_jToken)[index]);
        }

        public int GetArrayCount()
        {
            return ((JArray)_jToken).Count;
        }

        public object GetValue()
        {
            return ((JValue)_jToken).Value;
        }

        public IEnumerable<IJsonNode> EnumerateArray()
        {
            return ((JArray)_jToken).Select(jToken => new NewtonsoftJsonNode(jToken));
        }

        public string ToJsonString()
        {
            return _jToken.ToString();
        }
    }
}
