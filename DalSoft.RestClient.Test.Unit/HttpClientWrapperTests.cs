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
            dynamic restClient = new RestClient("https://www.google.com") { Timeout = TimeSpan.FromSeconds(3) };

            var httpClient = ((RestClient)restClient).GetHttpClient();

            restClient.Get();

            Assert.That(httpClient.Timeout, Is.EqualTo(restClient.Timeout));
        }
    }
}
