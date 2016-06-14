﻿using System;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DalSoft.RestClient
{
    //Thanks and credit to http://stackoverflow.com/questions/12634250/possible-to-get-chained-value-of-dynamicobject
    ///<summary>The job of the MemberAccessWrapper is to chain member calls to create a uri to call the appropriate http method using the provided parms</summary>
    internal class MemberAccessWrapper : DynamicObject
    {
        private readonly IHttpClientWrapper _httpClientWrapper;
        private readonly string _baseUri;
        private readonly string _uri;

        public MemberAccessWrapper(IHttpClientWrapper httpClientWrapper, string baseUri, string uri)
        {
            _httpClientWrapper = httpClientWrapper;
            _baseUri = baseUri;
            _uri = uri;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            if (binder == null) throw new ArgumentNullException("binder");

            if (EscapedResource(args, out result))
                return true;

            if (Query(args, out result))
                return true;

            if (GetLastCall().IsHttpVerb())
            {
                result = HttpVerb(args);
                return true;
            }

            if (Resource(args, out result))
                return true;

            result = new MemberAccessWrapper(_httpClientWrapper, _baseUri, _uri);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new MemberAccessWrapper(_httpClientWrapper, _baseUri, _uri + "/" + binder.Name);
            return true;
        }

        private async Task<object> HttpVerb(object[] args)
        {
            args.ValidateHttpVerbArgs();

            var httpMethodString = GetLastCall();

            var uri = Extensions.GetUri(httpMethodString, ToString(), args);
            var requestHeaders = args.GetRequestHeaders();

            var httpContent = Extensions.ParseContent(httpMethodString, args);
            var httpResponseMessage = await _httpClientWrapper.Send(new HttpMethod(httpMethodString), uri, requestHeaders, httpContent);

            return new RestClientResponseObject(httpResponseMessage);
        }

        private bool Resource(object[] args, out object result)
        {
            if (args.Length > 0)
            {
                args.ValidateResourceArgs();
                
                result = new MemberAccessWrapper(_httpClientWrapper, _baseUri, _uri + "/" + args[0]);
                return true;
            }

            result = null;
            return false;
        }

        private bool EscapedResource(object[] args, out object result)
        {
            if (GetLastCall() == "Resource")
            {
                args.ValidateResourceArgs();
                if (args.Length != 1) throw new ArgumentException("Resource can only have one argument");

                result = new MemberAccessWrapper(_httpClientWrapper, _baseUri, GetRelativeUri() + "/" + args[0]);
                return true;
            }

            result = null;
            return false;
        }

        private bool Query(object[] args, out object result)
        {
            if (GetLastCall() == "Query")
            {
                if (args.Length == 0)
                    throw new ArgumentException("Please provide a query");

                if (args.Length != 1)
                    throw new ArgumentException("Query has one argument");

                if (args[0].GetType().Namespace != null)
                    throw new ArgumentException("Query must be a anonymous type");

                var pairs = args[0].GetType().GetProperties()
                   .Select(x => x.Name + "=" + WebUtility.UrlEncode(x.GetValue(args[0], null).ToString())).ToArray();
                var queryString = "?" + string.Join("&", pairs);

                result = new MemberAccessWrapper(_httpClientWrapper, _baseUri, GetRelativeUri() + queryString);

                return true;
            }

            result = null;
            return false;
        }

        private string GetLastCall()
        {
            return _uri.Split("/".ToCharArray()).Last();
        }

        private string GetRelativeUri()
        {
            var resources = _uri.Split("/".ToCharArray());
            return string.Join("/", resources.Take(resources.Length - 1));
        }

        public override string ToString()
        {
            var baseUri = _baseUri + (_baseUri.EndsWith("/") ? string.Empty : "/");
            return baseUri + GetRelativeUri();
        }
    }
}
