using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.Handlers
{
    [TestFixture]
    public class FormUrlEncodedHandlerTests
    {
        private const string FormUrlEncodedContentType = "application/x-www-form-urlencoded";
        private static readonly Dictionary<string, string> FormUrlEncodedHeader = new Dictionary<string, string> { { "Content-Type", FormUrlEncodedContentType } };
        private static readonly object ObjectNested31Deep = new
        {
            o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { o = new { } } } } } } } } } } } } } } } } } } } } } } } } } } } } } } }
        };

        [Test]
        public async Task Send_DoNotPassFormUrlEncodedContentTypeHeader_HandlerDoesNotSetContentAsExpected()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHandler(), new UnitTestHandler(request => actualRequest = request)) { UseDefaultHandlers = false}
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), null, new { hello = "world" });
            
            Assert.That(actualRequest.Content, Is.Null);
            Assert.That(actualRequest.Headers, Is.Empty);
        }


        [Test]
        public async Task Send_NullContent_HandlerDoesNotSetContentAsExpected()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHandler(), new UnitTestHandler(request => actualRequest = request)) { UseDefaultHandlers = false }
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), FormUrlEncodedHeader, null);

            Assert.That(actualRequest.Content, Is.Null);
            Assert.That(actualRequest.Headers, Is.Empty);
        }

        [Test]
        public void Send_Nested_TooDeeply_ThrowsInvalidOperationException()
        {
            var httpClientWrapper = new HttpClientWrapper(new Config(new FormUrlEncodedHandler(), new UnitTestHandler()));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), FormUrlEncodedHeader, ObjectNested31Deep);
            });
        }

        [Test]
        public async Task Send_ObjectWithAllSupportedTypes_FormatsUrlFormEncodedAsExpected()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHandler(), new UnitTestHandler(request => actualRequest = request))
            );

            var guid = Guid.NewGuid();
            var now = DateTime.UtcNow;

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), FormUrlEncodedHeader,
            new
            {
                thisIsAbool = true,
                thisIsAInt = int.MaxValue,
                thisIsALong = long.MaxValue,
                thisIsADouble = double.MinValue,
                thisIsADecmial = 0.99m,
                thisIsAString = "Hello",
                thisIsDateTime = now,
                thisIsGuid = guid
            });

            var formUrlEncoded = await actualRequest.Content.ReadAsStringAsync();
            var formUrlDictionary = QueryHelpers.ParseQuery(formUrlEncoded);

            Assert.That(formUrlDictionary["thisIsAbool"][0], Is.EqualTo("True"));
            Assert.That(formUrlDictionary["thisIsAInt"][0], Is.EqualTo(int.MaxValue.ToString()));
            Assert.That(formUrlDictionary["thisIsALong"][0], Is.EqualTo(long.MaxValue.ToString()));
            Assert.That(formUrlDictionary["thisIsADouble"][0], Is.EqualTo(double.MinValue.ToString(CultureInfo.InvariantCulture)));
            Assert.That(formUrlDictionary["thisIsADecmial"][0], Is.EqualTo(0.99m.ToString(CultureInfo.InvariantCulture)));
            Assert.That(formUrlDictionary["thisIsAString"][0], Is.EqualTo("Hello"));
            Assert.That(formUrlDictionary["thisIsDateTime"][0], Is.EqualTo(now.ToString("s")));
            Assert.That(formUrlDictionary["thisIsGuid"][0], Is.EqualTo(guid.ToString()));
        }

        [Test]
        public async Task Send_ValuesThatNeedUrlEncoding_FormatsUrlFormEncodedAsExpected()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHandler(), new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), FormUrlEncodedHeader,
            new
            {
                thisIsAString = "!@£$%",
                thisIsAObjectWithStringArray = new[] { "one", "*[&]^", "three" }

            });

            var formUrlEncoded = await actualRequest.Content.ReadAsStringAsync();
            Assert.That(formUrlEncoded, Is.EqualTo("thisIsAString=%21%40%C2%A3%24%25&thisIsAObjectWithStringArray=one&thisIsAObjectWithStringArray=%2A%5B%26%5D%5E&thisIsAObjectWithStringArray=three"));
        }

        [Test]
        public async Task Send_ObjectWithSimpleArray_FormatsUrlFormEncodedAsExpected()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHandler(), new UnitTestHandler(request => actualRequest = request))
            );

            var now = DateTime.UtcNow;

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), FormUrlEncodedHeader,
            new
            {
                thisIsAString = "Hello",
                thisIsIntArray = new[] { 1, 2 },
                thisIsAStringArray = new[] { "one", "two" },
                thisIsADateTimeArray = new[] { now, now },
            });

            var formUrlEncoded = await actualRequest.Content.ReadAsStringAsync();
            var formUrlDictionary = QueryHelpers.ParseQuery(formUrlEncoded);

            Assert.That(formUrlDictionary["thisIsAString"][0], Is.EqualTo("Hello"));
            Assert.That(formUrlDictionary["thisIsIntArray"][0], Is.EqualTo("1"));
            Assert.That(formUrlDictionary["thisIsIntArray"][1], Is.EqualTo("2"));
            Assert.That(formUrlDictionary["thisIsAStringArray"][0], Is.EqualTo("one"));
            Assert.That(formUrlDictionary["thisIsAStringArray"][1], Is.EqualTo("two"));
            Assert.That(formUrlDictionary["thisIsADateTimeArray"][0], Is.EqualTo(now.ToString("s")));
            Assert.That(formUrlDictionary["thisIsADateTimeArray"][1], Is.EqualTo(now.ToString("s")));
        }
        
        [Test]
        public async Task Send_ComplexArray_FormatsAsUrlFormEncodedAsExpected()
        {
            //https://www.hanselman.com/blog/ASPNETWireFormatForModelBindingToArraysListsCollectionsDictionaries.aspx
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHandler(), new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), FormUrlEncodedHeader, 
            new
            {
                people = new[] { new  { FirstName = "George", LastName = "Washington"  }, new { FirstName = "Abraham", LastName = "Lincoln" } }
            });

            var formUrlEncoded = await actualRequest.Content.ReadAsStringAsync();
            
            var formUrlDictionary = QueryHelpers.ParseQuery(formUrlEncoded);

            Assert.That(formUrlDictionary["people[0].FirstName"][0], Is.EqualTo("George"));
            Assert.That(formUrlDictionary["people[0].LastName"][0], Is.EqualTo("Washington"));
            Assert.That(formUrlDictionary["people[1].FirstName"][0], Is.EqualTo("Abraham"));
            Assert.That(formUrlDictionary["people[1].LastName"][0], Is.EqualTo("Lincoln"));
        }

        [Test]
        public async Task Send_NestedObject_FormatsAsUrlFormEncodedAsExpected()
        {
            //http://codebuckets.com/2016/09/07/asp-net-mvc-and-binding-complex-objects-magic/
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHandler(), new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), FormUrlEncodedHeader,
            new
            {
                simple = "hello",
                simpleArray = new[] { 0 },

                nested = new
                {
                    simple = "hello",
                    simpleArray = new[] { 0 },

                    level1 = new
                    {   level1Simple = "hello1",
                        level1SimpleArray = new[] { 1 },
                        level1complexArray = new[] { new  { FirstName = "George1", LastName = "Washington1"  }, new { FirstName = "Abraham1", LastName = "Lincoln1" } },
                        level2 = new
                        {
                            level2Simple = "hello2",
                            level2SimpleArray = new[] { 2 },
                            level2complexArray = new[] { new  { FirstName = "George2", LastName = "Washington2"  }, new { FirstName = "Abraham2", LastName = "Lincoln2" } },
                            level3 = new
                            {
                                level3Simple = "hello3",
                                level3SimpleArray = new[] { 3 },
                                level3complexArray = new[] { new  { FirstName = "George3", LastName = "Washington3"  }, new { FirstName = "Abraham3", LastName = "Lincoln3" } },
                            }
                        }
                    }
                }
            });

            var formUrlEncoded = await actualRequest.Content.ReadAsStringAsync();
            var formUrlDictionary = QueryHelpers.ParseQuery(formUrlEncoded);

            Assert.That(formUrlDictionary["simple"][0], Is.EqualTo("hello"));
            Assert.That(formUrlDictionary["simpleArray"][0], Is.EqualTo("0"));

            Assert.That(formUrlDictionary["nested.simple"][0], Is.EqualTo("hello"));
            Assert.That(formUrlDictionary["nested.simpleArray"][0], Is.EqualTo("0"));

            Assert.That(formUrlDictionary["nested.level1.level1Simple"][0], Is.EqualTo("hello1"));
            Assert.That(formUrlDictionary["nested.level1.level1SimpleArray"][0], Is.EqualTo("1"));
            Assert.That(formUrlDictionary["nested.level1.level1complexArray[0].FirstName"][0], Is.EqualTo("George1"));
            Assert.That(formUrlDictionary["nested.level1.level1complexArray[0].LastName"][0], Is.EqualTo("Washington1"));
            Assert.That(formUrlDictionary["nested.level1.level1complexArray[1].FirstName"][0], Is.EqualTo("Abraham1"));
            Assert.That(formUrlDictionary["nested.level1.level1complexArray[1].LastName"][0], Is.EqualTo("Lincoln1"));

            Assert.That(formUrlDictionary["nested.level1.level2.level2Simple"][0], Is.EqualTo("hello2"));
            Assert.That(formUrlDictionary["nested.level1.level2.level2SimpleArray"][0], Is.EqualTo("2"));
            Assert.That(formUrlDictionary["nested.level1.level2.level2complexArray[0].FirstName"][0], Is.EqualTo("George2"));
            Assert.That(formUrlDictionary["nested.level1.level2.level2complexArray[0].LastName"][0], Is.EqualTo("Washington2"));
            Assert.That(formUrlDictionary["nested.level1.level2.level2complexArray[1].FirstName"][0], Is.EqualTo("Abraham2"));
            Assert.That(formUrlDictionary["nested.level1.level2.level2complexArray[1].LastName"][0], Is.EqualTo("Lincoln2"));

            Assert.That(formUrlDictionary["nested.level1.level2.level3.level3Simple"][0], Is.EqualTo("hello3"));
            Assert.That(formUrlDictionary["nested.level1.level2.level3.level3SimpleArray"][0], Is.EqualTo("3"));
            Assert.That(formUrlDictionary["nested.level1.level2.level3.level3complexArray[0].FirstName"][0], Is.EqualTo("George3"));
            Assert.That(formUrlDictionary["nested.level1.level2.level3.level3complexArray[0].LastName"][0], Is.EqualTo("Washington3"));
            Assert.That(formUrlDictionary["nested.level1.level2.level3.level3complexArray[1].FirstName"][0], Is.EqualTo("Abraham3"));
            Assert.That(formUrlDictionary["nested.level1.level2.level3.level3complexArray[1].LastName"][0], Is.EqualTo("Lincoln3"));
        }

        [Test, Ignore("Bug #30 Simple array nested in a complex array doesn't work as expected")]
        public async Task Send_NestedObjectSimpleArrayInNestedInComplexArray_FormatsAsUrlFormEncodedAsExpected()
        {
            //Bug simple Array nested in complex array loops properties rather the array value
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new FormUrlEncodedHandler(), new UnitTestHandler(request => actualRequest = request))
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), FormUrlEncodedHeader,
            new
            {
                nestedArray = new[] 
                {
                    new
                    {
                        simpleProp = "simple prop",
                        simpleArray = new[] { "simple 0", "simple 1" }

                    }
                }
            });

            var formUrlEncoded = await actualRequest.Content.ReadAsStringAsync();
            var formUrlDictionary = QueryHelpers.ParseQuery(formUrlEncoded);

            Assert.That(formUrlDictionary["nestedArray[0].simpleProp"][0], Is.EqualTo("simple prop"));
            Assert.That(formUrlDictionary["nestedArray[0].simpleProp"][0], Is.EqualTo("simple 0"));
            Assert.That(formUrlDictionary["nestedArray[0].simpleProp"][1], Is.EqualTo("simple 1"));
        }
    }
}

