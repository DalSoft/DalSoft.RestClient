using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DalSoft.RestClient.Serialization
{
    internal class SystemTextJsonNode : IJsonNode
    {
        private readonly JsonNode _jsonNode;
        private readonly JsonNodeKind _kind;

        public SystemTextJsonNode(JsonNode jsonNode)
        {
            _jsonNode = jsonNode;
            _kind = jsonNode is JsonObject ? JsonNodeKind.Object : jsonNode is JsonArray ? JsonNodeKind.Array : JsonNodeKind.Value;
        }

        public JsonNodeKind Kind => _kind;

        public IJsonNode GetMember(string name)
        {
            var member = _jsonNode is JsonObject jsonObject ? jsonObject[name] : null;
            return member == null ? null : new SystemTextJsonNode(member);
        }

        public IJsonNode GetIndex(int index)
        {
            var element = ((JsonArray)_jsonNode)[index];
            return element == null ? null : new SystemTextJsonNode(element);
        }

        public int GetArrayCount()
        {
            return ((JsonArray)_jsonNode).Count;
        }

        public object GetValue()
        {
            var jsonElement = ((JsonValue)_jsonNode).GetValue<JsonElement>();

            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Number:
                    return jsonElement.TryGetInt64(out var longValue) ? longValue : (object)jsonElement.GetDouble();
                case JsonValueKind.String:
                    return jsonElement.GetString();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                default:
                    return null;
            }
        }

        public IEnumerable<IJsonNode> EnumerateArray()
        {
            return ((JsonArray)_jsonNode).Select(jsonNode => jsonNode == null ? null : (IJsonNode)new SystemTextJsonNode(jsonNode));
        }

        public string ToJsonString()
        {
            return _jsonNode.ToJsonString();
        }
    }
}
