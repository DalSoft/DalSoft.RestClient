using DalSoft.RestClient.Serialization;

namespace DalSoft.RestClient.Extensions
{
    internal static class Json
    {
        internal static object Wrap(this IJsonNode node)
        {
            if (node == null)
                return null;

            switch (node.Kind)
            {
                case JsonNodeKind.Object:
                    return new RestClientResponseObject(node);
                case JsonNodeKind.Value:
                    return node.GetValue();
                case JsonNodeKind.Array:
                    return new LazyJsonArray(node);
                default:
                    return null;
            }
        }
    }
}
