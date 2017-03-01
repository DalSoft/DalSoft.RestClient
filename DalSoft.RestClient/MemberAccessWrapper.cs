using System;
using System.Dynamic;
using System.Linq;
using DalSoft.RestClient.Commands;

namespace DalSoft.RestClient
{
    internal class MemberAccessWrapper : DynamicObject
    {
        internal readonly IHttpClientWrapper HttpClientWrapper;
        internal readonly string BaseUri;
        internal readonly string Uri;

        public MemberAccessWrapper(IHttpClientWrapper httpClientWrapper, string baseUri, string uri)
        {
            HttpClientWrapper = httpClientWrapper;
            BaseUri = baseUri ?? string.Empty;
            Uri = uri ?? string.Empty;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));
            
            var command = CommandFactory.GetCommandFor(GetLastCall(), args);
            if (command != null)
            {
                result = command.Execute(args, this);
                return true;
            }
            
            result = new MemberAccessWrapper(HttpClientWrapper, BaseUri, Uri);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new MemberAccessWrapper(HttpClientWrapper, BaseUri, Uri + "/" + binder.Name);
            return true;
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
            var baseUri = BaseUri + (BaseUri.EndsWith("/") ? string.Empty : "/");
            return baseUri + GetRelativeUri();
        }
    }
}
