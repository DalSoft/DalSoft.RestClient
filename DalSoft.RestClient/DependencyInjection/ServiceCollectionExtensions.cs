using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DalSoft.RestClient.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static RestClientFactoryConfig AddRestClient(this IServiceCollection services, string baseUri)
        {
            return AddRestClient(services, baseUri, (Headers) null);
        }
        
        public static RestClientFactoryConfig AddRestClient(this IServiceCollection services, string baseUri, Headers defaultRequestHeaders)
        {
            return  services.AddRestClient(RestClientFactory.DefaultClientName, baseUri, defaultRequestHeaders);
        }
        
        public static RestClientFactoryConfig AddRestClient(this IServiceCollection services, string name, string baseUri)
        {
            return services.AddRestClient(name, baseUri, null);
        }
        
        public static RestClientFactoryConfig AddRestClient(this IServiceCollection services, string name, string baseUri, Headers defaultRequestHeaders)
        {
            return AddRestClient(services, name, sp => baseUri, sp => defaultRequestHeaders);
        }
        
        public static RestClientFactoryConfig AddRestClient(this IServiceCollection services, Func<IServiceProvider, string> baseUriLoader)
        {
            return AddRestClient(services, baseUriLoader, sp => (Headers) null);
        }
            
        public static RestClientFactoryConfig AddRestClient(this IServiceCollection services, Func<IServiceProvider, string> baseUriLoader, Headers defaultRequestHeaders)
        {
            return  services.AddRestClient(RestClientFactory.DefaultClientName, baseUriLoader, sp => defaultRequestHeaders);
        }
            
        public static RestClientFactoryConfig AddRestClient(this IServiceCollection services, Func<IServiceProvider, string> baseUriLoader, Func<IServiceProvider, Headers> headerLoader)
        {
            return  services.AddRestClient(RestClientFactory.DefaultClientName, baseUriLoader, headerLoader);
        }
        
        public static RestClientFactoryConfig AddRestClient(this IServiceCollection services, string name, Func<IServiceProvider, string> baseUriLoader, Func<IServiceProvider, Headers> headerLoader)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name cannot be empty", nameof(name));
            
            services.TryAddSingleton<IRestClientFactory, RestClientFactory>();

            var httpClientBuilder = services.AddHttpClient(name);
            var config = new RestClientFactoryConfig(httpClientBuilder); 
            
            services.AddSingleton(serviceProvider => new RestClientContainer(serviceProvider, name, baseUriLoader(sp), headerLoader(sp)));

            return config;
        }
    }
}
