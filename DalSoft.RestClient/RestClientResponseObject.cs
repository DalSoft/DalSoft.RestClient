using System.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DalSoft.RestClient
{
    internal class RestClientResponseObject : DynamicObject
    {
        private string _responseString;
        private readonly HttpResponseMessage _httpResponseMessage;
        private readonly bool _isRoot;
        private readonly bool _isJson;
        private readonly dynamic _currentObject;

        public RestClientResponseObject(HttpResponseMessage httpResponseMessage) //Root
        {
            _isRoot = true;

            _httpResponseMessage = httpResponseMessage;

            if (_httpResponseMessage.RequestMessage.Headers.Accept.Contains(new MediaTypeWithQualityHeaderValue(HttpClientWrapper.JsonContentType)))
            {
                var isValidJson = ToString().TryParseJson(out _currentObject); //Just because we told the server we accpet JSON doesn't mean it will send us valid JSON back

                if (isValidJson)
                {
                    _isJson = true;
                }
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
                throw new InvalidOperationException("Sorry implict cast not supported on child objects yet!");
            }

            if (binder.Type == typeof(IEnumerable) && _currentObject is JArray)
            {
                result = Extensions.WrapJToken(_currentObject);
                return true;
            }

            if (binder.Type == typeof(HttpResponseMessage))
            {
                result = _httpResponseMessage;
                return true;
            }

            if (_isJson)
            {
                var isValid = ToString().TryParseJson(out result, binder.Type);
                return isValid;
            }

            throw new InvalidOperationException("Can not cast to " + binder.Type.FullName + OutputErrorString());
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            //JToken
            var jToken = _currentObject as JToken;
            if (jToken != null)
            {
                result = jToken[binder.Name].WrapJToken();
                if (result != null)
                {
                    return true;
                }
            }

            if (binder.Name == "HttpResponseMessage")
            {
                result = _httpResponseMessage;
                return true;
            }

            //Member not found return null instead of throwing
            result = null;

            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var jArray = _currentObject as JArray;
            if (_currentObject as JArray != null)
            {
                result = jArray[(int)indexes[0]].WrapJToken(); //TODO could do better validation here
                return true;
            }

            throw new InvalidOperationException("Can't apply index to object" + OutputErrorString());
        }

        public override sealed string ToString()
        {
            if (!_isRoot)
                return _currentObject.ToString();

            if (_responseString != null)
                return _responseString;

            using (var content = _httpResponseMessage.Content)
            {
                var responseBytes = content.ReadAsByteArrayAsync().Result;
                
                _responseString = Encoding.UTF8.GetString(responseBytes, 0, responseBytes.Length);
                return _responseString;
            }
        }

        private string OutputErrorString()
        {
            return " \r\nResponse: \r\n" + _httpResponseMessage + " \r\nResponse String:\r\n" + ToString();
        }
    }
}