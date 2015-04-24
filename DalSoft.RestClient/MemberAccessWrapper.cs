using System;
using System.Dynamic;
using System.Linq;
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
        private readonly string _callLog;

        public MemberAccessWrapper(IHttpClientWrapper httpClientWrapper, string baseUri, string callLog)
        {
            _httpClientWrapper = httpClientWrapper;
            _baseUri = baseUri;
            _callLog = callLog;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            if (binder == null) throw new ArgumentNullException("binder");

            if (Resource(args, out result))
                return true;

            if (Query(args, out result))
                return true;

            if (Extensions.IsHttpVerb(GetLastCall()))
            {
                result = HttpVerb(args);
                return true;
            }

            var builder = new StringBuilder(_callLog);
            foreach (var t in args)
            {
                builder.Append("/");
                var s = t as string;
                if (s != null)
                    builder.Append("@\"").Append(s.Replace("\"", "\"\"")).Append("\"");
                else
                    builder.Append(t);
            }

            result = new MemberAccessWrapper(_httpClientWrapper, _baseUri, builder.ToString());

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new MemberAccessWrapper(_httpClientWrapper, _baseUri, _callLog + "/" + binder.Name);
            return true;
        }

        private async Task<object> HttpVerb(object[] args)
        {
            args.ParseHttpVerbArgs();

            var httpMethodString = GetLastCall();
            var uri = Extensions.GetUri(ToString(), args);
            var requestHeaders = args.GetRequestHeaders();
            var httpMethod = (HttpMethod)typeof(HttpMethod).GetProperty(httpMethodString).GetValue(null);

            var httpContent = Extensions.ParseContent(httpMethodString, args);
            var httpResponseMessage = await _httpClientWrapper.Send(httpMethod, uri, requestHeaders, httpContent);

            return new RestClientResponseObject(httpResponseMessage);
        }

        private bool Resource(object[] args, out object result)
        {
            if (GetLastCall() == "Resource")
            {
                if (args.Length == 0)
                    throw new ArgumentException("Please provide a resource");

                if (args.Length != 1)
                    throw new ArgumentException("Resource has one argument");

                if (!args[0].GetType().IsPrimitive)
                    throw new ArgumentException("Resource must be a Primitive type");

                result = new MemberAccessWrapper(_httpClientWrapper, _baseUri, _callLog.Replace("/Resource", string.Empty) + "/" + args[0]);
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
                   .Select(x => x.Name + "=" + HttpUtility.UrlEncode(x.GetValue(args[0], null).ToString())).ToArray();
                var queryString = "?" + string.Join("&", pairs);

                result = new MemberAccessWrapper(_httpClientWrapper, _baseUri, _callLog.Replace("/Query", string.Empty) + queryString);

                return true;
            }

            result = null;
            return false;
        }

        private string GetLastCall()
        {
            return _callLog.Split("/".ToCharArray()).Last();
        }

        public override string ToString()
        {
            var resource = _baseUri + (_baseUri.EndsWith("/") ? string.Empty : "/");
            resource += _callLog.Replace(GetLastCall(), string.Empty);
            return resource;
        }
    }
}
