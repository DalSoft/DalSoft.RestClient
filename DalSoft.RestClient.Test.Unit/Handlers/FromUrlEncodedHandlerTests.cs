using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.Handlers
{
    public class FromUrlEncodedHandlerTests
    {
        private const string FormUrlEncodedContentType = "application/x-www-form-urlencoded";
        private static readonly Dictionary<string, string> FormUrlEncodedHeader = new Dictionary<string, string> { { "Content-Type", FormUrlEncodedContentType } };

        [Test]
        public async Task Send_DoNotPassFormUrlEncodedContentTypeHeader_HandlerDoesNotSetContentAsExpected()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHander(), new UnitTestHandler(request => actualRequest = request)) { UseDefaultHandlers = false}
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), null, new { hello = "world" });
            
            Assert.That(actualRequest.Content, Is.Null);
            Assert.That(actualRequest.Headers, Is.Empty);
        }

        [Test]
        public async Task Send_Nested_TooDeeply_Throws()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHander(), new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), FormUrlEncodedHeader,
                new
                {
                    hello3 = new { complex = new { @is = new  { x=new { y = "s"} } } },
                });

            var formUrlEncoded = actualRequest.Content.ReadAsStringAsync().Result;

        }


        [Test]
        public async Task Send_Nest()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHander(), new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), FormUrlEncodedHeader, 
                new
                {
                    hello3 = new { complex= new { @is="OK" } },
                    hello4 = new { @is= new[] { new { @is = "OK" }, new { @is = "OK" } } },
                    hello5 = new { complex= new[] { 1,2,3 } },
                    complex= new[] { 1,2,3 }
                });

            var formUrlEncoded = actualRequest.Content.ReadAsStringAsync().Result;
                
        }
    }
}

