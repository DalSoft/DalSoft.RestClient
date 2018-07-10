using System;
using System.Net.Http;
using DalSoft.RestClient.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.DependencyInjection
{
    public class RestClientContainerTests
    {
        [Test]
        public void Ctor_PassingName_SetsNameProperty()
        {
            const string name = "MyClient1";

            var restClientContainer = new RestClientContainer(new ServiceCollection().BuildServiceProvider(), name, "http://dalsoft.co.uk", null);

            Assert.That(restClientContainer.Name, Is.EqualTo(name));
        }

        [Test]
        public void Ctor_PassingCorrectParams_RestClientFuncUsesNamedHttpClientFromIHttpClientFactory()
        {
            const string name = "MyClient1";

            var mockServiceProvider = new Mock<IServiceProvider>();
            var restClient = new HttpClient();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(provider => provider.CreateClient(name)).Returns(restClient);
            mockServiceProvider.Setup(provider => provider.GetService(typeof(IHttpClientFactory))).Returns(mockHttpClientFactory.Object);
            
            var restClientContainer = new RestClientContainer(mockServiceProvider.Object, name, "http://dalsoft.co.uk", null);

            var httpClientWrapper = (HttpClientWrapper)restClientContainer.RestClient().HttpClientWrapper;

            Assert.That(httpClientWrapper.HttpClient, Is.EqualTo(restClient));
        }

        [Test]
        public void Ctor_PassingBaseUri_RestClientFuncUsesPassedBaseUri()
        {
            const string name = "MyClient1";
            const string baseUri = "http://dalsoft.co.uk";

            var mockServiceProvider = new Mock<IServiceProvider>();
            var restClient = new HttpClient();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(provider => provider.CreateClient(name)).Returns(restClient);
            mockServiceProvider.Setup(provider => provider.GetService(typeof(IHttpClientFactory))).Returns(mockHttpClientFactory.Object);

            var restClientContainer = new RestClientContainer(mockServiceProvider.Object, name, baseUri, null);

            var restClientResult = restClientContainer.RestClient();

            Assert.That(restClientResult.BaseUri, Is.EqualTo(baseUri));
        }

        [Test]
        public void Ctor_PassingDefaultHeaders_RestClientFuncUsesPassedDefaultHeaders()
        {
            const string clientName = "MyClient1";
            const string baseUri = "http://dalsoft.co.uk";

            var mockServiceProvider = new Mock<IServiceProvider>();
            var restClient = new HttpClient();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(provider => provider.CreateClient(clientName)).Returns(restClient);
            mockServiceProvider.Setup(provider => provider.GetService(typeof(IHttpClientFactory))).Returns(mockHttpClientFactory.Object);

            var restClientContainer = new RestClientContainer(mockServiceProvider.Object, clientName, baseUri, new Headers(new { name = clientName }));

            var restClientResult = restClientContainer.RestClient();

            Assert.That(restClientResult.DefaultRequestHeaders["name"], Is.EqualTo(clientName));
        }
    }
}
