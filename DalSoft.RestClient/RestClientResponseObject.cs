using System.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using DalSoft.RestClient.Extensions;

namespace DalSoft.RestClient
{
    internal class RestClientResponseObject : DynamicObject
    {
        private readonly string _responseString;
        private readonly HttpResponseMessage _httpResponseMessage;
        private readonly bool _isRoot;
        private readonly bool _isJson;
        private readonly dynamic _currentObject;

        public RestClientResponseObject(HttpResponseMessage httpResponseMessage, string responseString) //Root
        {
            _isRoot = true;

            _httpResponseMessage = httpResponseMessage;
            _responseString = responseString;

            if (_httpResponseMessage.RequestMessage == null) return;

            // ReSharper disable once InvertIf
            if (_httpResponseMessage.RequestMessage.Headers.Accept.Contains(new MediaTypeWithQualityHeaderValue(Config.JsonMediaType)) ||
                _httpResponseMessage.RequestMessage.ExpectJsonResponse())
            {
                var isValidJson = ToString().TryParseJson(out _currentObject); //Just because we told the server we accpet JSON doesn't mean it will send us valid JSON back

                if (isValidJson)
                    _isJson = true;
                
            }
        }

        public RestClientResponseObject(JObject jObjectToWrap)
        {
            _isRoot = false;
            _currentObject = jObjectToWrap;
        }

        /// <summary>
        ///  If you don't call a method that invokes content you will need to dispose HttpContent, for Json this is done for you
        /// https://aspnetwebstack.codeplex.com/discussions/461495
        /// </summary>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (!_isRoot)
            {
                throw new InvalidOperationException("Sorry implicit cast not supported on child objects yet!");
            }

            if (binder.Type == typeof(string))
            {
                result = ToString();
                return true;
            }

            if (binder.Type == typeof(IEnumerable) && _currentObject is JArray)
            {
                result = Json.WrapJToken(_currentObject);
                return true;
            }

            if (binder.Type == typeof(HttpResponseMessage))
            {
                result = _httpResponseMessage;
                return true;
            }

            if (_isJson)
            {
                var isValid = ToString().TryParseJson(out result, binder.Type, _httpResponseMessage.RequestMessage.GetConfig().JsonSerializerSettings);

                if (result is Exception exception) throw exception; //Ok to throw the serialization error here to help the caller

                return isValid;
            }

            throw new InvalidCastException("Can not cast to " + binder.Type.FullName + OutputErrorString());
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == "HttpResponseMessage")
            {
                result = _httpResponseMessage;
                return true;
            }

            //JToken
            if (_currentObject is JToken jToken)
            {
                result = jToken[binder.Name].WrapJToken();
                if (result != null)
                {
                    return true;
                }
            }
            
            //Member not found return null instead of throwing
            result = null;

            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var jArray = _currentObject as JArray;
            if (_currentObject is JArray)
            {
                result = jArray[(int)indexes[0]].WrapJToken(); //TODO could do better validation here
                return true;
            }

            throw new InvalidOperationException("Can't apply index to object" + OutputErrorString());
        }

        public sealed override string ToString()
        {
            return !_isRoot ? (string) _currentObject.ToString() : _responseString;
        }

        private string OutputErrorString()
        {
            return " \r\nHttpResponseMessage: \r\n" + _httpResponseMessage + " \r\nResponse String:\r\n" + ToString();
        }
    }
}