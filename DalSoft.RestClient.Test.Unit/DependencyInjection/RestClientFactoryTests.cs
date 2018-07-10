using System;
using System.Collections.Generic;
using System.Net.Http;
using DalSoft.RestClient.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.DependencyInjection
{
    public class RestClientFactoryTests
    {
        [Test]
        public void Ctor_NullServiceProvider_ThrowsArgumentNullException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new RestClientFactory(null));
        }

        [TestCase(null)]
        [TestCase(" ")]
        [TestCase("NotMyClient")]
        public void CreateClient_RestClientContainerNotFoundByName_ThrowsInvalidOperationException(string name)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddRestClient("myclient1", "http://www.dalsoft.co.uk");

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var restClientFactory = new RestClientFactory(serviceCollection.BuildServiceProvider());
                restClientFactory.CreateClient(name);
            });

            Assert.That(exception.Message, Is.EqualTo($"No registered RestClient named: {name}"));
        }
        
        [Test]
        public void CreateClient_MoreThanOneRestClientContainerFoundByName_ThrowsInvalidOperationException()
        {
            const string myClient = "myclient1";
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddRestClient(myClient, "http://www.dalsoft.co.uk");
            serviceCollection.AddRestClient(myClient, "http://www.dalsoft.co.uk");

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var restClientFactory = new RestClientFactory(serviceCollection.BuildServiceProvider());
                restClientFactory.CreateClient(myClient);
            });

            Assert.That(exception.Message, Is.EqualTo($"More than one registered RestClient named: {myClient}"));
        }

        [Test]
        public void CreateClient_MatchingRestClientContainerByNameFound_ReturnsMatchingRestClient()
        {
            const string myClient = "myclient1";
            
            var mockServiceProvider = new Mock<IServiceProvider>();
            var restClient = new HttpClient();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(provider => provider.CreateClient(myClient)).Returns(restClient);
            mockServiceProvider.Setup(provider => provider.GetService(typeof(IHttpClientFactory))).Returns(mockHttpClientFactory.Object);

            mockServiceProvider.Setup(provider => provider.GetService(typeof(IEnumerable<RestClientContainer>)))
                .Returns(new[] { new RestClientContainer(mockServiceProvider.Object, myClient, "http://dalsoft.co.uk", null) } );

            var restClientResult = new RestClientFactory(mockServiceProvider.Object).CreateClient(myClient);
            var httpClientWrapper = (HttpClientWrapper)restClientResult.HttpClientWrapper;

            Assert.That(httpClientWrapper.HttpClient, Is.EqualTo(restClient));
        }

        [Test]
        public void CreateClient_NameLessOverload_ReturnsDefaultClient()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var restClient = new HttpClient();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(provider => provider.CreateClient(RestClientFactory.DefaultClientName)).Returns(restClient);
            mockServiceProvider.Setup(provider => provider.GetService(typeof(IHttpClientFactory))).Returns(mockHttpClientFactory.Object);

            mockServiceProvider.Setup(provider => provider.GetService(typeof(IEnumerable<RestClientContainer>)))
                .Returns(new[] { new RestClientContainer(mockServiceProvider.Object, RestClientFactory.DefaultClientName, "http://dalsoft.co.uk", null) } );

            var restClientResult = new RestClientFactory(mockServiceProvider.Object).CreateClient();
            var httpClientWrapper = (HttpClientWrapper)restClientResult.HttpClientWrapper;

            Assert.That(httpClientWrapper.HttpClient, Is.EqualTo(restClient));
        }
    }
}
