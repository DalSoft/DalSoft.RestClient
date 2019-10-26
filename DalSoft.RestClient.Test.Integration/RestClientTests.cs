using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using DalSoft.RestClient.Test.Integration.TestModels;
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
        public async Task Get_NonJsonContentFromGoogle_GetsContentCorrectly()
        {
            dynamic google = new RestClient("https://www.google.com", new Dictionary<string, string> { { "Accept", "text/html" } });

            var result = await google.news.Get();
            var content = result.ToString();

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Does.Contain("News"));
        }

        [Test]
        public async Task Post_NewUserAsDynamic_CreatesAndReturnsNewResourceAsDynamic()
        {
            dynamic client = new RestClient(BaseUri);
            var user = new { name = "foo", username = "bar", email = "test@test.com" };
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
            var result = await client.Users(1).Delete();

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
        public async Task Get_SetHeadersViaHeadersMethodDictionary_CorrectlySetsHeaders()
        {
            dynamic client = new RestClient("http://headers.jsontest.com/");

            var result = await client
                .Headers(new Dictionary<string, string> { { "Accept", "application/json" } })
                .Headers(new Dictionary<string, string> { { "MyDummyHeader", "MyValue" } })
                .Get();

            Assert.That(result.Accept, Is.EqualTo("application/json"));
            Assert.That(result.MyDummyHeader, Is.EqualTo("MyValue"));
        }

        [Test]
        public async Task Get_SetHeadersViaHeadersMethodObject_CorrectlySetsHeaders()
        {
            dynamic client = new RestClient("http://headers.jsontest.com/");

            var result = await client
                .Headers(new { Accept = "application/json" })
                .Headers(new { DummyHeader = "MyValue" })
                .Get();

            Assert.That(result.Accept, Is.EqualTo("application/json"));
            Assert.That(result.ToString(), Does.Contain("\"Dummy-Header\": \"MyValue\""));
        }

        [Test]
        public async Task Get_SetHeadersViaVerbMethod_CorrectlySetsHeaders()
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
                return await next(request, token);
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

            dynamic restClient = new RestClient("https://httpbin.org/cookies/set?testcookie1=darran1", new Config
            (
                httpClientHandler,
                new DelegatingHandlerWrapper(async (request, token, next) =>
                {
                    request.RequestUri = new Uri(request.RequestUri + "&testcookie2=darran2");
                    return await next(request, token);
                }),
                new DelegatingHandlerWrapper(async (request, token, next) =>
                {
                    request.RequestUri = new Uri(request.RequestUri + "&testcookie3=darran3");
                    return await next(request, token);
                })
            ));

            await restClient.Get();

            Assert.That(cookieContainer.GetCookies(new Uri("https://httpbin.org"))["testcookie1"]?.Value, Is.EqualTo("darran1"));
            Assert.That(cookieContainer.GetCookies(new Uri("https://httpbin.org"))["testcookie2"]?.Value, Is.EqualTo("darran2"));
            Assert.That(cookieContainer.GetCookies(new Uri("https://httpbin.org"))["testcookie3"]?.Value, Is.EqualTo("darran3"));
        }

        [Test]
        public async Task Post_DataUsingFormUrlEncodedContentType_CorrectlyPostsData()
        {
            var formUrlEncodedHeader = new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } };
            dynamic restClient = new RestClient("https://httpbin.org/post", formUrlEncodedHeader, new Config()
                .UseFormUrlEncodedHandler()
            );

            var formUrlEncodedData = new
            {
                custname = "George Washington",
                custtel = "449098090",
                custemail = "George.Washington@gov.org",
                size = "small",
                topping = new[] { "bacon", "cheese", "onion" },
                delivery = "11:00",
                comments = "Leave at the whitehouse"
            };

            var response = await restClient.Post(formUrlEncodedData);

            Assert.That(response.form.comments, Is.EqualTo(formUrlEncodedData.comments));
            Assert.That(response.form.custtel, Is.EqualTo(formUrlEncodedData.custtel));
            Assert.That(response.form.custemail, Is.EqualTo(formUrlEncodedData.custemail));
            Assert.That(response.form.size, Is.EqualTo(formUrlEncodedData.size));
            Assert.That(response.form.topping[0], Is.EqualTo("bacon"));
            Assert.That(response.form.topping[1], Is.EqualTo("cheese"));
            Assert.That(response.form.topping[2], Is.EqualTo("onion"));
            Assert.That(response.form.delivery, Is.EqualTo(formUrlEncodedData.delivery));
            Assert.That(response.form.comments, Is.EqualTo(formUrlEncodedData.comments));
        }

        [Test]
        public async Task Post_MultipartForm_CorrectlyPostsFile()
        {
            dynamic restClient = new RestClient("http://en.directupload.net/index.php", new Config
            (
                new MultipartFormDataHandler()
            ));

            var multipartContentType = new Dictionary<string, string> { { "Content-Type", $"multipart/form-data;boundary=\"Upload----{Guid.NewGuid()}\"" } };
            var filepath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + "/DalSoft.jpg";
            var fileBytes = File.ReadAllBytes(filepath);

            var result = await restClient.Query(new { mode = "upload" })
                .Post(new
                {
                    bilddatei = fileBytes,
                    filename = "dalsoft.jpg" //bilddatei is image file in german incase you were wondering
                },
                multipartContentType
            );

            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.ToString(), Does.Contain("Your picture was uploaded successfully!"));
        }
        
        [Test]
        public void Get_UsingRetryHandlerWhenTransientExceptionEncountered_ShouldBeRetried()
        {
            // ReSharper disable once InconsistentNaming
            const int ERROR_WINHTTP_NAME_NOT_RESOLVED = 12007;
            const int WSAHOST_NOT_FOUND = 11001;

            var numberOfActualTries = 0;

            dynamic restClient = new RestClient("http://A_Url_The_Will_Cause_A_Transient_Exception", new Config()
            .UseRetryHandler(maxRetries:3, waitToRetryInSeconds:2, maxWaitToRetryInSeconds: 10, backOffStrategy: RetryHandler.BackOffStrategy.Exponential)
            .UseHandler((request, token, next) =>
            {
                numberOfActualTries++;
                return next(request, token);
            }));

            var exception = Assert.ThrowsAsync<HttpRequestException>(async () => await restClient.Get());
            var webException = exception.InnerException as WebException;
            var win32Exception = exception.InnerException as Win32Exception;

            if (webException!=null) //.NET 4.5, .NET Standard all platforms except Windows
                Assert.That(webException.Status, Is.EqualTo(WebExceptionStatus.NameResolutionFailure));

            #if NETCOREAPP2_2
                if (win32Exception != null) //.NET Core > 2.1 windows uses sockets https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socketexception?view=netframework-4.7.2
                    Assert.That(win32Exception.NativeErrorCode, Is.EqualTo(WSAHOST_NOT_FOUND));
            #else
                if (win32Exception != null) //.NET Core < 2.1 Windows only https://github.com/dotnet/corefx/issues/19185
                        Assert.That(win32Exception.NativeErrorCode, Is.EqualTo(ERROR_WINHTTP_NAME_NOT_RESOLVED));
            #endif

            Assert.That(numberOfActualTries, Is.EqualTo(4)); //maxRetries + the first attempt
        }

        [Test, Ignore("Secrets")]
        public async Task Post_StatusUpdateUsingTwitterHandler_CorrectlyUpdateStatus()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config
            (
                new TwitterHandler
                (
                    consumerKey:"", consumerKeySecret:"", accessToken: "", accessTokenSecret:""
                )));

            var result = await restClient.Statuses.Update.Post( new { status = "Test" , trim_user = "1" } );
            
            Assert.AreEqual(HttpStatusCode.OK, result.HttpResponseMessage.StatusCode);
        }

        [Test, Ignore("Secrets")]
        public async Task Get_SearchTweetsUsingTwitterHandler_ReturnsTwitterSearchResults()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config
            (
                new TwitterHandler
                (
                    consumerKey:"", consumerKeySecret:"", accessToken:"", accessTokenSecret:""
                )));

            var result = await restClient.Search.Tweets.Query(new { q = "Hello World" }).Get();
            
            Assert.AreEqual("Hello+World", result.search_metadata.query);
            Assert.AreEqual(HttpStatusCode.OK, result.HttpResponseMessage.StatusCode);
        }

        [Test, Ignore("Secrets")]
        public async Task Get_UserTimelineUsingTwitterHandler_ReturnsUsersTimeline()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config
            (
                new TwitterHandler
                (
                    consumerKey:"", consumerKeySecret:"", accessToken:"", accessTokenSecret:""
                )));

            var result = await restClient.Statuses.Home_Timeline.Get();
            IEnumerable<dynamic> timeline = result;
            
            Assert.NotZero(timeline.Count());
            Assert.AreEqual(HttpStatusCode.OK, result.HttpResponseMessage.StatusCode);
        }

        [Test, Ignore("Secrets")]
        public async Task Post_ImageThenStatusUpdateUsingTwitterHandler_CorrectlyPostsImageAndUpdateStatus()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config
            (
                new TwitterHandler
                (
                    consumerKey:"", consumerKeySecret:"", accessToken:"", accessTokenSecret:""
                )));

            var filepath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + "/DalSoft.jpg";
            var fileBytes = File.ReadAllBytes(filepath);
            
            var mediaUploadResult =  await restClient.Media.Upload.Post( new { media = fileBytes } );
            var statusUpdateResult = await restClient.Statuses.Update.Post( new { status = "Upload" , trim_user = "1", media_ids = mediaUploadResult.media_id } );

            Assert.AreEqual(HttpStatusCode.OK, mediaUploadResult.HttpResponseMessage.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, statusUpdateResult.HttpResponseMessage.StatusCode);
        }

        [TestCase("www.instagram.com")] // A Grade
        public async Task Websites_SecurityHeaders_ShouldBeAGradeOrAbove(string url)
        {
            var restClient = new RestClient("https://securityheaders.com/");
            var response = await restClient.Query(new { q = url, followRedirects = "on" }).Head();

            var grade = response.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.FirstOrDefault())["X-Grade"];

            Assert.That(grade.StartsWith("A"), $"GRADE {grade}: {url}");

            Console.WriteLine($"GRADE {grade}: {url}");
        }

        //ToDO: Refactor These Tests

        [Test]
        public async Task Get_UsingStronglyTypedRestClient_CorrectlyReturnsDynamicResponse()
        {
            var client = new RestClient(BaseUri);

            User user = await client // Strongly typed RestClient
                .Resource("users")
                .Resource("1")
                .Get();

            Assert.AreEqual(1, user.id);
        }

        [Test]
        public async Task Get_UsingStronglyTypedRestClient_CanBeCastToHttpResponseMessage()
        {
            var client = new RestClient(BaseUri);

            HttpResponseMessage httpResponseMessage = await client //Strongly typed RestClient then cast to HttpResponseMessage
                .Resource("users")
                .Resource("1")
                .Get();

            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
        }

        [Test]
        public async Task Get_UsingStronglyTypedRestClient_HttpMethodExtension()
        {
            var client = new RestClient(BaseUri);

            User user = await client // Strongly typed RestClient extension method on resource
                .Users(1) // extension method
                .Get();

            Assert.AreEqual(1, user.id);
        }

        [Test]
        public async Task Get_UsingStronglyTypedRestClient_HttpMethodAndResponseExtension()
        {
            var client = new RestClient(BaseUri);
            
            var user = await client.GetUserById(1); // Strongly typed RestClient extension method full verb

            Assert.AreEqual(1, user.id);
        }

        [Test]
        public async Task Get_UsingDynamicallyTypedRestClient_HttpMethodAndResponseExtension()
        {
            var client = new RestClient(BaseUri);
            
            var user = await client.GetUserByIdDynamic(1); // dynamic extension method full verb

            Assert.AreEqual(1, user.id);
        }

        [Test]
        public async Task Get_UsingStronglyTypedRestClient_CanBeCastBackToDynamic()
        {
            var client = new RestClient(BaseUri);

            dynamic testBackAsDynamic = client; // Strongly typed RestClient cast back to dynamic
            var user = await testBackAsDynamic.Users(1).Get();

            Assert.AreEqual(1, user.id);
        }
 
        [Test]
        public async Task UsingStronglyTypedRestClient_CanBeCastToUsableHttpClient()
        {
            var client = new RestClient(BaseUri);

            //  Get underline httpclient
            HttpClient httpClient = (dynamic)client;

            var httpClientResponseString = await httpClient.GetStringAsync(client.BaseUri + "/users/1");

            Assert.True(httpClientResponseString.Contains("Leanne Graham"));
        }

        [Test]
        public async Task Delete_UsingStronglyTypedRestClient_CorrectlyReturnsDynamicResponse()
        {
            var client = new RestClient(BaseUri);
            
            HttpResponseMessage httpResponseMessage = await client
                .Resource("users")
                .Resource("1")
                .Delete();

            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
        }

        [Test]
        public async Task Post_UsingStronglyTypedRestClient_CorrectlyReturnsDynamicResponse()
        {
            var client = new RestClient(BaseUri);

            User user = await client
                .Resource("users")
                .Post(new User { name = "DalSoft" });

            Assert.AreEqual("DalSoft", user.name);
        }
    }
}
