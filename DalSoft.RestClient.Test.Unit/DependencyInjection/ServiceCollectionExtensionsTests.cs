using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.DependencyInjection
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void AddRestClient_WithValidParams_ReturnsRestClientFactoryConfig()
        {
            var serviceCollection = new ServiceCollection();
            
            Assert.IsInstanceOf<RestClientFactoryConfig>(serviceCollection.AddRestClient("http://dalsoft.co.uk"));
        }

        [Test]
        public void AddRestClient_WithValidParams_AddsDefaultRestClientToServiceCollectionWithSingletonScope()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddRestClient("http://dalsoft.co.uk");

            var implementationInstance = serviceCollection
                .SingleOrDefault(_=> _.ServiceType == typeof(RestClientContainer) && _.Lifetime == ServiceLifetime.Singleton)
                ?.ImplementationFactory(serviceCollection.BuildServiceProvider()) as RestClientContainer;
                

            Assert.NotNull(implementationInstance);
            Assert.That(implementationInstance.Name, Is.EqualTo(RestClientFactory.DefaultClientName));
        }

        [Test]
        public void AddRestClient_WithValidParams_AddsNamedHttpClientBuilderToRestClientFactoryConfig()
        {
            var serviceCollection = new ServiceCollection();

            var restClientFactoryConfig = serviceCollection.AddRestClient("https://dalsoft.co.uk");

            Assert.IsNotNull(restClientFactoryConfig.HttpClientBuilder);
            Assert.That(restClientFactoryConfig.HttpClientBuilder.Name, Is.EqualTo(RestClientFactory.DefaultClientName));
        }

        [Test]
        public void AddRestClient_WithBaseUri_CorrectlySetsBaseUri()
        {
            const string baseUri = "https://dalsoft.co.uk";
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddRestClient(baseUri);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.That(serviceProvider.GetService<IRestClientFactory>().CreateClient().BaseUri, Is.EqualTo(baseUri));
        }
        
        [Test]
        public void AddRestClient_WithDefaultHeaders_CorrectAddsDefaultHeaders()
        {
            var defaultHeaders = new Headers(new { ContentType="application/awesome" });
            var serviceCollection = new ServiceCollection();
            
            serviceCollection.AddRestClient("http://dalsoft.co.uk", defaultHeaders);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            Assert.Contains(defaultHeaders.First(), (ICollection)serviceProvider.GetService<IRestClientFactory>().CreateClient().DefaultRequestHeaders);
        }
        
        [Test]
        public void AddRestClient_ByName_ReturnsRestClientFactoryConfig()
        {
            var serviceCollection = new ServiceCollection();

            Assert.IsInstanceOf<RestClientFactoryConfig>(serviceCollection.AddRestClient("MyClient1", "http://dalsoft.co.uk"));
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void AddRestClient_EmptyName_ThrowsArgumentException(string name)
        {
            var serviceCollection = new ServiceCollection();

            Assert.Throws<ArgumentException>(()=>serviceCollection.AddRestClient(name, "http://dalsoft.co.uk"));
        }

        [Test]
        public void AddRestClient_ByName_AddsIRestClientFactoryToServiceCollectionWithSingletonScope()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddRestClient("MyClient1", "http://dalsoft.co.uk");

            Assert.That(serviceCollection.Count(_ => _.ServiceType == typeof(IRestClientFactory) && _.Lifetime == ServiceLifetime.Singleton), Is.EqualTo(1));
        }

        [Test]
        public void AddRestClient_ByName_AddsNamedHttpClientFactoryOptionsToServiceCollection()
        {
            const string clientName = "MyClient1";
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddRestClient(clientName, "http://dalsoft.co.uk");

            var httpClientFactoryOptions = serviceCollection
                .SingleOrDefault(_ => _.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>))?.ImplementationInstance as ConfigureNamedOptions<HttpClientFactoryOptions>;

            Assert.NotNull(httpClientFactoryOptions);
            Assert.That(httpClientFactoryOptions.Name, Is.EqualTo(clientName));
        }
        
        [Test]
        public void AddRestClient_ByName_AddsRestClientContainerToServiceCollectionWithSingletonScope()
        {
            const string clientName = "MyClient1";
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddRestClient(clientName, "http://dalsoft.co.uk");

            var implementationInstance = serviceCollection
                .SingleOrDefault(_=> _.ServiceType == typeof(RestClientContainer) && _.Lifetime == ServiceLifetime.Singleton)
                ?.ImplementationFactory(serviceCollection.BuildServiceProvider()) as RestClientContainer;
                

            Assert.NotNull(implementationInstance);
            Assert.That(implementationInstance.Name, Is.EqualTo(clientName));
        }

        [Test]
        public async Task AddRestClient_AddTwoNamedInstanes_CorrectNameInstanceIsCalled()
        {
            const string myclient1 = "MyClient1", myclient2 = "MyClient2";

            var serviceCollection = new ServiceCollection();
            var lastRequest = new HttpRequestMessage();
            
            serviceCollection.AddRestClient(myclient1, $"http://dalsoft.co.uk/{myclient1}")
                .UseNoDefaultHandlers()
                .UseUnitTestHandler(request => { lastRequest = request; });
                

            serviceCollection.AddRestClient(myclient2, $"http://dalsoft.co.uk/{myclient2}")
                .UseNoDefaultHandlers()
                .UseUnitTestHandler(request => { lastRequest = request; });;

            var restClientFactory = serviceCollection.BuildServiceProvider().GetService<IRestClientFactory>();
            dynamic restClient1 = restClientFactory.CreateClient(myclient1);

            await restClient1.Get();

            Assert.That(lastRequest.RequestUri.ToString(), Is.EqualTo($"http://dalsoft.co.uk/{myclient1}"));

            dynamic restClient2 = restClientFactory.CreateClient(myclient2);

            await restClient2.Get();

            Assert.That(lastRequest.RequestUri.ToString(), Is.EqualTo($"http://dalsoft.co.uk/{myclient2}"));
        }

        [Test]
        public async Task AddRestClient_ByNameAndPassingDefaultHeader_CorrectNameInstanceIsCalled()
        {
            const string myclient1 = "MyClient1";

            var serviceCollection = new ServiceCollection();
            var lastRequest = new HttpRequestMessage();
            
            serviceCollection.AddRestClient(myclient1, "http://dalsoft.co.uk/", new Headers(new { name = myclient1  }))
                .UseNoDefaultHandlers()
                .UseUnitTestHandler(request => { lastRequest = request; });
            
            var restClientFactory = serviceCollection.BuildServiceProvider().GetService<IRestClientFactory>();
            dynamic restClient1 = restClientFactory.CreateClient(myclient1);

            await restClient1.Get();

            Assert.That(lastRequest.Headers.GetValues("name").FirstOrDefault(), Is.EqualTo(myclient1));
        }
    }
}
