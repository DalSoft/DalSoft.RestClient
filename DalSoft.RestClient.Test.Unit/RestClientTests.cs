using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using DalSoft.RestClient.Test.Unit.TestData.Models;
using DalSoft.RestClient.Test.Unit.TestData.Resources;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DalSoft.RestClient.Test.Unit
{
    /* This now tests both dynamic and strongly-typed access. i.e. regardless of how declared restClient.Get() will always call strong type on RestClient,
       and when dynamically chained i.e. restClient.Users.Get() will always call dynamically. Read comments in IRestClient to understand more. */
    
    [TestFixture]
    public class RestClientTests
    {
        public const string BaseUri = "http://test.test";
        
        [TestCase(true), TestCase(false)]
        public async Task Query_ShouldSerializeObjectToQueryString(bool callDynamically)
        {
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<object>()))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage()}));

            dynamic client = new RestClient(mockHttpClient.Object, BaseUri);

            if (callDynamically)
                await client.Users.Query(new { Id = "test", another = 1 }).Get();
            else
                await ((IRestClient)client).Query(new { Id = "test", another = 1 }).Get();

            mockHttpClient.Verify(_ => _.Send
            (
                HttpMethod.Get, 
                It.Is<Uri>(__ => __ == new Uri($"{BaseUri}{(callDynamically ? "/Users" : string.Empty)}?Id=test&another=1")),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<object>()
            ));
        }

        [TestCase(true), TestCase(false)]
        public async Task Query_ShouldSerializeArrayToQueryString(bool callDynamically)
        {
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<object>()))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage() }));

            dynamic client = new RestClient(mockHttpClient.Object, BaseUri);

            if (callDynamically)
                await client.Users.Query(new { variables = new[] { "one", "other" }, otherVar = "stillWorks" }).Get();
            else
                await ((IRestClient)client).Resource("Users").Query(new { variables = new[] { "one", "other" }, otherVar = "stillWorks" }).Get();

            mockHttpClient.Verify(_ => _.Send
            (
                HttpMethod.Get,
                It.Is<Uri>(__ => __ == new Uri(BaseUri + "/Users?variables=one&variables=other&otherVar=stillWorks")),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<object>()
            ));
        }


        [TestCase(true), TestCase(false)]
        public async Task Query_StringThatRequiresEncoding_EncodesStringCorrectly(bool callDynamically)
        {
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<object>()))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage() }));

            dynamic client = new RestClient(mockHttpClient.Object, BaseUri);

            if (callDynamically)
                await client.Users.Query(new { variables = new[] { "!@£$%", "*[&]^" }, otherVar = "ƻƻƳƳ" }).Get();
            else
                await ((IRestClient)client).Resource("Users").Query(new { variables = new[] { "!@£$%", "*[&]^" }, otherVar = "ƻƻƳƳ" }).Get();

        // TODO: Investigate 
        #if NET461
            mockHttpClient.Verify(_ => _.Send
            (
                HttpMethod.Get,
                It.Is<Uri>(__ => __ == new Uri(BaseUri + "/Users?variables=!%40£%24%25&variables=*[%26]^&otherVar=ƻƻƳƳ")),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<object>()
            ));
        #else
            mockHttpClient.Verify(_ => _.Send
            (
                HttpMethod.Get,
                It.Is<Uri>(__ => __ == new Uri(BaseUri + "/Users?variables=%21%40£%24%25&variables=%2A%5B%26%5D^&otherVar=ƻƻƳƳ")),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<object>()
            ));
        #endif
        }

        [TestCase(true), TestCase(false)]
        public async Task ToString_NullContent_ReturnsEmptyString(bool callDynamically)
        {
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), null))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage() }));

            dynamic client = new RestClient(mockHttpClient.Object, BaseUri);

            dynamic result;

            if (callDynamically)
                result = await client.Users.Get();
            else
                result = await ((IRestClient)client).Resource("Users").Get();

            Assert.That(result.ToString(), Is.EqualTo(string.Empty));
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_SingleObjectAsDynamic_ReturnsDynamicCorrectly(bool callDynamically)
        {
           dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));
           var verbs = new Func<Task<dynamic>>[]
           {
               () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
               () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
               () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
               () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

               () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
               () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
               () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
               () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
           };
       
           foreach (var verb in verbs)
            {
                var user = await verb();
                Assert.That(user.id, Is.EqualTo(1));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_SingleObjectCastingToStrongType_CastsObjectCorrectly(bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));
            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                User user = await verb();
                Assert.That(user.id, Is.EqualTo(1));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_CastToString_CastsObjectCorrectly(bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));
            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                string responseBody = await verb();
                Assert.AreEqual("{\"id\":1}", responseBody);
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_AccessMissingMember_ReturnsNull(bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));
            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                var user = await verb();
                Assert.That(user.IAmAMissingMember, Is.Null);
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_SingleObjectImplicitCast_ReturnsTypeCorrectly(bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));
            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                User user = await verb();
                Assert.That(user.id, Is.EqualTo(1));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_ArrayOfObjectsImplicitCastToList_ReturnsTypeCorrectly(bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUsersResponse())));
            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                List<User> users = await verb();
                Assert.That(users.ElementAt(1).id, Is.EqualTo(2));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_ArrayOfObjectAccessByIndex_ReturnsValueByIndexCorrectly(bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUsersResponse())));
            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                List<User> users = await verb();
                Assert.That(users[0].id, Is.EqualTo(1));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_ArrayOfObjectsAsDynamicAccessByIndex_ReturnsValueByIndexCorrectly(bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUsersResponse())));
            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                var users = await verb();
                Assert.That(users[0].id, Is.EqualTo(1));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_ArrayOfObjectsEnumeratingUsingForEach_CorrectlyEnumeratesOverEachItem(bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUsersResponse())));
            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                    () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                var i = 1;

                var users = await verb();

                foreach (var user in users)
                {
                    Assert.That(user.id, Is.EqualTo(i));
                    i++;
                }
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_SingleObjectCastAsHttpResponseMessage_CastAsHttpResponseMessageCorrectly(bool callDynamically)
        {
            var response = GetMockUserResponse();
            response.StatusCode = HttpStatusCode.BadGateway;

            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => response)));

            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                HttpResponseMessage httpResponseMessage = await verb();
                Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task Get_SingleObjectGetHttpResponseMessageDynamically_GetsHttpResponseMessageCorrectly(bool callDynamically)
        {
            var response = GetMockUserResponse();
            response.StatusCode = HttpStatusCode.BadGateway;

            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => response)));
            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                var result = await verb();
                Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
            }
        }

        [TestCase(true), TestCase(false)]
        public void AllVerbs_SingleObjectSynchronously_GetsObjectCorrectly(bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));
            var verbs = new Func<Task<dynamic>>[]
            {
                () => callDynamically ? client.Users.Get(1) : ((IRestClient)client).Get(),
                () => callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("users/1").Get(),
                () => callDynamically ? client.Users.Delete(1) : ((IRestClient)client).Delete(),
                () => callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("users/1").Delete(),

                () => callDynamically ? client.Users(1).Post() : ((IRestClient)client).Post(),
                () => callDynamically ? client.Users(1).Put() : ((IRestClient)client).Put(),
                () => callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Patch(),
                () => callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Merge()
            };

            foreach (var verb in verbs)
            {
                var result = verb().Result;
                Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result.id, Is.EqualTo(1));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task Get_NonJsonContentFromGoogle_GetsContentCorrectly(bool callDynamically)
        {
            var nonJsonContent = new HttpResponseMessage
            {
                Content = new StringContent(("<html><body>Top Stories</body></html>"))
            };

            dynamic google = new RestClient
            (
                BaseUri, new Dictionary<string, string> { { "Accept", "text/html" } }, 
                new Config(new UnitTestHandler(request => nonJsonContent))
            );

            dynamic result;

            if (callDynamically)
                result = await google.news.Get();
            else
                result = await ((IRestClient)google).Resource("news").Get();

            var content = result.ToString();
            
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Does.Contain("Top Stories"));
        }

        [Test]
        public void AllVerbs_DynamicallyChainingMethodsPassingNonPrimitive_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users(new { id = 1 }).Get(),
                ()=>client.Users(new { id = 1 }).Head(),
                ()=>client.Users(new { id = 1 }).Delete(),
                ()=>client.Users(new { id = 1 }).Post(),
                ()=>client.Users(new { id = 1 }).Put(),
                ()=>client.Users(new { id = 1 }).Patch(),
                ()=>client.Users(new { id = 1 }).Merge()
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void ImmutableVerbs_DynamicallyChainingMethodsPassingNonPrimitive_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                 ()=>client.Users().Get(new { id = 1 }),
                 ()=>client.Users().Head(new { id = 1 }),
                 ()=>client.Users().Delete(new { id = 1 })
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void MutableVerbs_DynamicallyChainingMethodsPassingPrimitive_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Post(1),
                ()=>client.Users.Put(1),
                ()=>client.Users.Patch(1),
                ()=>client.Users.Merge(1)
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task MutableVerbs_ChainingMethodsPassingEmptyEnumerable_SetsEmptyEnumerableCorrectly(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri + "/", new Config(new UnitTestHandler(request => resultingRequest = request)));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=> callDynamically ? client.Users.Post(new { my_array = new string[]{} }) : ((IRestClient)client).Resource("Users").Post(new { my_array = new string[]{} }),
                ()=> callDynamically ? client.Users(1).Put(new { my_array = new string[]{} }) : ((IRestClient)client).Resource("Users/1").Put(new { my_array = new string[]{} }),
                ()=> callDynamically ? client.Users(1).Patch(new { my_array = new string[]{} }) : ((IRestClient)client).Resource("Users/1").Patch(new { my_array = new string[]{} }),
                ()=> callDynamically ? client.Users(1).Merge(new { my_array = new string[]{} }) : ((IRestClient)client).Resource("Users/1").Merge(new { my_array = new string[]{} })
            };

            foreach (var verb in verbs)
            {
                await verb();
                var requestBody = await resultingRequest.Content.ReadAsStringAsync();
                Assert.That(requestBody, Is.EqualTo("{\"my_array\":[]}"));
            }
        }
        
        [Test]
        public void AllVerbs_DynamicallyChainingAndSecondArgNotHeaderDictionary_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Get(1, new {}),
                ()=>client.Users.Head(1, new {}),
                ()=>client.Users.Delete(1, new {}),
                ()=>client.Users.Post(new {}, new {}),
                ()=>client.Users.Put(new {}, new {}),
                ()=>client.Users.Patch(new {}, new {}),
                ()=>client.Users.Merge(new {}, new {})
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void AllVerbs_DynamicallyChainingWithMoreThan2Args_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Get(1, new Dictionary<string, string>(), 1),
                ()=>client.Users.Head(1, new Dictionary<string, string>(), 1),
                ()=>client.Users.Delete(1, new Dictionary<string, string>(), 1),
                ()=>client.Users.Post(new {}, new Dictionary<string, string>(), 1),
                ()=>client.Users.Put(new {}, new Dictionary<string, string>(), 1),
                ()=>client.Users.Patch(new {}, new Dictionary<string, string>(), 1),
                ()=>client.Users.Merge(new {}, new Dictionary<string, string>(), 1),
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [TestCase("this is resource requires escaping", true), TestCase("this is resource requires escaping", false)]
        public async Task AllVerbs_ResourceRequiresEscaping_CorrectlyEscapesUri(string resource, bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Resource(resource).Get() : ((IRestClient)client).Resource(resource).Get(),
                async () =>
                {
                    if (callDynamically)
                        return await client.Resource(resource).Head();

                    return await ((IRestClient)client).Resource(resource).Head();
                },
                ()=>callDynamically ? client.Resource(resource).Delete() : ((IRestClient)client).Resource(resource).Delete(),
                ()=>callDynamically ? client.Resource(resource).Post() : ((IRestClient)client).Resource(resource).Post(),
                ()=>callDynamically ? client.Resource(resource).Put(new {}) : ((IRestClient)client).Resource(resource).Put(),
                ()=>callDynamically ? client.Resource(resource).Patch(new {}) : ((IRestClient)client).Resource(resource).Patch(),
                ()=>callDynamically ? client.Resource(resource).Merge(new {}) : ((IRestClient)client).Resource(resource).Merge(),
            };

            foreach (var verb in verbs)
            {
                HttpResponseMessage response = await verb();
                Assert.AreEqual(BaseUri + "/this%20is%20resource%20requires%20escaping", response.RequestMessage.RequestUri.AbsoluteUri);
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_NullResource_NotAddedToUriChain(bool callDynamically)
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Resource(null).Get() : ((IRestClient)client).Resource(null).Get(),
                async () =>
                {
                    if (callDynamically)
                        return await client.Resource(null).Head();

                    return await ((IRestClient)client).Resource(null).Head();
                },
                ()=>callDynamically ? client.Resource(null).Delete() : ((IRestClient)client).Resource(null).Delete(),
                ()=>callDynamically ? client.Resource(null).Post() : ((IRestClient)client).Resource(null).Post(),
                ()=>callDynamically ? client.Resource(null).Put(new {}) : ((IRestClient)client).Resource(null).Put(),
                ()=>callDynamically ? client.Resource(null).Patch(new {}) : ((IRestClient)client).Resource(null).Patch(),
                ()=>callDynamically ? client.Resource(null).Merge(new {}) : ((IRestClient)client).Resource(null).Merge()
            };

            foreach (var verb in verbs)
            {
                HttpResponseMessage response = await verb();
                Assert.AreEqual(BaseUri + "/", response.RequestMessage.RequestUri.ToString());
            }
        }

        [Description("Test for #84")]
        [TestCase("this is a invalidUri", true, typeof(UriFormatException)), TestCase("this is a invalidUri", false, typeof(UriFormatException))]
        [TestCase(null, true, typeof(ArgumentNullException)), TestCase(null, false, typeof(ArgumentNullException))]
        public void RestClient_HttpClientWithInvalidBasedUri_ThrowsCorrectException(string uri, bool callDynamically, Type exceptionType)
        {
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws(exceptionType, () => new RestClient(uri));
        }

        [Description("Test for #49")]
        [TestCase(true), TestCase(false)]
        public async Task AllMutableVerbs_PostingAStringInsteadOfObject_CorrectlyPostsStringBody(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));

            const string stringToBePosted = "{ \"name\":\"John\"  }";
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Resource("Users").Post(stringToBePosted) : ((IRestClient)client).Resource("Users").Post(stringToBePosted),
                ()=>callDynamically ? client.Resource("Users").Put(stringToBePosted) : ((IRestClient)client).Resource("Users").Put(stringToBePosted),
                ()=>callDynamically ? client.Resource("Users").Patch(stringToBePosted) : ((IRestClient)client).Resource("Users").Patch(stringToBePosted),
                ()=>callDynamically ? client.Resource("Users").Merge(stringToBePosted) : ((IRestClient)client).Resource("Users").Merge(stringToBePosted)
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.AreEqual(stringToBePosted, await resultingRequest.Content.ReadAsStringAsync());
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_ChainingMethodsWithTrailingSlashInBaseUri_GeneratesCorrectUri(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri + "/", new Config(new UnitTestHandler(request => resultingRequest = request)));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("Users/1").Get(),
                // ReSharper disable once ConditionalTernaryEqualBranch
                ()=>callDynamically ? client.Users(1).Head() : client.Users(1).Head(),  // ((IRestClient)client).Resource("Users/1").Head() returns HttpResponseMessage
                ()=>callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("Users/1").Delete(),
                ()=>callDynamically ? client.Users(1).Post() : ((IRestClient)client).Resource("Users/1").Post(),
                ()=>callDynamically ? client.Users(1).Put() : ((IRestClient)client).Resource("Users/1").Put(),
                ()=>callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Resource("Users/1").Patch(),
                ()=>callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Resource("Users/1").Merge()
            };
            
            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.RequestUri.ToString().ToLower(), Is.EqualTo(BaseUri + "/users/1"));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_ChainingMethods_GeneratesCorrectUri(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Users(1).Get() : ((IRestClient)client).Resource("Users/1").Get(),
                // ReSharper disable once ConditionalTernaryEqualBranch
                ()=>callDynamically ? client.Users(1).Head() : client.Users(1).Head(),  // ((IRestClient)client).Resource("Users/1").Head() returns HttpResponseMessage
                ()=>callDynamically ? client.Users(1).Delete() : ((IRestClient)client).Resource("Users/1").Delete(),
                ()=>callDynamically ? client.Users(1).Post() : ((IRestClient)client).Resource("Users/1").Post(),
                ()=>callDynamically ? client.Users(1).Put() : ((IRestClient)client).Resource("Users/1").Put(),
                ()=>callDynamically ? client.Users(1).Patch() : ((IRestClient)client).Resource("Users/1").Patch(),
                ()=>callDynamically ? client.Users(1).Merge() : ((IRestClient)client).Resource("Users/1").Merge()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.RequestUri.ToString().ToLower(), Is.EqualTo(BaseUri + "/users/1"));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_ChainingMethodsUsingGuid_GeneratesCorrectUri(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));

            var testGuid = Guid.NewGuid();

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Users(testGuid).Get() : ((IRestClient)client).Resource($"Users/{testGuid}").Get(),
                // ReSharper disable once ConditionalTernaryEqualBranch
                ()=>callDynamically ? client.Users(testGuid).Head() : client.Users(testGuid).Head(),  // ((IRestClient)client).Resource("Users/1").Head() returns HttpResponseMessage
                ()=>callDynamically ? client.Users(testGuid).Delete() : ((IRestClient)client).Resource($"Users/{testGuid}").Delete(),
                ()=>callDynamically ? client.Users(testGuid).Post() : ((IRestClient)client).Resource($"Users/{testGuid}").Post(),
                ()=>callDynamically ? client.Users(testGuid).Put() : ((IRestClient)client).Resource($"Users/{testGuid}").Put(),
                ()=>callDynamically ? client.Users(testGuid).Patch() : ((IRestClient)client).Resource($"Users/{testGuid}").Patch(),
                ()=>callDynamically ? client.Users(testGuid).Merge() : ((IRestClient)client).Resource($"Users/{testGuid}").Merge()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.RequestUri.ToString().ToLower(), Is.EqualTo($"{BaseUri}/users/{testGuid}"));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_ChainingMethodsUsingEnum_GeneratesCorrectUri(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));

            var testEnum = HttpStatusCode.Accepted;

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Users(testEnum).Get() : ((IRestClient)client).Resource($"Users/{testEnum}").Get(),
                // ReSharper disable once ConditionalTernaryEqualBranch
                ()=>callDynamically ? client.Users(testEnum).Head() : client.Users(testEnum).Head(),  
                ()=>callDynamically ? client.Users(testEnum).Delete() : ((IRestClient)client).Resource($"Users/{testEnum}").Delete(),
                ()=>callDynamically ? client.Users(testEnum).Post() : ((IRestClient)client).Resource($"Users/{testEnum}").Post(),
                ()=>callDynamically ? client.Users(testEnum).Put() : ((IRestClient)client).Resource($"Users/{testEnum}").Put(),
                ()=>callDynamically ? client.Users(testEnum).Patch() : ((IRestClient)client).Resource($"Users/{testEnum}").Patch(),
                ()=>callDynamically ? client.Users(testEnum).Merge() : ((IRestClient)client).Resource($"Users/{testEnum}").Merge(),
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.RequestUri.ToString().ToLower(), Is.EqualTo($"{BaseUri}/users/{testEnum}".ToLower()));
            }
        }

        [Test]
        public async Task ImmutableVerbs_ChainingMethodsPassingParamInVerb_GeneratesCorrectUri()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users().Get(1),
                ()=>client.Users().Head(1),
                ()=>client.Users().Delete(1),
                ()=>client.Users.Get(1),
                ()=>client.Users.Head(1),
                ()=>client.Users.Delete(1)
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.RequestUri.ToString().ToLower(), Is.EqualTo(BaseUri + "/users/1"));
            }
        }

        [Test]
        public void Resource_NonPrimitiveArg_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Resource(new { id=1 }).Get(),
                ()=>client.Users.Resource(new { id=1 }).Head(),
                ()=>client.Users.Resource(new { id=1 }).Delete(),
                ()=>client.Users.Resource(new { id=1 }).Post(),
                ()=>client.Users.Resource(new { id=1 }).Put(),
                ()=>client.Users.Resource(new { id=1 }).Patch(),
                ()=>client.Users.Resource(new { id=1 }).Merge()
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void Resource_MoreThanOneArg_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            Assert.ThrowsAsync<ArgumentException>(async () => await client.Users.Resource(1, 1).Get());

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Resource(1, 1).Get(),
                ()=>client.Users.Resource(1, 1).Head(),
                ()=>client.Users.Resource(1, 1).Delete(),
                ()=>client.Users.Resource(1, 1).Post(),
                ()=>client.Users.Resource(1, 1).Put(),
                ()=>client.Users.Resource(1, 1).Patch(),
                ()=>client.Users.Resource(1, 1).Merge()

            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void Resource_NoArgs_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            Assert.ThrowsAsync<ArgumentException>(async () => await client.Users.Resource(1, 1).Get());

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Resource().Get(),
                ()=>client.Users.Resource().Head(),
                ()=>client.Users.Resource().Delete(),
                ()=>client.Users.Resource().Post(),
                ()=>client.Users.Resource().Put(),
                ()=>client.Users.Resource().Patch(),
                ()=>client.Users.Resource().Merge()
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task Resource_CorrectArgs_GeneratesCorrectUri(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Users.Resource("this-is-not-valid-in-csharp").Get() : ((IRestClient)client).Resource("Users/this-is-not-valid-in-csharp").Get(),
                // ()=>callDynamically ? client.Users.Resource("this-is-not-valid-in-csharp").Users.Head() : ((IRestClient)client).Resource("Users/this-is-not-valid-in-csharp").Head(),
                ()=>callDynamically ? client.Users.Resource("this-is-not-valid-in-csharp").Delete() : ((IRestClient)client).Resource("Users/this-is-not-valid-in-csharp").Delete(),
                ()=>callDynamically ? client.Users.Resource("this-is-not-valid-in-csharp").Post() : ((IRestClient)client).Resource("Users/this-is-not-valid-in-csharp").Post(),
                ()=>callDynamically ? client.Users.Resource("this-is-not-valid-in-csharp").Put() : ((IRestClient)client).Resource("Users/this-is-not-valid-in-csharp").Put(),
                ()=>callDynamically ? client.Users.Resource("this-is-not-valid-in-csharp").Patch() : ((IRestClient)client).Resource("Users/this-is-not-valid-in-csharp").Patch(),
                ()=>callDynamically ? client.Users.Resource("this-is-not-valid-in-csharp").Merge() : ((IRestClient)client).Resource("Users/this-is-not-valid-in-csharp").Merge()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.RequestUri.ToString().ToLower(), Is.EqualTo(BaseUri + "/users/this-is-not-valid-in-csharp"));
            }
        }

        [Test]
        public void Query_NonAnonymousArg_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Query(new User { id = 1 }).Get(),
                ()=>client.Users.Query(new User { id = 1 }).Head(),
                ()=>client.Users.Query(new User { id = 1 }).Delete(),
                ()=>client.Users.Query(new User { id = 1 }).Post(),
                ()=>client.Users.Query(new User { id = 1 }).Put(),
                ()=>client.Users.Query(new User { id = 1 }).Patch(),
                ()=>client.Users.Query(new User { id = 1 }).Merge()
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void Query_MoreThanOneArg_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Query(new { id = 1 }, new { id = 1 }).Get(),
                ()=>client.Users.Query(new { id = 1 }, new { id = 1 }).Head(),
                ()=>client.Users.Query(new { id = 1 }, new { id = 1 }).Delete(),
                ()=>client.Users.Query(new { id = 1 }, new { id = 1 }).Post(),
                ()=>client.Users.Query(new { id = 1 }, new { id = 1 }).Put(),
                ()=>client.Users.Query(new { id = 1 }, new { id = 1 }).Patch(),
                ()=>client.Users.Query(new { id = 1 }, new { id = 1 }).Merge()
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void Query_NoArgs_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Query().Get(),
                ()=>client.Users.Query().Head(),
                ()=>client.Users.Query().Delete(),
                ()=>client.Users.Query().Post(),
                ()=>client.Users.Query().Put(),
                ()=>client.Users.Query().Patch(),
                ()=>client.Users.Query().Merge()
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task Query_CorrectArgs_GeneratesCorrectUri(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Users(1).Query(new { my="query string" }).Get() : ((IRestClient)client).Resource("Users/1").Query(new { my="query string" }).Get(),
                //()=>callDynamically ? client.Users(1).Query(new { my="query string" }).Head() : ((IRestClient)client).Resource("Users/1").Query(new { my="query string" }).Head(),
                ()=>callDynamically ? client.Users(1).Query(new { my="query string" }).Delete() : ((IRestClient)client).Resource("Users/1").Query(new { my = "query string" }).Delete(),
                ()=>callDynamically ? client.Users(1).Query(new { my="query string" }).Post() : ((IRestClient)client).Resource("Users/1").Query(new { my = "query string" }).Post(),
                ()=>callDynamically ? client.Users(1).Query(new { my="query string" }).Put() : ((IRestClient)client).Resource("Users/1").Query(new { my = "query string" }).Put(),
                ()=>callDynamically ? client.Users(1).Query(new { my="query string" }).Patch() : ((IRestClient)client).Resource("Users/1").Query(new { my = "query string" }).Patch(),
                ()=>callDynamically ? client.Users(1).Query(new { my="query string" }).Merge() : ((IRestClient)client).Resource("Users/1").Query(new { my = "query string" }).Merge()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.RequestUri.ToString().ToLower(), Is.EqualTo(BaseUri + "/users/1?my=query+string"));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_SetDefaultHeadersViaCtor_CorrectlySetsHeaders(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri,
                                             new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } },
                                             new Config(new UnitTestHandler(request => resultingRequest = request))
            );

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Users.Get() : ((IRestClient)client).Resource("Users").Get(),
                //()=>callDynamically ? client.Users.Head() : ((IRestClient)client).Resource("Users").Head(),
                ()=>callDynamically ? client.Users.Delete() : ((IRestClient)client).Resource("Users").Delete(),
                ()=>callDynamically ? client.Users.Post() : ((IRestClient)client).Resource("Users").Post(),
                ()=>callDynamically ? client.Users.Put() : ((IRestClient)client).Resource("Users").Put(),
                ()=>callDynamically ? client.Users.Patch() : ((IRestClient)client).Resource("Users").Patch(),
                ()=>callDynamically ? client.Users.Merge() : ((IRestClient)client).Resource("Users").Merge()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
                Assert.That(resultingRequest.Headers.GetValues("MyDummyHeader").First(), Is.EqualTo("MyValue"));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_SetHeadersUsingHeadersMethodDictionary_CorrectlySetsHeaders(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} }).Headers(new Dictionary<string, string> { { "Accept", "application/json" } }).Users.Get() : ((IRestClient)client).Headers(new Headers { { "MyDummyHeader", "MyValue"}, { "Accept", "application/json" } }).Resource("Users").Get(),
                //()=>callDynamically ? client.Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} }).Headers(new Dictionary<string, string> { { "Accept", "application/json" } }).Head() : ((IRestClient)client).Headers(new Headers { { "MyDummyHeader", "MyValue"}, { "Accept", "application/json" } }).Head(),
                ()=>callDynamically ? client.Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} }).Headers(new Dictionary<string, string> { { "Accept", "application/json" } }).Users.Delete() : ((IRestClient)client).Headers(new Headers { { "MyDummyHeader", "MyValue"}, { "Accept", "application/json" } }).Resource("Users").Delete(),
                ()=>callDynamically ? client.Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} }).Headers(new Dictionary<string, string> { { "Accept", "application/json" } }).Users.Post() : ((IRestClient)client).Headers(new Headers { { "MyDummyHeader", "MyValue"}, { "Accept", "application/json" } }).Resource("Users").Post(),
                ()=>callDynamically ? client.Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} }).Headers(new Dictionary<string, string> { { "Accept", "application/json" } }).Users.Put() : ((IRestClient)client).Headers(new Headers { { "MyDummyHeader", "MyValue"}, { "Accept", "application/json" } }).Resource("Users").Put(),
                ()=>callDynamically ? client.Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} }).Headers(new Dictionary<string, string> { { "Accept", "application/json" } }).Users.Patch() : ((IRestClient)client).Headers(new Headers { { "MyDummyHeader", "MyValue"}, { "Accept", "application/json" } }).Resource("Users").Patch(),
                ()=>callDynamically ? client.Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} }).Headers(new Dictionary<string, string> { { "Accept", "application/json" } }).Users.Merge() : ((IRestClient)client).Headers(new Headers { { "MyDummyHeader", "MyValue"}, { "Accept", "application/json" } }).Resource("Users").Merge(),
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
                Assert.That(resultingRequest.Headers.GetValues("MyDummyHeader").First(), Is.EqualTo("MyValue"));
            }
        }

        [Test]
        public async Task AllVerbs_SetHeadersUsingHeadersMethodDictionary_CorrectlyGeneratesUrl()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Users
                    .Get(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Users
                    .Head(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Users
                    .Delete(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Users
                    .Post(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Users
                    .Put(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Users
                    .Patch(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Users
                    .Merge()
            };

            foreach (var verb in verbs)
            {
                await verb();
             
                Assert.That(resultingRequest.RequestUri.ToString(), Is.EqualTo($"{BaseUri}/Users"));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_SetHeadersUsingHeadersMethodObject_CorrectlySetsHeaders(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;

            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? client.Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Users.Get() : ((IRestClient)client).Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Resource("Users").Get(),
                //()=>callDynamically ? client.Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} }).Headers(new Dictionary<string, string> { { "Accept", "application/json" } }).Head() : ((IRestClient)client).Headers(new Headers { { "MyDummyHeader", "MyValue"}, { "Accept", "application/json" } }).Head(),
                ()=>callDynamically ? client.Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Users.Delete() : ((IRestClient)client).Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Resource("Users").Delete(),
                ()=>callDynamically ? client.Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Users.Post() : ((IRestClient)client).Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Resource("Users").Post(),
                ()=>callDynamically ? client.Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Users.Put() : ((IRestClient)client).Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Resource("Users").Put(),
                ()=>callDynamically ? client.Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Users.Patch() : ((IRestClient)client).Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Resource("Users").Patch(),
                ()=>callDynamically ? client.Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Users.Merge() : ((IRestClient)client).Headers(new { DummyHeader = "MyValue", Accpet = "application/json"}).Resource("Users").Merge()
            };
            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
                Assert.That(resultingRequest.Headers.GetValues("Dummy-Header").First(), Is.EqualTo("MyValue"));
            }
        }

        [Test]
        public async Task AllVerbs_SetHeadersUsingHeadersMethodObject_CorrectlyGeneratesUrl()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri + "/", new Config(new UnitTestHandler(request => resultingRequest = request)));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Users
                    .Get(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Users
                    .Head(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Users
                    .Delete(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Users
                    .Post(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Users
                    .Put(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Users
                    .Patch(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Users
                    .Merge()
            };

            foreach (var verb in verbs)
            {
                await verb();

                Assert.That(resultingRequest.RequestUri.ToString(), Is.EqualTo($"{BaseUri}/Users"));
            }
        }

        [Test]
        public async Task AllVerbs_SetHeadersViaVerbMethod_CorrectlySetsHeaders()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Get(null, new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } }),
                ()=>client.Head(null, new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } }),
                ()=>client.Delete(null, new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } }),
                ()=>client.Post(null, new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } }),
                ()=>client.Put(null, new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } }),
                ()=>client.Patch(null, new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } }),
                ()=>client.Merge(null, new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } })
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
                Assert.That(resultingRequest.Headers.GetValues("MyDummyHeader").First(), Is.EqualTo("MyValue"));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_SetHttpMessageHandlersViaCtor_CorrectlyInvokesHandlers(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            dynamic restClient = new RestClient(BaseUri, new Config
            (
                new DelegatingHandlerWrapper(async (request, token, next) =>
                {
                    request.Headers.Add("TestHandlerHeader1", "TestHandler1");
                    return await next(request, token);
                }),
                new DelegatingHandlerWrapper(async (request, token, next) =>
                {
                    request.Headers.Add("TestHandlerHeader2", "TestHandler2");
                    return await next(request, token);
                }),
                new UnitTestHandler(request => resultingRequest = request)
            ));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? restClient.Users.Get() : ((IRestClient)restClient).Resource("Users").Get(),
                //()=>callDynamically ? restClient.Users.Head() : ((IRestClient)restClient).Resource("Users").Head(),
                ()=>callDynamically ? restClient.Users.Delete() : ((IRestClient)restClient).Resource("Users").Delete(),
                ()=>callDynamically ? restClient.Users.Post() : ((IRestClient)restClient).Resource("Users").Post(),
                ()=>callDynamically ? restClient.Users.Put() : ((IRestClient)restClient).Resource("Users").Put(),
                ()=>callDynamically ? restClient.Users.Patch() : ((IRestClient)restClient).Resource("Users").Patch(),
                ()=>callDynamically ? restClient.Users.Merge() : ((IRestClient)restClient).Resource("Users").Merge()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.GetValues("TestHandlerHeader1").First(), Is.EqualTo("TestHandler1"));
                Assert.That(resultingRequest.Headers.GetValues("TestHandlerHeader2").First(), Is.EqualTo("TestHandler2"));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task AllVerbs_SetHttpMessageHandlerFuncsViaCtor_CorrectlyInvokesHandlerFuncs(bool callDynamically)
        {
            HttpRequestMessage resultingRequest = null;
            var config = new Config
            (
                async (request, token, next) =>
                {
                    request.Headers.Add("TestHandlerHeader1", "TestHandler1");
                    return await next(request, token);
                },
                async (request, token, next) =>
                {
                    request.Headers.Add("TestHandlerHeader2", "TestHandler2");
                    return await next(request, token);
                }
            );

            var pipeline = config.Pipeline.ToList();
            pipeline.Add(new UnitTestHandler(request => resultingRequest = request));
            config.Pipeline = pipeline;

            dynamic restClient = new RestClient(BaseUri, config);

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>callDynamically ? restClient.Users.Get() : ((IRestClient)restClient).Resource("Users").Get(),
                //()=>callDynamically ? restClient.Users.Head() : ((IRestClient)restClient).Resource("Users").Head(),
                ()=>callDynamically ? restClient.Users.Delete() : ((IRestClient)restClient).Resource("Users").Delete(),
                ()=>callDynamically ? restClient.Users.Post() : ((IRestClient)restClient).Resource("Users").Post(),
                ()=>callDynamically ? restClient.Users.Put() : ((IRestClient)restClient).Resource("Users").Put(),
                ()=>callDynamically ? restClient.Users.Patch() : ((IRestClient)restClient).Resource("Users").Patch(),
                ()=>callDynamically ? restClient.Users.Merge() : ((IRestClient)restClient).Resource("Users").Merge()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.GetValues("TestHandlerHeader1").First(), Is.EqualTo("TestHandler1"));
                Assert.That(resultingRequest.Headers.GetValues("TestHandlerHeader2").First(), Is.EqualTo("TestHandler2"));
            }
        }

        [TestCase(true), TestCase(false)]
        public async Task Deserialize_UsingModelWithJsonProperty_CorrectlyDeserializes(bool callDynamically)
        {
            var user = new { phone_number = "+44 12345" };
            var config = new Config()
                .UseUnitTestHandler(request => new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(user))
                });

            dynamic restClient = new RestClient(BaseUri, config);

            User response = callDynamically ? await restClient.Users(1).Get() : await ((IRestClient)restClient).Resource("Users/1").Get();
            //PhoneNumber has JsonProperty attribute
            Assert.That(response.PhoneNumber, Is.EqualTo(user.phone_number));

        }

        [TestCase(true), TestCase(false)]
        public async Task Deserialize_WhenSettingJsonSerializerSettings_CorrectlyDeserializes(bool callDynamically)
        {
            var user = new { phone_number = "+44 12345", user_name = "dalsoft" };
            var config = new Config()
                .SetJsonSerializerSettings(new JsonSerializerSettings { ContractResolver =  new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() } })
                .UseUnitTestHandler(request => new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(user))
                });

            dynamic restClient = new RestClient(BaseUri, config);

            UserCamelCase response = callDynamically ? await restClient.Users(1).Get() : await ((IRestClient)restClient).Resource("Users/1").Get(); ;
            //Should map SnakeCase to CamelCase using JsonSerializerSettings
            Assert.That(response.PhoneNumber, Is.EqualTo(user.phone_number));
            Assert.That(response.UserName, Is.EqualTo(user.user_name));
        }

        [Test]
        public void DynamicRestClient_CastingToHttpClientWithANonHttpClientWrapper_ThrowsNotSupportedException()
        {
            var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();

            dynamic restClient = new RestClient(mockHttpClientWrapper.Object, BaseUri);

            Assert.Throws<NotSupportedException>(() =>
            {
                HttpClient httpClient = restClient;
            });
        }

        [Test]
        public void DynamicRestClient_CastingToHttpClientNotAtRoot_InvalidCastException()
        {
            dynamic restClient = new RestClient(BaseUri);

            Assert.Throws<InvalidCastException>(() =>
            {
                HttpClient httpClient = restClient.Users; // Not root url
            });
        }

        [Test]
        public void DynamicRestClient_AccessingHttpClientMemberWithANonHttpClientWrapper_ThrowsNotSupportedException()
        {
            var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();

            dynamic restClient = new RestClient(mockHttpClientWrapper.Object, BaseUri);

            Assert.Throws<NotSupportedException>(() =>
            {
                HttpClient httpClient = restClient.HttpClient;
            });
        }

        [Test]
        public void DynamicRestClient_AccessingHttpClientNotAtRoot_ThrowsInvalidCastException()
        {
            dynamic restClient = new RestClient(BaseUri);

            Assert.Throws<InvalidCastException>(() =>
            {
                HttpClient httpClient = restClient.Users.HttpClient; // Not at root Url
            });
        }

        [Test]
        public void StronglyTypedRestClient_AccessingHttpClientMemberWithANonHttpClientWrapper_ThrowsNotSupportedException()
        {
            var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();

            var restClient = new RestClient(mockHttpClientWrapper.Object, BaseUri);

            Assert.Throws<NotSupportedException>(() =>
            {
                var httpClient = restClient.HttpClient;
            });
        }

        [Test]
        public void DynamicRestClient_CastingIRestClientClientNotAtRoot_InvalidCastException()
        {
            dynamic client = new RestClient(BaseUri);

            Assert.Throws<InvalidCastException>(() =>
            {
                IRestClient restClient = client.Users; // Not root url
            });
        }

        [Test]
        public async Task DynamicRestClient_CallingAuthorizationWithBearer_AddBearerToAuthorizationHeader()
        {
            HttpRequestMessage actualHttpRequestMessage = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request =>
            {
                actualHttpRequestMessage = request;
                return GetMockUserResponse();
            })));

            await client.Authorization(AuthenticationSchemes.Bearer, "MyBearerToken").Get();

            Assert.AreEqual("Bearer", actualHttpRequestMessage.Headers.Authorization.Scheme);
            Assert.AreEqual("MyBearerToken", actualHttpRequestMessage.Headers.Authorization.Parameter);
        }


        [Test]
        public async Task StronglyTypedRestClient_CallingAuthorizationWithBearer_AddBearerToAuthorizationHeader()
        {

            HttpRequestMessage actualHttpRequestMessage = null;
            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request =>
            {
                actualHttpRequestMessage = request;
                return GetMockUserResponse();

            })));

            await client.Authorization(AuthenticationSchemes.Bearer, "MyBearerToken").Get();

            Assert.AreEqual("Bearer", actualHttpRequestMessage.Headers.Authorization.Scheme);
            Assert.AreEqual("MyBearerToken", actualHttpRequestMessage.Headers.Authorization.Parameter);
        }

        [Test]
        public void StronglyTypedRestClient_CallingAuthorizationWithBearerSchemeAndUserNamePassword_ThrowsArgumentException()
        {
            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));

            Assert.ThrowsAsync<ArgumentException>(async () => await client.Authorization(AuthenticationSchemes.Bearer, "Username", "Password").Get());
        }

        [Test]
        public void StronglyTypedRestClient_CallingAuthorizationWithBearerNullBearerToken_ThrowsArgumentException()
        {
            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));

            Assert.ThrowsAsync<ArgumentNullException>(async () => await client.Authorization(AuthenticationSchemes.Bearer, null).Get());
        }

        [Test]
        public async Task DynamicRestClient_CallingAuthorizationWithBasic_AddsBasicToAuthorizationHeader()
        {
            HttpRequestMessage actualHttpRequestMessage = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request =>
            {
                actualHttpRequestMessage = request;
                return GetMockUserResponse();
            })));

            await client.Authorization(AuthenticationSchemes.Basic, "MyUserName", "MyPassword").Get();

            Assert.AreEqual("Basic", actualHttpRequestMessage.Headers.Authorization.Scheme);
            Assert.AreEqual("MyUserName:MyPassword", Encoding.UTF8.GetString(Convert.FromBase64String(actualHttpRequestMessage.Headers.Authorization.Parameter)));
        }

        [Test]
        public async Task StronglyTypedRestClient_CallingAuthorizationWithBasic_AddsBasicToAuthorizationHeader()
        {
            HttpRequestMessage actualHttpRequestMessage = null;
            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request =>
            {
                actualHttpRequestMessage = request;
                return GetMockUserResponse();
            })));

            await client.Authorization(AuthenticationSchemes.Basic, "MyUserName", "MyPassword").Get();

            Assert.AreEqual("Basic", actualHttpRequestMessage.Headers.Authorization.Scheme);
            Assert.AreEqual("MyUserName:MyPassword", Encoding.UTF8.GetString(Convert.FromBase64String(actualHttpRequestMessage.Headers.Authorization.Parameter))); ;
        }

        [Test]
        public void StronglyTypedRestClient_CallingAuthorizationWithBasicSchemeAndBearerToken_ThrowsArgumentException()
        {
            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));

            Assert.ThrowsAsync<ArgumentException>(async () => await client.Authorization(AuthenticationSchemes.Basic, "MyToken").Get());
        }

        [Test]
        public void StronglyTypedRestClient_CallingAuthorizationWithBasicSchemeAndNullUsername_ThrowsArgumentException()
        {
            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));

            Assert.ThrowsAsync<ArgumentNullException>(async () => await client.Authorization(AuthenticationSchemes.Basic, null, "MyPassword").Get());
        }

        [Test]
        public void StronglyTypedRestClient_CallingAuthorizationWithBasicSchemeAndNullPassword_ThrowsArgumentException()
        {
            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));

            Assert.ThrowsAsync<ArgumentNullException>(async () => await client.Authorization(AuthenticationSchemes.Basic, "MyUsername", null).Get());
        }

        [Test]
        public async Task StronglyTypedRestClient_UsingStronglyTypedResource_InvokesCorrectUri()
        {
            HttpRequestMessage actualRequest = null;

            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request =>
            {
                actualRequest = request;
                return GetMockUserResponse();
            })));

            await client.Resource<RestApiResources>(resource => resource.Apps).Get();

            Assert.AreEqual($"{BaseUri}/apps", actualRequest.RequestUri.ToString().ToLower());
        }

        [Test]
        public async Task StronglyTypedRestClient_UsingStronglyTypedNestedResource_InvokesCorrectUri()
        {
            HttpRequestMessage actualRequest = null;

            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request =>
            {
                actualRequest = request;
                return GetMockUserResponse();
            })));

            await client.Resource<RestApiResources>
            (
                resource => resource.Users.Departments
            )
            .Get();

            Assert.AreEqual($"{BaseUri}/users/departments", actualRequest.RequestUri.ToString().ToLower());
        }

        [Test]
        public async Task StronglyTypedRestClient_UsingStronglyTypedResourceMethod_InvokesCorrectUri()
        {
            HttpRequestMessage actualRequest = null;

            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request =>
            {
                actualRequest = request;
                return GetMockUserResponse();
            })));

            await client.Resource<RestApiResources>
            (
                resource => resource.Users.GetUser(1)
            )
            .Get();

            Assert.AreEqual($"{BaseUri}/users/1", actualRequest.RequestUri.ToString().ToLower());
        }

        [Test]
        public async Task StronglyTypedRestClient_UsingStronglyTypedResourceAndResponse_ReturnsCorrectlyCastResponse()
        {
            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));

            var result = await client.Resource<RestApiResources>
            (
                resource => resource.Users.GetUser(1)
            )
            .Get<User>();

            Assert.AreEqual(1, result.id);
        }

        [Test]
        public async Task StronglyTypedRestClient_UsingStronglyTypedResourceAndRequestAndResponse_PostsCorrectBodyAndReturnsCorrectlyCastResponse()
        {
            HttpRequestMessage actualRequest = null;

            var client = new RestClient(BaseUri, new Config(new UnitTestHandler(request =>
            {
                actualRequest = request;
                return GetMockUserResponse();
            })));

            var result = await client.Resource<RestApiResources>
            (
                resource => resource.Users.GetUser(1)
            )
            .Post<User, User>(new User { id = 1 });

            var body = await actualRequest.Content.ReadAsStringAsync();

            Assert.True(body.Contains("\"id\":1"));
            Assert.AreEqual(1, result.id);
        }

        private static HttpResponseMessage GetMockUserResponse()
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new { id = 1 }))
            };
        }

        private static HttpResponseMessage GetMockUsersResponse()
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new List<User>
                {
                    new User {id = 1},
                    new User {id = 2},
                    new User {id = 3},
                    new User {id = 4}
                }))
            };
        }
    }
}
