using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using DalSoft.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Linq;

namespace DalSoft.RestClient
{
    /// <summary>
    /// 
    /// </summary>
    internal class RestClientResponseObject : DuckObject, IEnumerable
    {
        private readonly HttpResponseMessage _httpResponseMessage;
        private string _responseString;
        private readonly bool _isJson;

        private RestClientResponseObject(object jObjectToWrap, bool isJson, string responseString)
        {
            _responseString = responseString;
            _isJson = isJson;
            this.Extend(jObjectToWrap);
        }

        public RestClientResponseObject(HttpResponseMessage httpResponseMessage, bool isJson)
        {
            _httpResponseMessage = httpResponseMessage;
            
            this.Extend(new { HttpResponseMessage = httpResponseMessage });

            if (_httpResponseMessage.RequestMessage.Headers.Accept.Contains(new MediaTypeWithQualityHeaderValue(HttpClientWrapper.JsonContentType)))
            {
                dynamic responseObject = JsonConvert.DeserializeObject(ToString());
                _isJson = true;
                this.Extend((object)responseObject);
            }
        }

        /// <summary>
        ///  If you don't call a method that invokes content you will need to dispose HttpContent, for Json this is doine for you
        /// https://aspnetwebstack.codeplex.com/discussions/461495
        /// </summary>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(string))
            {
                result = ToString();
                return true;
            }

            if (binder.Type == typeof(HttpResponseMessage))
            {
                result = _httpResponseMessage;
                return true;
            }

            if (_isJson)
            {
                result = JsonConvert.DeserializeObject(ToString(), binder.Type);
                return true;
            }

            result = null;
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            TryGetValue(binder.Name, out result);

            //JValue
            var jValue = result as JValue;
            if (jValue != null)
            {
                result = jValue.Value;
                return true;
            }
            //JArray
            var restClientResponseMessage = result as RestClientResponseObject;
            if (restClientResponseMessage != null && restClientResponseMessage["Root"] as JArray != null)
            {
                var jArray = (JArray) restClientResponseMessage["Root"];
                result = new RestClientResponseObject(jArray.Select(x => new RestClientResponseObject(x, true, ToString())).ToArray(), true,ToString());
                return true;
            }
            //Default
            return base.TryGetMember(binder, out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (this["Root"] as JArray != null)
            {
                var jArray = (JArray)this["Root"];
                result = new RestClientResponseObject(jArray[(int) indexes[0]], true, ToString());
                return true;
            }
            
            return base.TryGetIndex(binder, indexes, out result);
        }

        public override sealed string ToString()
        {
            if (_responseString != null)
                return _responseString;
            
            using (var content = _httpResponseMessage.Content)
            {
                _responseString = content.ReadAsStringAsync().Result;
                return _responseString;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this["Root"] as JArray != null)
            {
                var jArray = (JArray)this["Root"];
                return jArray.Select(x => new RestClientResponseObject(x, true, ToString())).ToArray().GetEnumerator();
                
            }
            
            return (IEnumerator)this.GetEnumerator();
        }
    }
}
