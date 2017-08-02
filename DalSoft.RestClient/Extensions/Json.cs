using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace DalSoft.RestClient.Extensions
{
    internal static class Json
    {
        internal static bool TryParseJson(this string json, out object result, Type type = null)
        {
            type = type ?? typeof(object);
            try
            {
                result = ParsePascalCase(json);
                return true;
            }
            catch (Exception ex)
            {
                result = ex;
                return false; //Eat invalid json  
            }
        }

        internal static object WrapJToken(this JToken jToken)
        {
            object result = null;

            //JObject
            var jObject = jToken as JObject;
            if (jObject != null)
            {
                result = new RestClientResponseObject(jObject);
            }

            //JValue
            var jValue = jToken as JValue;
            if (jValue != null)
            {
                result = jValue.Value;
            }

            //JArray
            var jArray = jToken as JArray;
            if (jArray != null)
            {
                result = jArray.Select(WrapJToken).ToArray();
            }

            return result;
        }


        // https://stackoverflow.com/questions/35777561/get-a-dynamic-object-for-jsonconvert-deserializeobject-making-properties-upperca
        internal static JToken ParsePascalCase(string json)
        {
            using (var textReader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                return jsonReader.ParsePascalCase();
            }
        }

        internal static JToken ParsePascalCase(this JsonReader reader)
        {
            return reader.ParsePascalCase(n =>
            {
                char[] a = n.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                return new string(a);
            });
        }

        internal static JToken ParsePascalCase(this JsonReader reader, Func<string, string> nameMap)
        {
            JToken token;

            using (var writer = new RenamingJTokenWriter(nameMap))
            {
                writer.WriteToken(reader);
                token = writer.Token;
            }

            return token;
        }
    }
}
