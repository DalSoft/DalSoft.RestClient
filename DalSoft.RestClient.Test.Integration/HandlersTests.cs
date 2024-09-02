using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Integration
{
    [TestFixture]
    public class HandlersTests
    {
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

        [Test, Ignore("Need to find a new site to test this, or better still create a integration test controller")]
        public async Task Post_MultipartForm_CorrectlyPostsFile()
        {
            dynamic restClient = new RestClient("https://www.directupload.eu/index.php", new Config
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
            Assert.That(result.ToString(), Does.Contain("Dein Bild wurde erfolgreich hochgeladen!")); // Your picture was uploaded successfully!
        }

        [Test]
        public void Get_UsingRetryHandlerWhenTransientExceptionEncountered_ShouldBeRetried()
        {
            // ReSharper disable InconsistentNaming
            const int ERROR_WINHTTP_NAME_NOT_RESOLVED = 12007;
            const int WSAHOST_NOT_FOUND = 11001;
            // ReSharper restore InconsistentNaming

            var numberOfActualTries = 0;

            dynamic restClient = new RestClient("http://A_Url_The_Will_Cause_A_Transient_Exception", new Config()
            .UseRetryHandler(maxRetries: 3, waitToRetryInSeconds: 2, maxWaitToRetryInSeconds: 10, backOffStrategy: RetryHandler.BackOffStrategy.Exponential)
            .UseHandler((request, token, next) =>
            {
                numberOfActualTries++;
                return next(request, token);
            }));

            var exception = Assert.ThrowsAsync<HttpRequestException>(async () => await restClient.Get());
            var webException = exception.InnerException as WebException;
            var win32Exception = exception.InnerException as Win32Exception;

            if (webException != null) //.NET 4.5, .NET Standard all platforms except Windows
                Assert.That(webException.Status, Is.EqualTo(WebExceptionStatus.NameResolutionFailure));
            if (exception.InnerException is SocketException) //.NET Standard post Core 2.1 Windows only https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-core-2-1
                Assert.That(win32Exception.NativeErrorCode, Is.EqualTo(WSAHOST_NOT_FOUND));
            else if (win32Exception != null) //.NET Standard pre Core 2.1 Windows only https://github.com/dotnet/corefx/issues/19185
                Assert.That(win32Exception.NativeErrorCode, Is.EqualTo(ERROR_WINHTTP_NAME_NOT_RESOLVED));

            Assert.That(numberOfActualTries, Is.EqualTo(4)); //maxRetries + the first attempt
        }

        [Test, Ignore("Secrets")]
        public async Task Post_StatusUpdateUsingTwitterHandler_CorrectlyUpdateStatus()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config
            (
                new TwitterHandler
                (
                    consumerKey: "", consumerKeySecret: "", accessToken: "", accessTokenSecret: ""
                )));

            var result = await restClient.Statuses.Update.Post(new { status = "Test", trim_user = "1" });

            Assert.AreEqual(HttpStatusCode.OK, result.HttpResponseMessage.StatusCode);
        }

        [Test, Ignore("Secrets")]
        public async Task Get_SearchTweetsUsingTwitterHandler_ReturnsTwitterSearchResults()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config
            (
                new TwitterHandler
                (
                    consumerKey: "", consumerKeySecret: "", accessToken: "", accessTokenSecret: ""
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
                    consumerKey: "", consumerKeySecret: "", accessToken: "", accessTokenSecret: ""
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
                    consumerKey: "", consumerKeySecret: "", accessToken: "", accessTokenSecret: ""
                )));

            var filepath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + "/DalSoft.jpg";
            var fileBytes = File.ReadAllBytes(filepath);

            var mediaUploadResult = await restClient.Media.Upload.Post(new { media = fileBytes });
            var statusUpdateResult = await restClient.Statuses.Update.Post(new { status = "Upload", trim_user = "1", media_ids = mediaUploadResult.media_id });

            Assert.AreEqual(HttpStatusCode.OK, mediaUploadResult.HttpResponseMessage.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, statusUpdateResult.HttpResponseMessage.StatusCode);
        }
    }
}
