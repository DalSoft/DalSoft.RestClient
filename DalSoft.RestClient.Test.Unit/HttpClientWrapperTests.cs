using System;
using System.Collections.Generic;
using System.Net.Http;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit
{
    public class HttpClientWrapperTests
    {
        [Test]
        public void Ctor_PassingBaseUriAndHttpMessageHandler_ShouldPassHttpMessageHandlerToHttpClient()
        {
            var expectedHandler = new HttpClientHandler();
            var restClient = new RestClient("http://test.test", expectedHandler);

            var actualHandler = restClient.GetHandler();

            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        [Test]
        public void Ctor_PassingBaseUriAndHttpMessageHandlerAndDefaultRequestHeaders_ShouldPassHttpMessageHandlerToHttpClient()
        {
            var expectedHandler = new HttpClientHandler();
            var restClient = new RestClient("http://test.test", new Dictionary<string, string>(), expectedHandler);

            var actualHandler = restClient.GetHandler();

            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        [Test]
        public void Timeout_SettingBeforeSend_ShouldPassTimeoutToHttpClient()
        {
            var restClient = new RestClient("https://www.google.com", new Config (timeout:TimeSpan.FromSeconds(3)));

            var httpClient = restClient.GetHttpClient();

            Assert.That(httpClient.Timeout, Is.EqualTo(restClient.Config.Timeout));
        }
    }
}
