using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DalSoft.RestClient.Extensions
{
    internal static class Json
    {
        internal static bool TryParseJson(this string json, out object result, Type type = null, JsonSerializerSettings jsonSerializerSettings = null)
        {
            type = type ?? typeof(object);
            try
            {
                result = JsonConvert.DeserializeObject(json, type, jsonSerializerSettings);
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
            if (jToken is JObject jObject)
            {
                result = new RestClientResponseObject(jObject);
            }

            //JValue
            if (jToken is JValue jValue)
            {
                result = jValue.Value;
            }

            //JArray
            if (jToken is JArray jArray)
            {
                result = new List<dynamic>(jArray.Select(WrapJToken));
            }

            return result;
        }
    }
}
