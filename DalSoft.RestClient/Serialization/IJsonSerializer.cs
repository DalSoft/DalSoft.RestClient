using System;

namespace DalSoft.RestClient.Serialization
{
    internal interface IJsonSerializer
    {
        bool SupportsUtf8 { get; } //When true the Utf8 members below are used to skip the intermediate string, serializers that only work with strings return false

        string Serialize(object value);

        bool TrySerializeToUtf8Bytes(object value, out byte[] utf8Json); //Fast path that skips the intermediate string, serializers that only work with strings return false

        object Deserialize(string json, Type type); //Throws the serializer's native exception on invalid json

        object DeserializeUtf8(byte[] utf8Json, Type type);

        bool TryParse(string json, out IJsonNode result); //Never throws, invalid json returns false

        bool TryParseUtf8(byte[] utf8Json, out IJsonNode result);
    }
}
