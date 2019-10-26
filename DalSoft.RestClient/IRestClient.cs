using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace DalSoft.RestClient
{
    /* Used for Strongly typed calls for those that prefer it that way, just wraps MemberAccessWrapper using StronglyTypedMemberAccessWrapper */
    public interface IRestClient : IHeaderExtensions
    {
        /* No matter whether it's declared as a dynamic or RestClient, the methods are called via strongly RestClient regardless as .NET will invoke strong methods before trying dynamic methods.
           But if you have declared or cast it as a dynamic as soon as you attempt a dynamic method not in the RestClient for example RestClient.Users(1).Get() it will call dynamically as normal using MemberAccessWrapper */
        HttpClient HttpClient { get; }
        IHeaderExtensions Authorization(AuthenticationSchemes authenticationScheme, string username, string password);
        IHeaderExtensions Authorization(AuthenticationSchemes authenticationScheme, string bearer);
    }

    public interface IHeaderExtensions : IHeaders, IQuery { }

    public interface IResource
    {
        IQuery Resource(string resource);  
        IQuery Resource<TResources>(Expression<Func<TResources, string>> resourceExpression) where TResources : new();
    }

    public interface IHeaders : IHttpMethods
    {
       IQuery Headers<THeaders>(THeaders headers) where THeaders : class;
    }

    public interface IQuery : IHttpMethods, IResource
    {
        IHttpMethods Query(object queryString); // ToDo: Like Headers should support object and dictionary
    }

    public interface IHttpMethods
    {
        Task<dynamic> Get();
        Task<TReturns> Get<TReturns>() where TReturns : class;
        Task<dynamic> Options();
        Task<TReturns> Options<TReturns>() where TReturns : class;
        Task<HttpResponseMessage> Head(); 
        Task<HttpResponseMessage> Trace();

        Task<dynamic> Post();
        Task<TReturns> Post<TReturns>() where TReturns : class;
        Task<TReturns> Post<TBody, TReturns>(TBody body) where TBody : class where TReturns : class;
        Task<dynamic> Post<TBody>(TBody body) where TBody : class;
        Task<dynamic> Put();
        Task<TReturns> Put<TReturns>() where TReturns : class;
        Task<dynamic> Put<TBody>(TBody body) where TBody : class;
        Task<TReturns> Put<TBody, TReturns>(TBody body) where TBody : class where TReturns : class;
        Task<dynamic> Patch();
        Task<TReturns> Patch<TReturns>() where TReturns : class;
        Task<dynamic> Patch<TBody>(TBody body) where TBody : class;
        Task<TReturns> Patch<TBody, TReturns>(TBody body) where TBody : class where TReturns : class;
        Task<dynamic> Merge();
        Task<TReturns> Merge<TReturns>() where TReturns : class;
        Task<dynamic> Merge<TBody>(TBody body) where TBody : class;
        Task<TReturns> Merge<TBody, TReturns>(TBody body) where TBody : class where TReturns : class;
        Task<dynamic> Delete();
        Task<TReturns> Delete<TReturns>() where TReturns : class;
        Task<TReturns> Delete<TBody, TReturns>(TBody body) where TBody : class where TReturns : class;
        Task<dynamic> Delete<TBody>(TBody body) where TBody : class;
    }
}