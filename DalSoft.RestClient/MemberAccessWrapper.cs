using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using DalSoft.RestClient.Commands;

namespace DalSoft.RestClient
{
    internal class MemberAccessWrapper : DynamicObject
    {
        internal readonly IHttpClientWrapper HttpClientWrapper;
        internal readonly string BaseUri;
        internal readonly string Uri;
        internal readonly Dictionary<string, string> Headers;

        public MemberAccessWrapper(IHttpClientWrapper httpClientWrapper, string baseUri, string uri, Dictionary<string, string> headers)
        {
            HttpClientWrapper = httpClientWrapper;
            BaseUri = baseUri ?? string.Empty;
            Uri = uri ?? string.Empty;
            Headers = headers ?? new Dictionary<string, string>();
        }

        private bool IsRoot => Uri == string.Empty;

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));
            
            var command = CommandFactory.GetCommandFor(GetLastCall(), args);
            if (command != null)
            {
                result = command.Execute(args, this);
                return true;
            }
            
            result = new MemberAccessWrapper(HttpClientWrapper, BaseUri, Uri, Headers);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (IsRoot && binder.Name == nameof(HttpClient))
            {
                if (!(HttpClientWrapper is HttpClientWrapper httpClientWrapper))
                    throw new NotSupportedException($"{nameof(HttpClientWrapper)} doesn't support HttpClient");

                result = httpClientWrapper.HttpClient;
                return true;
            }

            result = new MemberAccessWrapper(HttpClientWrapper, BaseUri, Uri.TrimStart("/".ToCharArray()) + "/" + binder.Name, Headers);
            return true;
        }
        
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (IsRoot && binder.Type == typeof(IRestClient) || 
                binder.Type == typeof(IHeaderExtensions) || 
                binder.Type == typeof(IHeaders) || 
                binder.Type == typeof(IResource) || 
                binder.Type == typeof(IQuery)  ||
                binder.Type == typeof(IHttpMethods))
            {
                result = new StronglyTypedMemberAccessWrapper(this);
                return true;
            }

            throw new InvalidCastException("Can not cast to " + binder.Type.FullName);
        }

        internal string GetRelativeUri()
        {
            var resources = Uri.Split("/".ToCharArray());
            return string.Join("/", resources.Take(resources.Length - 1));
        }
        
        internal string GetLastCall()
        {
            return Uri.Split("/".ToCharArray()).Last();
        }
        
        public override string ToString()
        {
            var baseUri = BaseUri.TrimEnd("/".ToCharArray()) + "/";
            return baseUri + GetRelativeUri();
        }
    }
}
