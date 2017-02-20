using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using DalSoft.RestClient.Test.Integration.Models;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Integration
{
    [TestFixture]
    public class RestClientTests
    {
        private const string BaseUri = "http://jsonplaceholder.typicode.com";
        
        [Test]
        public async Task Get_SingleUserAsDynamic_ReturnsDynamicCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            var user = await client.Users.Get(1);

            Assert.That(user.id, Is.EqualTo(1));
        }

        [Test]
        public async Task Get_AccessMissingMember_ReturnsNull()
        {
            dynamic client = new RestClient(BaseUri);

            var user = await client.Users.Get(1);

            Assert.That(user.IAmAMissingMember, Is.Null);
        }

        [Test]
        public async Task Get_SingleUserImplicitCast_ReturnsTypeCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            User user = await client.Users.Get(1);

            Assert.That(user.id, Is.EqualTo(1));
        }

        [Test]
        public async Task Get_ArrayOfUsersImplicitCastToList_ReturnsTypeCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            List<User> users = await client.Users.Get();

            Assert.That(users.ElementAt(1).id, Is.EqualTo(2));
        }

        [Test]
        public async Task Get_ArrayOfUsersAccessByIndex_ReturnsValueByIndexCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            List<User> users = await client.Users.Get();

            Assert.That(users[0].id, Is.EqualTo(1));
        }

        [Test]
        public async Task Get_ArrayOfUsersAsDynamicAccessByIndex_ReturnsValueByIndexCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            var users = await client.Users.Get();

            Assert.That(users[0].id, Is.EqualTo(1));
        }

        [Test]
        public async Task Get_ArrayOfUsersEnumeratingUsingForEach_CorrectlyEnumeratesOverEachItem()
        {
            dynamic client = new RestClient(BaseUri);
            var i = 1;

            var users = await client.Users.Get();

            foreach (var user in users)
            {
                Assert.That(user.id, Is.EqualTo(i));
                i++;
            }
        }

        [Test]
        public async Task Get_SingleUserCastAsHttpResponseMessage_CastAsHttpResponseMessageCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            HttpResponseMessage httpResponseMessage = await client.Users.Get(1);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Get_SingleUserGetHttpResponseMessageDynamically_GetsHttpResponseMessageCorrectly()
        {
            dynamic client = new RestClient(BaseUri);
            var user = await client.Users.Get(1);

            Assert.That(user.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Get_SingleUserUsingResourceMethod_GetsUserCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            HttpResponseMessage httpResponseMessage = await client.Users.Resource(1).Get();

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Get_SingleUserUsingInlineMethod_GetsUserCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            HttpResponseMessage httpResponseMessage = await client.Users(1).Get();

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void Get_SingleUserSynchronously_GetsUserCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            HttpResponseMessage httpResponseMessage = client.Users(1).Get().Result;

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Get_NestedCommentsFromUser_GetsCommentCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            List<Comment> comment = await client.users(2).Comments.Get();

            Assert.That(comment.First().postId, Is.EqualTo(1));
        }

        [Test]
        public async Task Get_NoJsonContentFromGoogle_GetsContentCorrectly()
        {
            dynamic google = new RestClient("https://www.google.com", new Dictionary<string, string> { { "Accept", "text/html" } });

            var result = await google.news.Get();
            var content = result.ToString();

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Does.Contain("Top Stories"));
        }

        [Test]
        public async Task Post_NewUserAsDynamic_CreatesAndReturnsNewResourceAsDynamic()
        {
            dynamic client = new RestClient(BaseUri);
            var user = new { name = "foo", username = "bar", email = "test@test.com"  };
            var result = await client.Users.Post(user);

            Assert.That(result.name, Is.EqualTo(user.name));
            Assert.That(result.email, Is.EqualTo(user.email));
            Assert.That(result.username, Is.EqualTo(user.username));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task Post_NewUserAsStaticType_CreatesAndReturnsNewResourceAsStaticType()
        {
            dynamic client = new RestClient(BaseUri);
            var user = new User { name = "foo", username = "bar", email = "test@test.com" };
            var result = await client.Users.Post(user);

            Assert.That(result.name, Is.EqualTo(user.name));
            Assert.That(result.email, Is.EqualTo(user.email));
            Assert.That(result.username, Is.EqualTo(user.username));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task Put_UpdateUserWithDynamic_UpdatesAndReturnsNewResourceAsDynamic()
        {
            dynamic client = new RestClient(BaseUri);
            var user = new { name = "foo1", username = "bar1", email = "test1@test.com" };
            var result = await client.Users(1).Put(user);

            Assert.That(result.name, Is.EqualTo(user.name));
            Assert.That(result.email, Is.EqualTo(user.email));
            Assert.That(result.username, Is.EqualTo(user.username));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Put_UpdateUserWithStaticType_UpdatesAndReturnsNewResourceAsStaticType()
        {
            dynamic client = new RestClient(BaseUri);
            var user = new User { name = "foo1", username = "bar1", email = "test1@test.com" };
            var result = await client.Users(1).Put(user);

            Assert.That(result.name, Is.EqualTo(user.name));
            Assert.That(result.email, Is.EqualTo(user.email));
            Assert.That(result.username, Is.EqualTo(user.username));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }


        [Test]
        public async Task Patch_UpdateUserWithDynamic_UpdatesAndReturnsNewResourceAsDynamic()
        {
            dynamic client = new RestClient(BaseUri);
            var user = new { name = "foo1", username = "bar1", email = "test1@test.com" };
            var result = await client.Users(1).Patch(user);

            Assert.That(result.name, Is.EqualTo(user.name));
            Assert.That(result.email, Is.EqualTo(user.email));
            Assert.That(result.username, Is.EqualTo(user.username));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Patch_UpdateUserWithStaticType_UpdatesAndReturnsNewResourceAsStaticType()
        {
            dynamic client = new RestClient(BaseUri);
            var user = new User { name = "foo1", username = "bar1", email = "test1@test.com" };
            var result = await client.Users(1).Patch(user);

            Assert.That(result.name, Is.EqualTo(user.name));
            Assert.That(result.email, Is.EqualTo(user.email));
            Assert.That(result.username, Is.EqualTo(user.username));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Delete_DeleteUser_HttpResponseMessageReturnsOK()
        {
            dynamic client = new RestClient(BaseUri);
            var result = await client.Users.Delete(1);

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Delete_DeleteUserUsingInlineMethod_HttpResponseMessageReturnsOK()
        {
            dynamic client = new RestClient(BaseUri);
            var result = await client.Users(1).Delete();

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Delete_DeleteUserUsingResourceMethod_HttpResponseMessageReturnsOK()
        {
            dynamic client = new RestClient(BaseUri);
            var result = await client.Users().Resource(1).Delete();

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Query_PassQuery_CorrectlyAppendsQueryString()
        {
            dynamic client = new RestClient(BaseUri);

            List<User> users = await client.Users().Query(new { id = 2 }).Get();

            Assert.That(users.Count, Is.EqualTo(1));
            Assert.That(users[0].id, Is.EqualTo(2));
        }

        [Test]
        public async Task Get_SetDefaultHeadersViaCtor_CorrectlySetsHeaders()
        {
            dynamic client = new RestClient("http://headers.jsontest.com/",
                new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } }
            );

            var result = await client.Get();
            Assert.That(result.Accept, Is.EqualTo("application/json"));
            Assert.That(result.MyDummyHeader, Is.EqualTo("MyValue"));
        }

        [Test]
        public async Task Get_SetHeadersDefaultHeadersViaProperty_CorrectlySetsHeaders()
        {
            dynamic client = new RestClient("http://headers.jsontest.com/");
            client.DefaultRequestHeaders.Add("MyDummyHeader", "MyValue");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var result = await client.Get();
            Assert.That(result.Accept, Is.EqualTo("application/json"));
            Assert.That(result.MyDummyHeader, Is.EqualTo("MyValue"));
        }

        [Test]
        public async Task Get_SetHeadersViaMethod_CorrectlySetsHeaders()
        {
            dynamic client = new RestClient("http://headers.jsontest.com/");

            var result = await client.Get(null, new Dictionary<string, string> { { "MyDummyHeader", "MyValue" }, { "Accept", "application/json" } });
            Assert.That(result.Accept, Is.EqualTo("application/json"));
            Assert.That(result.MyDummyHeader, Is.EqualTo("MyValue"));
        }

        [Test]
        public async Task Get_SetHttpMessageHandlersViaCtor_CorrectlyInvokesHandlers()
        {
            dynamic restClient = new RestClient("http://headers.jsontest.com/", new Config
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
                })
            ));

            var result = await restClient.Get();

            Assert.That(result.TestHandlerHeader1, Is.EqualTo("TestHandler1"));
            Assert.That(result.TestHandlerHeader2, Is.EqualTo("TestHandler2"));
        }

        [Test]
        public async Task Get_SetHttpMessageHandlerFuncsViaCtor_CorrectlyInvokesHandlerFuncs()
        {
            dynamic restClient = new RestClient("http://headers.jsontest.com/", new Config(
            async (request, token, next) =>
            {
                request.Headers.Add("TestHandlerHeader1", "TestHandler1");
                return await  next(request, token);
            }, 
            async (request, token, next) =>
            {
                request.Headers.Add("TestHandlerHeader2", "TestHandler2");
                return await next(request, token);
            }));

            var result = await restClient.Get();

            Assert.That(result.TestHandlerHeader1, Is.EqualTo("TestHandler1"));
            Assert.That(result.TestHandlerHeader2, Is.EqualTo("TestHandler2"));
        }

        [Test]
        public async Task Get_SetCookieContainerUsingHttpClientHandlerViaCtor_CorrectlySetsCookie()
        {
            var cookieContainer = new CookieContainer();
            var httpClientHandler = new HttpClientHandler { CookieContainer = cookieContainer };

            dynamic restClient = new RestClient("https://httpbin.org/cookies/set?testcookie=darran", new Config(httpClientHandler));

            await restClient.Get();

            Assert.That(cookieContainer.GetCookies(new Uri("https://httpbin.org"))["testcookie"]?.Value, Is.EqualTo("darran"));
        }

        [Test]
        public async Task Get_SetHttpClientHandlerAndDelegatingHandlersViaCtor_CorrectlyInvokesHttpClientHandlerAndDelegatingHandlers()
        {
            var cookieContainer = new CookieContainer();
            var httpClientHandler = new HttpClientHandler { CookieContainer = cookieContainer };

            dynamic restClient = new RestClient("https://httpbin.org/cookies/set?testcookie1=darran1",  new Config
            (
                httpClientHandler,
                new DelegatingHandlerWrapper(async (request, token, next) =>
                {
                    request.RequestUri = new Uri(request.RequestUri + "&testcookie2=darran2");
                    return await next(request, token);
                }),
                new DelegatingHandlerWrapper(async (request, token, next) =>
                {
                    request.RequestUri = new Uri(request.RequestUri + "&testcookie3=darran3"); ;
                    return await next(request, token);
                })
            ));

            await restClient.Get();

            Assert.That(cookieContainer.GetCookies(new Uri("https://httpbin.org"))["testcookie1"]?.Value, Is.EqualTo("darran1"));
            Assert.That(cookieContainer.GetCookies(new Uri("https://httpbin.org"))["testcookie2"]?.Value, Is.EqualTo("darran2"));
            Assert.That(cookieContainer.GetCookies(new Uri("https://httpbin.org"))["testcookie3"]?.Value, Is.EqualTo("darran3"));
        }
    }
}
