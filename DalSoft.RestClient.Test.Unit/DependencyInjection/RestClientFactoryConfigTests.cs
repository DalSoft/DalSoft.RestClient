using System;
using System.Linq;
using DalSoft.RestClient.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.DependencyInjection
{
    public class RestClientFactoryConfigTests
    {
        [Test]
        public void Ctor_NullHttpClientBuilder_ThrowsArgumentNullExceptions()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new RestClientFactoryConfig(null));
        }

        [Test]
        public void Ctor_PassHttpClientBuilder_SetsHttpClientBuilderProperty()
        {
            var serviceCollection = new ServiceCollection();
            var httpClientBuilder = serviceCollection.AddHttpClient("myclient1");
            var restClientFactoryConfig = new RestClientFactoryConfig(httpClientBuilder);

            Assert.That(restClientFactoryConfig.HttpClientBuilder, Is.EqualTo(httpClientBuilder));
        }

        [Test]
        public void Ctor_ByDefault_SetsUseDefaultHandlersToTrue()
        {
            var serviceCollection = new ServiceCollection();
            var restClientFactoryConfig = serviceCollection.AddRestClient("myclient1", "http://dalsoft.co.uk");

            Assert.That(restClientFactoryConfig.UseDefaultHandlers, Is.EqualTo(true));
        }

        [Test]
        public void Ctor_ByDefault_AddsDefaultJsonHandlerToHttpClientBuilder()
        {
            const string clientName = "myclient1";

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddRestClient("myclient1", "http://dalsoft.co.uk");

            var httpClientFactoryOptions = serviceCollection.Where(_ => _.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>))
               .Select(_=>_.ImplementationInstance).Cast<ConfigureNamedOptions<HttpClientFactoryOptions>>().ToList();

            Assert.That(httpClientFactoryOptions.Count(), Is.EqualTo(1));
            Assert.That(httpClientFactoryOptions.First().Name, Is.EqualTo(clientName));
        }
    }
}
