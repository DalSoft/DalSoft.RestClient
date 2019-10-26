using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable InheritdocConsiderUsage

namespace DalSoft.RestClient
{
    /// <summary>RestClient is a conventions based dynamic Rest Client by Darran Jones</summary>
    public class RestClient : DynamicObject, IRestClient, IDisposable
    {
        internal readonly IHttpClientWrapper HttpClientWrapper;
        public IReadOnlyDictionary<string, string> DefaultRequestHeaders => HttpClientWrapper.DefaultRequestHeaders;
        public string BaseUri { get; }
        
        public RestClient(string baseUri) : this(new HttpClientWrapper(), baseUri) { }
        public RestClient(string baseUri, Config config) : this(new HttpClientWrapper(config), baseUri) { }
        public RestClient(string baseUri, IDictionary<string, string> defaultRequestHeaders) : this(new HttpClientWrapper(defaultRequestHeaders), baseUri) { }
        public RestClient(string baseUri, IDictionary<string, string> defaultRequestHeaders, Config config) : this(new HttpClientWrapper(defaultRequestHeaders, config), baseUri) { }
        
        public RestClient(IHttpClientWrapper httpClientWrapper, string baseUri)
        {
            HttpClientWrapper = httpClientWrapper ?? new HttpClientWrapper();

            if (HttpClientWrapper is HttpClientWrapper containsHttpClientWrapper)
                containsHttpClientWrapper.HttpClient.BaseAddress = new Uri(baseUri);

            BaseUri = baseUri;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new MemberAccessWrapper(HttpClientWrapper, BaseUri, binder.Name, new Dictionary<string, string>());
            return true;
        }
        
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type != typeof(HttpClient))
                throw new InvalidCastException("Can not cast to " + binder.Type.FullName);
            
            if (!(HttpClientWrapper is HttpClientWrapper httpClientWrapper))
                throw new NotSupportedException($"{nameof(HttpClientWrapper)} doesn't support HttpClient");
                
            result =  httpClientWrapper.HttpClient;
            return true;
        }

        public HttpClient HttpClient => CreateStronglyTypedMemberAccessWrapper().HttpClient;
        public IHeaderExtensions Authorization(AuthenticationSchemes authenticationScheme, string username, string password) => CreateStronglyTypedMemberAccessWrapper().Authorization(authenticationScheme, username, password);
        public IHeaderExtensions Authorization(AuthenticationSchemes authenticationScheme, string bearer) => CreateStronglyTypedMemberAccessWrapper().Authorization(authenticationScheme, bearer);

        public IQuery Headers<THeaders>(THeaders headers) where THeaders : class => CreateStronglyTypedMemberAccessWrapper().Headers(headers);
        public IQuery Resource(string resource) => CreateStronglyTypedMemberAccessWrapper().Resource(resource);
        public IQuery Resource<TResources>(Expression<Func<TResources, string>> resourceExpression) where TResources : new()
        {
            var resource = resourceExpression.Compile()(new TResources());

            return CreateStronglyTypedMemberAccessWrapper().Resource(resource);
        }
        public IHttpMethods Query(object queryString) => CreateStronglyTypedMemberAccessWrapper().Query(queryString);
        
        // Only called for the edge case where you want call a http verb on the BaseUri RestClient.Get(), see comments in IRestClient.
        public Task<dynamic> Get() => CreateStronglyTypedMemberAccessWrapper().Get();
        public Task<TReturns> Get<TReturns>() where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Get().ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);

        public Task<dynamic> Options() => CreateStronglyTypedMemberAccessWrapper().Options();
        public Task<TReturns> Options<TReturns>() where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Options().ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);

        public Task<HttpResponseMessage> Head() => CreateStronglyTypedMemberAccessWrapper().Head();
        public Task<HttpResponseMessage> Trace() => CreateStronglyTypedMemberAccessWrapper().Trace();

        public Task<dynamic> Post() => CreateStronglyTypedMemberAccessWrapper().Post(default(object));
        public Task<TReturns> Post<TReturns>() where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Post().ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);
        public Task<dynamic> Post<TBody>(TBody body) where TBody : class => CreateStronglyTypedMemberAccessWrapper().Post(body);
        public Task<TReturns> Post<TBody, TReturns>(TBody body) where TBody : class where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Post(body).ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);

        public Task<dynamic> Put() => CreateStronglyTypedMemberAccessWrapper().Put(default(object));
        public Task<TReturns> Put<TReturns>() where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Put().ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);
        public Task<dynamic> Put<TBody>(TBody body) where TBody : class => CreateStronglyTypedMemberAccessWrapper().Put(body);
        public Task<TReturns> Put<TBody, TReturns>(TBody body) where TBody : class where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Put(body).ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);

        public Task<dynamic> Patch() => CreateStronglyTypedMemberAccessWrapper().Patch(default(object));
        public Task<TReturns> Patch<TReturns>() where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Patch().ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);
        public Task<dynamic> Patch<TBody>(TBody body) where TBody : class => CreateStronglyTypedMemberAccessWrapper().Patch(body);
        public Task<TReturns> Patch<TBody, TReturns>(TBody body) where TBody : class where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Patch(body).ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);

        public Task<dynamic> Merge() => CreateStronglyTypedMemberAccessWrapper().Patch(default(object));
        public Task<TReturns> Merge<TReturns>() where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Patch().ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);
        public Task<dynamic> Merge<TBody>(TBody body) where TBody : class => CreateStronglyTypedMemberAccessWrapper().Patch(body);
        public Task<TReturns> Merge<TBody, TReturns>(TBody body) where TBody : class where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Patch(body).ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);

        public Task<dynamic> Delete() => CreateStronglyTypedMemberAccessWrapper().Delete(default(object));
        public Task<TReturns> Delete<TReturns>() where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Delete().ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);
        public Task<dynamic> Delete<TBody>(TBody body) where TBody : class => CreateStronglyTypedMemberAccessWrapper().Delete(body);
        public Task<TReturns> Delete<TBody, TReturns>(TBody body) where TBody : class where TReturns : class => CreateStronglyTypedMemberAccessWrapper().Delete(body).ContinueWith(task => (TReturns)task.Result, cancellationToken: default, continuationOptions: TaskContinuationOptions.None, scheduler: TaskScheduler.Default);


        public void Dispose()
        {
            HttpClientWrapper.Dispose();
        }

        private StronglyTypedMemberAccessWrapper CreateStronglyTypedMemberAccessWrapper() => new StronglyTypedMemberAccessWrapper(new MemberAccessWrapper(HttpClientWrapper, BaseUri, null, new Headers()));
    }
}
