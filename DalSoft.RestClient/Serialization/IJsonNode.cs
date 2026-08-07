using System.Collections.Generic;

namespace DalSoft.RestClient.Serialization
{
    internal enum JsonNodeKind
    {
        Object,
        Array,
        Value
    }

    internal interface IJsonNode
    {
        JsonNodeKind Kind { get; }

        IJsonNode GetMember(string name); //Case sensitive, null if missing or not an object

        IJsonNode GetIndex(int index);

        int GetArrayCount();

        object GetValue();

        IEnumerable<IJsonNode> EnumerateArray();

        string ToJsonString();
    }
}
