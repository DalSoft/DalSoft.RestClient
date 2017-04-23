using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using DalSoft.RestClient.Test.Unit.TestModels;
using Moq;
using Newtonsoft.Json;

namespace DalSoft.RestClient.Test.Unit
{
    [TestFixture]
    public class RestClientTests
    {
        public const string BaseUri = "http://test.test";
        
        [Test]
        public async Task Query_ShouldSerializeObjectToQueryString()
        {
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<object>()))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage()}));

            dynamic client = new RestClient(mockHttpClient.Object, BaseUri);
            await client.Query(new { Id = "test", another = 1 }).Get();

            mockHttpClient.Verify(_ => _.Send
            (
                HttpMethod.Get, 
                It.Is<Uri>(__ => __ == new Uri(BaseUri + "?Id=test&another=1")),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<object>()
            ));
        }

        [Test]
        public async Task Query_ShouldSerializeArrayToQueryString()
        {
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<object>()))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage() }));

            dynamic client = new RestClient(mockHttpClient.Object, BaseUri);
            await client.Query(new { variables = new[] { "one", "other" }, otherVar = "stillWorks" }).Get();

            mockHttpClient.Verify(_ => _.Send
            (
                HttpMethod.Get,
                It.Is<Uri>(__ => __ == new Uri(BaseUri + "?variables=one&variables=other&otherVar=stillWorks")),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<object>()
            ));
        }


        [Test]
        public async Task Query_StringThatRequiresEncoding_EncodesStringCorrectly()
        {
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<object>()))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage() }));

            dynamic client = new RestClient(mockHttpClient.Object, BaseUri);
            await client.Query(new { variables = new[] { "!@£$%", "*[&]^" }, otherVar = "ƻƻƳƳ" }).Get();

            mockHttpClient.Verify(_ => _.Send
            (
                HttpMethod.Get,
                It.Is<Uri>(__ => __ == new Uri(BaseUri + "?variables=!%40£%24%25&variables=*[%26]^&otherVar=ƻƻƳƳ")),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<object>()
            ));
        }

        [Test]
        public async Task ToString_NullContent_ReturnsEmptyString()
        {
            var mockHttpClient = new Mock<IHttpClientWrapper>();

            mockHttpClient
                .Setup(_ => _.Send(HttpMethod.Get, It.IsAny<Uri>(), It.IsAny<IDictionary<string, string>>(), null))
                .Returns(Task.FromResult(new HttpResponseMessage { RequestMessage = new HttpRequestMessage() }));

            dynamic client = new RestClient(mockHttpClient.Object, BaseUri);
            var result = await client.Get();

           Assert.That(result.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task AllVerbs_SingleObjectAsDynamic_ReturnsDynamicCorrectly()
        {
           dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));

           var verbs = new Func<Task<dynamic>>[]
           {
                ()=>client.Users.Get(1),
                ()=>client.Users(1).Get(),
                ()=>client.Users.Delete(1),
                ()=>client.Users(1).Delete(),

                ()=>client.Users(1).Post(),
                ()=>client.Users(1).Put(),
                ()=>client.Users(1).Patch()
           };

            foreach (var verb in verbs)
            {
                var user = await verb();
                Assert.That(user.id, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task AllVerbs_AccessMissingMember_ReturnsNull()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));

            var verbs = new Func<Task<dynamic>>[]
            {
                    ()=>client.Users.Get(1),
                    ()=>client.Users(1).Get(),
                    ()=>client.Users.Delete(1),
                    ()=>client.Users(1).Delete(),

                    ()=>client.Users(1).Post(),
                    ()=>client.Users(1).Put(),
                    ()=>client.Users(1).Patch()
             };

            foreach (var verb in verbs)
            {
                var user = await verb();
                Assert.That(user.IAmAMissingMember, Is.Null);
            }
        }

        [Test]
        public async Task AllVerbs_SingleObjectImplicitCast_ReturnsTypeCorrectly()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                    ()=>client.Users.Get(1),
                    ()=>client.Users(1).Get(),
                    ()=>client.Users.Delete(1),
                    ()=>client.Users(1).Delete(),

                    ()=>client.Users(1).Post(),
                    ()=>client.Users(1).Put(),
                    ()=>client.Users(1).Patch()
            };

            foreach (var verb in verbs)
            {
                User user = await verb();
                Assert.That(user.id, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task AllVerbs_ArrayOfObjectsImplicitCastToList_ReturnsTypeCorrectly()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUsersResponse())));

            var verbs = new Func<Task<dynamic>>[]
            {
                    ()=>client.Users.Get(1),
                    ()=>client.Users(1).Get(),
                    ()=>client.Users.Delete(1),
                    ()=>client.Users(1).Delete(),

                    ()=>client.Users(1).Post(),
                    ()=>client.Users(1).Put(),
                    ()=>client.Users(1).Patch()
            };

            foreach (var verb in verbs)
            {
                List<User> users = await verb();
                Assert.That(users.ElementAt(1).id, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task AllVerbs_ArrayOfObjectAccessByIndex_ReturnsValueByIndexCorrectly()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUsersResponse())));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                    ()=>client.Users.Get(1),
                    ()=>client.Users(1).Get(),
                    ()=>client.Users.Delete(1),
                    ()=>client.Users(1).Delete(),

                    ()=>client.Users(1).Post(),
                    ()=>client.Users(1).Put(),
                    ()=>client.Users(1).Patch()
            };

            foreach (var verb in verbs)
            {
                List<User> users = await verb();
                Assert.That(users[0].id, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task AllVerbs_ArrayOfObjectsAsDynamicAccessByIndex_ReturnsValueByIndexCorrectly()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUsersResponse())));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                    ()=>client.Users.Get(1),
                    ()=>client.Users(1).Get(),
                    ()=>client.Users.Delete(1),
                    ()=>client.Users(1).Delete(),

                    ()=>client.Users(1).Post(),
                    ()=>client.Users(1).Put(),
                    ()=>client.Users(1).Patch()
            };

            foreach (var verb in verbs)
            {
                var users = await verb();
                Assert.That(users[0].id, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task AllVerbs_ArrayOfObjectsEnumeratingUsingForEach_CorrectlyEnumeratesOverEachItem()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUsersResponse())));

            var verbs = new Func<Task<dynamic>>[]
            {
                    ()=>client.Users.Get(1),
                    ()=>client.Users(1).Get(),
                    ()=>client.Users.Delete(1),
                    ()=>client.Users(1).Delete(),

                    ()=>client.Users(1).Post(),
                    ()=>client.Users(1).Put(),
                    ()=>client.Users(1).Patch()
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

        [Test]
        public async Task AllVerbs_SingleObjectCastAsHttpResponseMessage_CastAsHttpResponseMessageCorrectly()
        {
            var response = GetMockUserResponse();
            response.StatusCode = HttpStatusCode.BadGateway;

            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => response)));

            var verbs = new Func<Task<dynamic>>[]
            {
                    ()=>client.Users.Get(1),
                    ()=>client.Users(1).Get(),
                    ()=>client.Users.Head(1),
                    ()=>client.Users(1).Head(),
                    ()=>client.Users.Delete(1),
                    ()=>client.Users(1).Delete(),

                    ()=>client.Users(1).Post(),
                    ()=>client.Users(1).Put(),
                    ()=>client.Users(1).Patch()
            };

            foreach (var verb in verbs)
            {
                HttpResponseMessage httpResponseMessage = await verb();
                Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
            }
        }

        [Test]
        public async Task Get_SingleObjectGetHttpResponseMessageDynamically_GetsHttpResponseMessageCorrectly()
        {
            var response = GetMockUserResponse();
            response.StatusCode = HttpStatusCode.BadGateway;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => response)));
           
            var verbs = new Func<Task<dynamic>>[]
            {
                    ()=>client.Users.Get(1),
                    ()=>client.Users(1).Get(),
                    ()=>client.Users.Head(1),
                    ()=>client.Users(1).Head(),
                    ()=>client.Users.Delete(1),
                    ()=>client.Users(1).Delete(),

                    ()=>client.Users(1).Post(),
                    ()=>client.Users(1).Put(),
                    ()=>client.Users(1).Patch()
            };

            foreach (var verb in verbs)
            {
                var result = await verb();
                Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
            }
        }

        [Test]
        public void AllVerbs_SingleObjectSynchronously_GetsObjectCorrectly()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => GetMockUserResponse())));

            var verbs = new Func<Task<dynamic>>[]
            {
                    ()=>client.Users.Get(1),
                    ()=>client.Users(1).Get(),
                    ()=>client.Users.Delete(1),
                    ()=>client.Users(1).Delete(),

                    ()=>client.Users(1).Post(),
                    ()=>client.Users(1).Put(),
                    ()=>client.Users(1).Patch()
            };

            foreach (var verb in verbs)
            {
                var result = verb().Result;
                Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result.id, Is.EqualTo(1));
            }
        }
        
        [Test]
        public async Task Get_NonJsonContentFromGoogle_GetsContentCorrectly()
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

            var result = await google.news.Get();
            var content = result.ToString();
            
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Does.Contain("Top Stories"));
        }

        [Test]
        public void AllVerbs_ChainingMethodsPassingNonPrimitive_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users(new { id = 1 }).Get(),
                ()=>client.Users(new { id = 1 }).Head(),
                ()=>client.Users(new { id = 1 }).Delete(),
                ()=>client.Users(new { id = 1 }).Post(),
                ()=>client.Users(new { id = 1 }).Put(),
                ()=>client.Users(new { id = 1 }).Patch()
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void ImmutableVerbs_ChainingMethodsPassingNonPrimitive_ThrowsArgumentException()
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
        public void MutableVerbs_ChainingMethodsPassingPrimitive_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Post(1),
                ()=>client.Users.Put(1),
                ()=>client.Users.Patch(1)
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void MutableVerbs_ChainingMethodsPassingStringThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Post("This is a string"),
                ()=>client.Users.Put("This is a string"),
                ()=>client.Users.Patch("This is a string")
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void AllVerbs_SecondArgNotHeaderDictionary_ThrowsArgumentException()
        {
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Get(1, new {}),
                ()=>client.Users.Head(1, new {}),
                ()=>client.Users.Delete(1, new {}),
                ()=>client.Users.Post(new {}, new {}),
                ()=>client.Users.Put(new {}, new {}),
                ()=>client.Users.Patch(new {}, new {})
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void AllVerbs_MoreThan2Args_ThrowsArgumentException()
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
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void AllVerbs_UsingInvalidUri_ThrowsArgumentException()
        {
            dynamic client = new RestClient("this is a invalidUri", new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Get(),
                ()=>client.Users.Head(),
                ()=>client.Users.Delete(),
                ()=>client.Users.Post(),
                ()=>client.Users.Put(),
                ()=>client.Users.Patch(),
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public void AllVerbs_UsingNullUri_ThrowsArgumentException()
        {
            dynamic client = new RestClient(null, new Config(new UnitTestHandler()));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Get(),
                ()=>client.Users.Head(),
                ()=>client.Users.Delete(),
                ()=>client.Users.Post(),
                ()=>client.Users.Put(),
                ()=>client.Users.Patch(),
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public async Task AllVerbs_ChainingMethods_GeneratesCorrectUri()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users(1).Get(),
                ()=>client.Users(1).Head(),
                ()=>client.Users(1).Delete(),
                ()=>client.Users(1).Post(),
                ()=>client.Users(1).Put(),
                ()=>client.Users(1).Patch()
            };
            
            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.RequestUri.ToString().ToLower(), Is.EqualTo(BaseUri + "/users/1"));
            }
        }

        [Test]
        public async Task AllVerbs_ChainingMethodsUsingGuid_GeneratesCorrectUri()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));

            var testGuid = Guid.NewGuid();

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users(testGuid).Get(),
                ()=>client.Users(testGuid).Head(),
                ()=>client.Users(testGuid).Delete(),
                ()=>client.Users(testGuid).Post(),
                ()=>client.Users(testGuid).Put(),
                ()=>client.Users(testGuid).Patch()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.RequestUri.ToString().ToLower(), Is.EqualTo($"{BaseUri}/users/{testGuid}"));
            }
        }

        [Test]
        public async Task AllVerbs_ChainingMethodsUsingEnum_GeneratesCorrectUri()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));

            var testEnum= HttpStatusCode.Accepted;

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users(testEnum).Get(),
                ()=>client.Users(testEnum).Head(),
                ()=>client.Users(testEnum).Delete(),
                ()=>client.Users(testEnum).Post(),
                ()=>client.Users(testEnum).Put(),
                ()=>client.Users(testEnum).Patch()
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
                ()=>client.Users.Resource(new { id=1 }).Patch()
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
                ()=>client.Users.Resource(1, 1).Patch()
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
                ()=>client.Users.Resource().Patch()
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public async Task Reesource_CorrectArgs_GeneratesCorrectUri()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users.Resource("this-is-not-valid-in-csharp").Get(),
                ()=>client.Users.Resource("this-is-not-valid-in-csharp").Head(),
                ()=>client.Users.Resource("this-is-not-valid-in-csharp").Delete(),
                ()=>client.Users.Resource("this-is-not-valid-in-csharp").Post(),
                ()=>client.Users.Resource("this-is-not-valid-in-csharp").Put(),
                ()=>client.Users.Resource("this-is-not-valid-in-csharp").Patch()
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
                ()=>client.Users.Query(new User { id = 1 }).Patch()
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
                ()=>client.Users.Query(new { id = 1 }, new { id = 1 }).Patch()
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
                ()=>client.Users.Query().Patch()
            };

            foreach (var verb in verbs)
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await verb());
            }
        }

        [Test]
        public async Task Query_CorrectArgs_GeneratesCorrectUri()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Users(1).Query(new { my="query string" }).Get(),
                ()=>client.Users(1).Query(new { my="query string" }).Head(),
                ()=>client.Users(1).Query(new { my="query string" }).Delete(),
                ()=>client.Users(1).Query(new { my="query string" }).Post(),
                ()=>client.Users(1).Query(new { my="query string" }).Put(),
                ()=>client.Users(1).Query(new { my="query string" }).Patch()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.RequestUri.ToString().ToLower(), Is.EqualTo(BaseUri + "/users/1?my=query+string"));
            }
        }
        
        [Test]
        public async Task AllVerbs_SetDefaultHeadersViaCtor_CorrectlySetsHeaders()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri,
                                             new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } },
                                             new Config(new UnitTestHandler(request => resultingRequest = request))
            );

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client.Get(),
                ()=>client.Head(),
                ()=>client.Delete(),
                ()=>client.Post(),
                ()=>client.Put(),
                ()=>client.Patch()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
                Assert.That(resultingRequest.Headers.GetValues("MyDummyHeader").First(), Is.EqualTo("MyValue"));
            }
        }
        
        [Test]
        public async Task AllVerbs_SetHeadersUsingHeadersMethodDictionary_CorrectlySetsHeaders()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));
            
            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Headers(new Dictionary<string, string> { { "Accept", "application/json" } })
                    .Get(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Headers(new Dictionary<string, string> { { "Accept", "application/json" } })
                    .Head(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Headers(new Dictionary<string, string> { { "Accept", "application/json" } })
                    .Delete(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Headers(new Dictionary<string, string> { { "Accept", "application/json" } })
                    .Post(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Headers(new Dictionary<string, string> { { "Accept", "application/json" } })
                    .Put(),
                ()=>client
                    .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue"} })
                    .Headers(new Dictionary<string, string> { { "Accept", "application/json" } })
                    .Patch()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
                Assert.That(resultingRequest.Headers.GetValues("MyDummyHeader").First(), Is.EqualTo("MyValue"));
            }
        }

        [Test]
        public async Task AllVerbs_SetHeadersUsingHeadersMethodObject_CorrectlySetsHeaders()
        {
            HttpRequestMessage resultingRequest = null;
            dynamic client = new RestClient(BaseUri, new Config(new UnitTestHandler(request => resultingRequest = request)));

            var verbs = new Func<Task<dynamic>>[]
            {
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Headers(new { Accept = "application/json" })
                    .Get(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Headers(new { Accept = "application/json" })
                    .Head(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Headers(new { Accept = "application/json" })
                    .Delete(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Headers(new { Accept = "application/json" })
                    .Post(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Headers(new { Accept = "application/json" })
                    .Put(),
                ()=>client
                    .Headers(new { DummyHeader = "MyValue" } )
                    .Headers(new { Accept = "application/json" })
                    .Patch()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
                Assert.That(resultingRequest.Headers.GetValues("Dummy-Header").First(), Is.EqualTo("MyValue"));
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
                ()=>client.Patch(null, new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } })
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.Accept.First().MediaType, Is.EqualTo("application/json"));
                Assert.That(resultingRequest.Headers.GetValues("MyDummyHeader").First(), Is.EqualTo("MyValue"));
            }
        }

        [Test]
        public async Task AllVerbs_SetHttpMessageHandlersViaCtor_CorrectlyInvokesHandlers()
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
                ()=>restClient.Get(),
                ()=>restClient.Head(),
                ()=>restClient.Delete(),
                ()=>restClient.Post(),
                ()=>restClient.Put(),
                ()=>restClient.Patch()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.GetValues("TestHandlerHeader1").First(), Is.EqualTo("TestHandler1"));
                Assert.That(resultingRequest.Headers.GetValues("TestHandlerHeader2").First(), Is.EqualTo("TestHandler2"));
            }
        }

        [Test]
        public async Task AllVerbs_SetHttpMessageHandlerFuncsViaCtor_CorrectlyInvokesHandlerFuncs()
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
                ()=>restClient.Get(),
                ()=>restClient.Head(),
                ()=>restClient.Delete(),
                ()=>restClient.Post(),
                ()=>restClient.Put(),
                ()=>restClient.Patch()
            };

            foreach (var verb in verbs)
            {
                await verb();
                Assert.That(resultingRequest.Headers.GetValues("TestHandlerHeader1").First(), Is.EqualTo("TestHandler1"));
                Assert.That(resultingRequest.Headers.GetValues("TestHandlerHeader2").First(), Is.EqualTo("TestHandler2"));
            }
        }

        [Test]
        public async Task Deserialize_UsingModelWithJsonProperty_CorrectlyDeserializes()
        {
            var user = new { phone_number = "+44 12345" };
            var config = new Config()
                .UseUnitTestHandler(request => new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(user))
                });

            dynamic restClient = new RestClient(BaseUri, config);

            User response = await restClient.Get();
            //PhoneNumber has JsonProperty attribute
            Assert.That(response.PhoneNumber, Is.EqualTo(user.phone_number));

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
