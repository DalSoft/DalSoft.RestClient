using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

            List<Post> post = await client.Posts(2).Comments.Get();

            Assert.That(post.First().id, Is.EqualTo(6));
        }

        [Test]
        public async Task Get_NoJsonContentFromGoogle_GetsContentCorrectly()
        {
            dynamic client = new RestClient("https://www.google.com", new Dictionary<string, string>{{ "Accept","text/html" }});

            var google = await client.News.Get();
            var result = google.ToString();

            Assert.That(google.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.StringContaining("Top Stories"));
        }



        [Test]
        public async Task Post_NewPostAsDynamic_CreatesAndReturnsNewResourceAsDynamic()
        {
            dynamic client = new RestClient(BaseUri);
            var post = new {  title="foo", body="bar", userId=10 };
            var result = await client.Posts.Post(post);

            Assert.That(result.title, Is.EqualTo(post.title));
            Assert.That(result.body, Is.EqualTo(post.body));
            Assert.That(result.userId, Is.EqualTo(post.userId));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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
        public async Task Delete_DeletePost_HttpResponseMessageReturnsNoContent()
        {
            dynamic client = new RestClient(BaseUri);
            var result = await client.Posts.Delete(1);

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task Delete_DeletePostUsingInlineMethod_HttpResponseMessageReturnsNoContent()
        {
            dynamic client = new RestClient(BaseUri);
            var result = await client.Posts(1).Delete();

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task Delete_DeletePostUsingResourceMethod_HttpResponseMessageReturnsNoContent()
        {
            dynamic client = new RestClient(BaseUri);
            var result = await client.Posts().Resource(1).Delete();

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
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
        public async Task Get_SetDefaultHeaders_CorrectlySetsHeaders()
        {
            dynamic client = new RestClient("http://headers.jsontest.com/", 
                new Dictionary<string, string> {{ "MyDummyHeader", "MyValue" }, { "Accept", "application/json" }}
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
        

    }
}
