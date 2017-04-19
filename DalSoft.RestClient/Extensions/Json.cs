using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DalSoft.RestClient.Extensions
{
    internal static class Json
    {
        internal static bool TryParseJson(this string json, out object result, Type type = null)
        {
            type = type ?? typeof(object);
            try
            {
                result = JsonConvert.DeserializeObject(json, type);
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
    }
}
