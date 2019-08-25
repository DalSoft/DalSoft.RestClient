using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Integration
{
    public class RestClientFactoryTests
    {
        private const string Name1 = "MyClient1";
        private const string Name2 = "MyClient2";
        
        [Test]
        public async Task CreateClient_InvokingUsingHttpClientHandler_CreatesUsableRestClientUsingHandler()
        {
            var services = new ServiceCollection();
            var cookieContainer = new CookieContainer();
            
            services
                .AddRestClient(Name1, "https://httpbin.org/cookies/set?testcookie=darran")
                .UseHttpClientHandler(() => new HttpClientHandler { CookieContainer = cookieContainer });

            dynamic restClient = services.BuildServiceProvider().GetService<IRestClientFactory>().CreateClient(Name1);

            await restClient.Get();
            
            Assert.That(cookieContainer.GetCookies(new Uri("https://httpbin.org"))["testcookie"]?.Value, Is.EqualTo("darran"));
        }

        [Test]
        public async Task CreateClient_MoreThanOneRegisteredClient_InvokesCorrectRestClient()
        {
            var services = new ServiceCollection();

            services.AddRestClient(Name1, "http://jsonplaceholder.typicode.com");

            services.AddRestClient(Name2, "https://www.google.com");
            
            dynamic restClient1 = services.BuildServiceProvider().GetService<IRestClientFactory>().CreateClient(Name1);
            var user = await restClient1.Users.Get(1);

            Assert.That(user.id, Is.EqualTo(1));

            dynamic restClient2 = services.BuildServiceProvider().GetService<IRestClientFactory>().CreateClient(Name2);
            
            var result = await restClient2.Headers(new Headers { { "Accept", "text/html" } } ).news.Get();
            var content = result.ToString();

            Assert.That(content, Does.Contain("News"));
        }

        [Test]
        public async Task CreateClient_SetCookiesUsingCookieHandler_CorrectlySetsCookie()
        {
            var services = new ServiceCollection();

            services.AddRestClient(Name1, "https://httpbin.org/cookies/set?testcookie=darran")
                .UseCookieHandler();

            var restClient1 = services.BuildServiceProvider().GetService<IRestClientFactory>().CreateClient(Name1);
            HttpResponseMessage response = await restClient1.Get();

            Assert.AreEqual("darran", response.GetCookieContainer()?.GetCookies(new Uri("https://httpbin.org"))["testcookie"]?.Value);
        }
    }
}
