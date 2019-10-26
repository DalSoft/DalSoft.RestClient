using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DalSoft.RestClient
{
    internal class StronglyTypedMemberAccessWrapper : DynamicObject, IRestClient 
    {
        internal dynamic MemberAccessWrapper { get; private set; }

        public StronglyTypedMemberAccessWrapper(MemberAccessWrapper memberAccessWrapper)
        {
            MemberAccessWrapper = memberAccessWrapper;
        }

        public HttpClient HttpClient => (HttpClient)MemberAccessWrapper.HttpClient;

        public IHeaderExtensions Authorization(AuthenticationSchemes authenticationScheme, string username, string password)
        {
            if (authenticationScheme != AuthenticationSchemes.Basic) throw new ArgumentException(nameof(authenticationScheme), $"username and password may only be used with the {AuthenticationSchemes.Basic} authentication scheme.");
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username), $"{username} is null or empty");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password), $"{password} is null or empty");

            MemberAccessWrapper = MemberAccessWrapper.Headers(new Headers(new { Authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}" }));

            return MemberAccessWrapper;
        }

        public IHeaderExtensions Authorization(AuthenticationSchemes authenticationScheme, string bearer)
        {
            if (authenticationScheme != AuthenticationSchemes.Bearer) throw new ArgumentException(nameof(authenticationScheme), $"bearer may only be used with the {AuthenticationSchemes.Bearer} authentication scheme.");

            if (string.IsNullOrWhiteSpace(bearer)) throw new ArgumentNullException(nameof(bearer), $"{bearer} is null or empty");

            MemberAccessWrapper = MemberAccessWrapper.Headers(new Headers(new {Authorization = $"Bearer {bearer}"}));

            return MemberAccessWrapper;
        }

        public IQuery Headers<THeaders>(THeaders headers) where THeaders : class
        {
            MemberAccessWrapper = MemberAccessWrapper.Headers(headers); 

            return MemberAccessWrapper;
        }

        public IQuery Resource(string resource)
        {
            MemberAccessWrapper = MemberAccessWrapper.Resource(resource); 
            
            return MemberAccessWrapper;
        }

        public IQuery Resource<TResources>(Expression<Func<TResources, string>> resourceExpression) where TResources : new()
        {
            var resource = resourceExpression.Compile()(new TResources());

            return MemberAccessWrapper.Resource(resource);
        }

        public IHttpMethods Query(object queryString)
        {
            MemberAccessWrapper = MemberAccessWrapper.Query(queryString); 
            
            return MemberAccessWrapper;
        }
        
        public Task<dynamic> Get() => MemberAccessWrapper.Get();
        public Task<TReturns> Get<TReturns>() where TReturns : class => ((Task<object>)MemberAccessWrapper.Get()).ContinueWith(task => (TReturns)(dynamic)task.Result);
        public Task<dynamic> Options() => MemberAccessWrapper.Options();
        public Task<TReturns> Options<TReturns>() where TReturns : class => ((Task<object>)MemberAccessWrapper.Options()).ContinueWith(task => (TReturns)(dynamic)task.Result);
        public Task<HttpResponseMessage> Head() => ((Task<object>) MemberAccessWrapper.Head()).ContinueWith(task => (HttpResponseMessage)(dynamic) task.Result);
        public Task<HttpResponseMessage> Trace() => ((Task<object>) MemberAccessWrapper.Trace()).ContinueWith(task => (HttpResponseMessage)(dynamic) task.Result);

        public Task<dynamic> Post() => MemberAccessWrapper.Post(default(object));
        public Task<TReturns> Post<TReturns>() where TReturns : class => ((Task<object>)MemberAccessWrapper.Post()).ContinueWith(task => (TReturns)(dynamic)task.Result);
        public Task<dynamic> Post<TBody>(TBody body) where TBody : class => MemberAccessWrapper.Post(body);
        public Task<TReturns> Post<TBody, TReturns>(TBody body) where TBody : class where TReturns : class => ((Task<object>)MemberAccessWrapper.Post(body)).ContinueWith(task => (TReturns)(dynamic)task.Result);

        public Task<dynamic> Put() => MemberAccessWrapper.Put(default(object));
        public Task<TReturns> Put<TReturns>() where TReturns : class => ((Task<object>)MemberAccessWrapper.Put()).ContinueWith(task => (TReturns)(dynamic)task.Result);
        public Task<dynamic> Put<TBody>(TBody body) where TBody : class => MemberAccessWrapper.Put(body);
        public Task<TReturns> Put<TBody, TReturns>(TBody body) where TBody : class where TReturns : class => ((Task<object>)MemberAccessWrapper.Put(body)).ContinueWith(task => (TReturns)(dynamic)task.Result);

        public Task<dynamic> Patch() => MemberAccessWrapper.Patch(default(object));
        public Task<TReturns> Patch<TReturns>() where TReturns : class => ((Task<object>)MemberAccessWrapper.Patch()).ContinueWith(task => (TReturns)(dynamic)task.Result);
        public Task<dynamic> Patch<TBody>(TBody body) where TBody : class => MemberAccessWrapper.Patch(body);
        public Task<TReturns> Patch<TBody, TReturns>(TBody body) where TBody : class where TReturns : class => ((Task<object>)MemberAccessWrapper.Patch(body)).ContinueWith(task => (TReturns)(dynamic)task.Result);

        public Task<dynamic> Merge() => MemberAccessWrapper.Patch(default(object));
        public Task<TReturns> Merge<TReturns>() where TReturns : class => ((Task<object>)MemberAccessWrapper.Patch()).ContinueWith(task => (TReturns)(dynamic)task.Result);
        public Task<dynamic> Merge<TBody>(TBody body) where TBody : class => MemberAccessWrapper.Patch(body);
        public Task<TReturns> Merge<TBody, TReturns>(TBody body) where TBody : class where TReturns : class => ((Task<object>)MemberAccessWrapper.Patch(body)).ContinueWith(task => (TReturns)(dynamic)task.Result);

        public Task<dynamic> Delete() => MemberAccessWrapper.Delete(default(object));
        public Task<TReturns> Delete<TReturns>() where TReturns : class => ((Task<object>)MemberAccessWrapper.Delete()).ContinueWith(task => (TReturns)(dynamic)task.Result);
        public Task<dynamic> Delete<TBody>(TBody body) where TBody : class => MemberAccessWrapper.Delete(body);
        public Task<TReturns> Delete<TBody, TReturns>(TBody body) where TBody : class where TReturns : class => ((Task<object>)MemberAccessWrapper.Delete(body)).ContinueWith(task => (TReturns)(dynamic)task.Result);

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            return ((MemberAccessWrapper)MemberAccessWrapper).TryInvoke(binder, args, out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return ((MemberAccessWrapper)MemberAccessWrapper).TryGetMember(binder, out result);
        }
        
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            return ((MemberAccessWrapper) MemberAccessWrapper).TryConvert(binder, out result);
        }
    }
}