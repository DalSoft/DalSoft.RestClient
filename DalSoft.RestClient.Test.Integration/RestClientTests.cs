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
        public async Task Get_SinglePostAsDynamic_ReturnsDynamicCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            var post = await client.Posts.Get(1);

            Assert.That(post.id, Is.EqualTo(1));
        }

        [Test]
        public async Task Get_AccessMissingMember_ReturnsNull()
        {
            dynamic client = new RestClient(BaseUri);

            var post = await client.Posts.Get(1);

            Assert.That(post.IAmAMissingMember, Is.Null);
        }

        [Test]
        public async Task Get_SinglePostImplicitCast_ReturnsTypeCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            Post post = await client.Posts.Get(1);

            Assert.That(post.id, Is.EqualTo(1));
        }

        [Test]
        public async Task Get_ArrayOfPostsImplicitCastToList_ReturnsTypeCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            List<Post> posts = await client.Posts.Get();

            Assert.That(posts.ElementAt(1).id, Is.EqualTo(2));
        }

        [Test]
        public async Task Get_ArrayOfPostsAccessByIndex_ReturnsValueByIndexCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            List<Post> posts = await client.Posts.Get();

            Assert.That(posts[0].id, Is.EqualTo(1));
        }

        [Test]
        public async Task Get_ArrayOfPostsAsDynamicAccessByIndex_ReturnsValueByIndexCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            var posts = await client.Posts.Get();

            Assert.That(posts[0].id, Is.EqualTo(1));
        }

        [Test]
        public async Task Get_ArrayOfPostsEnumeratingUsingForEach_CorrectlyEnumeratesOverEachItem()
        {
            dynamic client = new RestClient(BaseUri);
            var i = 1;

            var posts = await client.Posts.Get();

            foreach (var post in posts)
            {
                Assert.That(post.id, Is.EqualTo(i));
                i++;
            }
        }

        [Test]
        public async Task Get_SinglePostCastAsHttpResponseMessage_CastAsHttpResponseMessageCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            HttpResponseMessage httpResponseMessage = await client.Posts.Get(1);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Get_SinglePostGetHttpResponseMessageDynamically_GetsHttpResponseMessageCorrectly()
        {
            dynamic client = new RestClient(BaseUri);
            var post = await client.Posts.Get(1);

            Assert.That(post.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Get_SinglePostUsingResourceMethod_GetsPostCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            HttpResponseMessage httpResponseMessage = await client.Posts.Resource(1).Get();

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Get_SinglePostUsingInlineMethod_GetsPostCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            HttpResponseMessage httpResponseMessage = await client.Posts(1).Get();

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void Get_SinglePostSynchronously_GetsPostCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            HttpResponseMessage httpResponseMessage = client.Posts(1).Get().Result;

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Get_NestedCommentsFromPost_GetsCommentCorrectly()
        {
            dynamic client = new RestClient(BaseUri);

            List<Post> post = await client.posts(2).Comments.Get();

            Assert.That(post.First().id, Is.EqualTo(6));
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
        public async Task Post_NewPostAsDynamic_CreatesAndReturnsNewResourceAsDynamic()
        {
            dynamic client = new RestClient(BaseUri);
            var post = new { title = "foo", body = "bar", userId = 10 };
            var result = await client.Posts.Post(post);

            Assert.That(result.title, Is.EqualTo(post.title));
            Assert.That(result.body, Is.EqualTo(post.body));
            Assert.That(result.userId, Is.EqualTo(post.userId));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task Post_NewPostAsStaticType_CreatesAndReturnsNewResourceAsStaticType()
        {
            dynamic client = new RestClient(BaseUri);
            var post = new Post { title = "foo", body = "bar", userId = 10 };
            var result = await client.Posts.Post(post);

            Assert.That(result.title, Is.EqualTo(post.title));
            Assert.That(result.body, Is.EqualTo(post.body));
            Assert.That(result.userId, Is.EqualTo(post.userId));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task Put_UpdatePostWithDynamic_UpdatesAndReturnsNewResourceAsDynamic()
        {
            dynamic client = new RestClient(BaseUri);
            var post = new { title = "foo", body = "bar", userId = 10 };
            var result = await client.Posts(1).Put(post);

            Assert.That(result.title, Is.EqualTo(post.title));
            Assert.That(result.body, Is.EqualTo(post.body));
            Assert.That(result.userId, Is.EqualTo(post.userId));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Put_UpdatePostWithStaticType_UpdatesAndReturnsNewResourceAsStaticType()
        {
            dynamic client = new RestClient(BaseUri);
            var post = new Post { title = "foo", body = "bar", userId = 10 };
            var result = await client.Posts(1).Put(post);

            Assert.That(result.title, Is.EqualTo(post.title));
            Assert.That(result.body, Is.EqualTo(post.body));
            Assert.That(result.userId, Is.EqualTo(post.userId));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }


        [Test]
        public async Task Patch_UpdatePostWithDynamic_UpdatesAndReturnsNewResourceAsDynamic()
        {
            dynamic client = new RestClient(BaseUri);
            var post = new { title = "foo", body = "bar", userId = 10 };
            var result = await client.Posts(1).Patch(post);

            Assert.That(result.title, Is.EqualTo(post.title));
            Assert.That(result.body, Is.EqualTo(post.body));
            Assert.That(result.userId, Is.EqualTo(post.userId));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Patch_UpdatePostWithStaticType_UpdatesAndReturnsNewResourceAsStaticType()
        {
            dynamic client = new RestClient(BaseUri);
            var post = new Post { title = "foo", body = "bar", userId = 10 };
            var result = await client.Posts(1).Patch(post);

            Assert.That(result.title, Is.EqualTo(post.title));
            Assert.That(result.body, Is.EqualTo(post.body));
            Assert.That(result.userId, Is.EqualTo(post.userId));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Delete_DeletePost_HttpResponseMessageReturnsOK()
        {
            dynamic client = new RestClient(BaseUri);
            var result = await client.Posts.Delete(1);

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Delete_DeletePostUsingInlineMethod_HttpResponseMessageReturnsOK()
        {
            dynamic client = new RestClient(BaseUri);
            var result = await client.Posts(1).Delete();

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Delete_DeletePostUsingResourceMethod_HttpResponseMessageReturnsOK()
        {
            dynamic client = new RestClient(BaseUri);
            var result = await client.Posts().Resource(1).Delete();

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Query_PassQuery_CorrectlyAppendsQueryString()
        {
            dynamic client = new RestClient(BaseUri);

            List<Post> posts = await client.Posts().Query(new { id = 2 }).Get();

            Assert.That(posts.Count, Is.EqualTo(1));
            Assert.That(posts[0].id, Is.EqualTo(2));
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
