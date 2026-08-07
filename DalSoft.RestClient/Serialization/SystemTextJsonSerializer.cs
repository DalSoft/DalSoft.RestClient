using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DalSoft.RestClient.Serialization
{
    internal class SystemTextJsonSerializer : IJsonSerializer
    {
        //Real world systems are less than perfect and send us less than perfect JSON, so be lenient in what we accept - a reasonable compromise over strict parsing
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private static readonly JsonDocumentOptions DocumentOptions = new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        internal static readonly SystemTextJsonSerializer Default = new SystemTextJsonSerializer();

        private readonly JsonSerializerOptions _options;

        internal JsonSerializerOptions Options => _options;

        public SystemTextJsonSerializer(JsonSerializerOptions options = null)
        {
            _options = options ?? DefaultOptions;
        }

        public bool SupportsUtf8 => true;

        public string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, value.GetType(), _options);
        }

        public bool TrySerializeToUtf8Bytes(object value, out byte[] utf8Json)
        {
            utf8Json = JsonSerializer.SerializeToUtf8Bytes(value, value.GetType(), _options);
            return true;
        }

        public object Deserialize(string json, Type type)
        {
            return JsonSerializer.Deserialize(json, type, _options);
        }

        public object DeserializeUtf8(byte[] utf8Json, Type type)
        {
            return JsonSerializer.Deserialize(new ReadOnlySpan<byte>(utf8Json), type, _options);
        }

        public bool TryParse(string json, out IJsonNode result)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                result = null;
                return true; //An empty body isn't invalid json it's just no content
            }

            try
            {
                var jsonNode = JsonNode.Parse(json, documentOptions: DocumentOptions);
                result = jsonNode == null ? null : new SystemTextJsonNode(jsonNode);
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
            if (IsNullOrWhiteSpace(utf8Json))
            {
                result = null;
                return true; //An empty body isn't invalid json it's just no content
            }

            try
            {
                var jsonNode = JsonNode.Parse(new ReadOnlySpan<byte>(utf8Json), documentOptions: DocumentOptions);
                result = jsonNode == null ? null : new SystemTextJsonNode(jsonNode);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false; //Eat invalid json
            }
        }

        private static bool IsNullOrWhiteSpace(byte[] utf8Json)
        {
            if (utf8Json == null || utf8Json.Length == 0)
                return true;

            foreach (var b in utf8Json)
            {
                if (b != (byte)' ' && b != (byte)'\t' && b != (byte)'\r' && b != (byte)'\n')
                    return false;
            }

            return true;
        }
    }
}
