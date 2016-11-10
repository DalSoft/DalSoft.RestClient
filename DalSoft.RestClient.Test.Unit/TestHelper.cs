using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Linq;

namespace DalSoft.RestClient.Test.Unit
{
    internal static class TestHelper
    {
        /* Doing this with getting private fields because I don't own HttpClient and don't won't to make the wrapper more complex than it needs to be just for this test */

        internal static HttpMessageHandler GetHandler(this RestClient restClient)
        {
            var httpClientWrapper = (HttpClientWrapper)restClient.HttpClientWrapper;
            var httpClient = (HttpMessageInvoker)GetPrivateField(httpClientWrapper, "_httpClient");
            var actualHandler = (HttpMessageHandler)GetPrivateField(httpClient, "handler");

            return actualHandler;
        }

        internal static HttpClient GetHttpClient(this RestClient restClient)
        {
            var httpClientWrapper = (HttpClientWrapper)restClient.HttpClientWrapper;
            var httpClient = (HttpClient)GetPrivateField(httpClientWrapper, "_httpClient");
            
            return httpClient;
        }

        internal static object GetPrivateField<T>(T instance, string fieldName)
        {   
            var field = typeof(T).GetField(fieldName) ?? typeof(T).GetRuntimeFields()
                .SingleOrDefault(_ => _.Name == fieldName || _.Name == "_" + fieldName); //.NET 4.5 vs .NET Standard 1.4
            return field?.GetValue(instance);
        }
    }
}