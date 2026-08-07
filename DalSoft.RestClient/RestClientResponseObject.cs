using System;
using System.Collections;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DalSoft.RestClient.Extensions;
using DalSoft.RestClient.Serialization;

namespace DalSoft.RestClient
{
    internal class RestClientResponseObject : DynamicObject
    {
        private static readonly MediaTypeWithQualityHeaderValue JsonAcceptHeader = new MediaTypeWithQualityHeaderValue(Config.JsonMediaType);

        private readonly byte[] _utf8Body;
        private readonly HttpResponseMessage _httpResponseMessage;
        private readonly bool _isRoot;
        private readonly bool _expectJson;
        private readonly IJsonSerializer _serializer;
        private string _responseString;
        private bool _parseAttempted;
        private bool _isJson;
        private IJsonNode _currentObject;

        public RestClientResponseObject(HttpResponseMessage httpResponseMessage, string responseString) //Root
        {
            _isRoot = true;

            _httpResponseMessage = httpResponseMessage;
            _responseString = responseString;

            if (_httpResponseMessage.RequestMessage == null) return;

            _serializer = _httpResponseMessage.RequestMessage.GetConfig()?.JsonSerializer ?? SystemTextJsonSerializer.Default;

            _expectJson = _httpResponseMessage.RequestMessage.Headers.Accept.Contains(JsonAcceptHeader) ||
                          _httpResponseMessage.RequestMessage.ExpectJsonResponse();
        }

        public RestClientResponseObject(HttpResponseMessage httpResponseMessage, byte[] utf8Body) //Root over a utf-8 body, the string is only decoded if asked for
            : this(httpResponseMessage, (string)null)
        {
            _utf8Body = utf8Body;
        }

        public RestClientResponseObject(IJsonNode nodeToWrap)
        {
            _isRoot = false;
            _currentObject = nodeToWrap;
        }

        private void EnsureParsed() //Parse lazily so typed casts and non json access never pay for building the DOM
        {
            if (_parseAttempted || !_expectJson) return;

            //Just because we told the server we accpet JSON doesn't mean it will send us valid JSON back
            _isJson = _utf8Body != null && _serializer.SupportsUtf8 ? _serializer.TryParseUtf8(_utf8Body, out _currentObject) : _serializer.TryParse(ToString(), out _currentObject);
            _parseAttempted = true; //Benign race if a response is shared across threads, worst case we parse twice
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

            if (binder.Type == typeof(IEnumerable))
            {
                EnsureParsed();

                if (_currentObject?.Kind == JsonNodeKind.Array)
                {
                    result = _currentObject.Wrap();
                    return true;
                }
            }

            if (binder.Type == typeof(HttpResponseMessage))
            {
                result = _httpResponseMessage;
                return true;
            }

            if (_expectJson)
            {
                try
                {
                    //Ok to throw the serialization error here to help the caller
                    result = _utf8Body != null && _serializer.SupportsUtf8 ? _serializer.DeserializeUtf8(_utf8Body, binder.Type) : _serializer.Deserialize(ToString(), binder.Type);
                    return true;
                }
                catch (Exception)
                {
                    EnsureParsed();

                    if (!_isJson)
                        throw new InvalidCastException("Can not cast to " + binder.Type.FullName + OutputErrorString());

                    throw;
                }
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

            EnsureParsed();

            if (_currentObject != null)
            {
                result = _currentObject.GetMember(binder.Name).Wrap();
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
            EnsureParsed();

            if (_currentObject?.Kind == JsonNodeKind.Array)
            {
                result = _currentObject.GetIndex((int)indexes[0]).Wrap(); //TODO could do better validation here
                return true;
            }

            throw new InvalidOperationException("Can't apply index to object" + OutputErrorString());
        }

        public sealed override string ToString()
        {
            if (!_isRoot) return _currentObject.ToJsonString();

            return _responseString ?? (_responseString = DecodeUtf8(_utf8Body)); //Root over a utf-8 body decodes once on first ask
        }

        private static string DecodeUtf8(byte[] utf8Body)
        {
            if (utf8Body == null || utf8Body.Length == 0)
                return string.Empty;

            if (utf8Body.Length >= 3 && utf8Body[0] == 0xEF && utf8Body[1] == 0xBB && utf8Body[2] == 0xBF) //Skip the BOM like ReadAsStringAsync does
                return Encoding.UTF8.GetString(utf8Body, 3, utf8Body.Length - 3);

            return Encoding.UTF8.GetString(utf8Body);
        }

        private string OutputErrorString()
        {
            return " \r\nHttpResponseMessage: \r\n" + _httpResponseMessage + " \r\nResponse String:\r\n" + ToString();
        }
    }
}
