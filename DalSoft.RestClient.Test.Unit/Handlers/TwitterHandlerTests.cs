using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.Handlers
{
    public class TwitterHandlerTests
    {
        [TestCase(null, "consumerKeySecret", "accessToken", "accessTokenSecret")]
        [TestCase("consumerKey", null, "accessToken", "accessTokenSecret")]
        [TestCase("consumerKey", "consumerKeySecret", null, "accessTokenSecret")]
        [TestCase("consumerKey", "consumerKeySecret", "accessToken", null)]
        public void Ctor_EmptyKeyOrSecret_ThrowsArgumentNullException(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
        {
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(()=>new RestClient("https://api.twitter.com/1.1", new Config().UseTwitterHandler(consumerKey, consumerKeySecret, accessToken, accessTokenSecret)));
        }

        [Test]
        public async Task Send_WhenTwitterApiUrl_AddsTwitterAuthorisationHeader()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config()
                .UseTwitterHandler(consumerKey:"consumerKey", consumerKeySecret:"consumerKeySecret", accessToken:"accessToken", accessTokenSecret:"accessTokenSecret")
                .UseUnitTestHandler(request => new HttpResponseMessage { Content = new StringContent("{ 'fake' : 'response' }") }));

            HttpResponseMessage httpResponseMessage = await restClient.Statuses.Update.Post(new {status = "Test", trim_user = "1"});

            Assert.IsTrue(httpResponseMessage.RequestMessage.Headers.Authorization.ToString().Contains("oauth_"));
        }

        [Test]
        public async Task Send_WhenTwitterUploadUrl_AddsTwitterAuthorisationHeader()
        {
            dynamic restClient = new RestClient("https://upload.twitter.com/1.1", new Config()
                .UseTwitterHandler(consumerKey:"consumerKey", consumerKeySecret:"consumerKeySecret", accessToken:"accessToken", accessTokenSecret:"accessTokenSecret")
                .UseUnitTestHandler(request => new HttpResponseMessage { Content = new StringContent("{ 'fake' : 'response' }") }));

            HttpResponseMessage httpResponseMessage = await restClient.Statuses.Update.Post(new {status = "Test", trim_user = "1"});

            Assert.IsTrue(httpResponseMessage.RequestMessage.Headers.Authorization.ToString().Contains("oauth_"));
        }

        [Test]
        public async Task Send_WhenNotTwitterUrl_DoesToAddTwitterAuthorisationHeader()
        {
            dynamic restClient = new RestClient("https://dalsoft.co.uk", new Config()
                .UseTwitterHandler(consumerKey:"consumerKey", consumerKeySecret:"consumerKeySecret", accessToken:"accessToken", accessTokenSecret:"accessTokenSecret")
                .UseUnitTestHandler(request => new HttpResponseMessage { Content = new StringContent("{ 'fake' : 'response' }") }));

            HttpResponseMessage httpResponseMessage = await restClient.Statuses.Update.Post(new {status = "Test", trim_user = "1"});

            Assert.IsNull(httpResponseMessage.RequestMessage.Headers.Authorization);
        }

        [Test]
        public async Task Send_RestClientStyleChain_ConvertedToUrlTwitterExpects()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config()
                .UseTwitterHandler(consumerKey: "consumerKey", consumerKeySecret: "consumerKeySecret", accessToken: "accessToken", accessTokenSecret: "accessTokenSecret")
                .UseUnitTestHandler(request => new HttpResponseMessage { Content = new StringContent("{ 'fake' : 'response' }") }));

            HttpResponseMessage httpResponseMessage = await restClient.Search.Tweets.Query(new { q = "Hello World" }).Get();

            Assert.AreEqual("https://api.twitter.com/1.1/search/tweets.json?q=Hello%20World", httpResponseMessage.RequestMessage.RequestUri.AbsoluteUri);
        }

        [Test]
        public async Task Send_RestClientStyleChainButWithTwitterStyleDotJsonExtension_ConvertedToUrlTwitterExpects()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config()
                .UseTwitterHandler(consumerKey: "consumerKey", consumerKeySecret: "consumerKeySecret", accessToken: "accessToken", accessTokenSecret: "accessTokenSecret")
                .UseUnitTestHandler(request => new HttpResponseMessage { Content = new StringContent("{ 'fake' : 'response' }") }));

            HttpResponseMessage httpResponseMessage = await restClient.Search.Tweets.Json.Query(new { q = "Hello World" }).Get();

            Assert.AreEqual("https://api.twitter.com/1.1/search/tweets.json?q=Hello%20World", httpResponseMessage.RequestMessage.RequestUri.AbsoluteUri);
        }
        
        [Test] 
        public async Task Send_BinaryOrStreamDataUsingTwitterApi_SwitchesToUploadUrl()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config()
                .UseTwitterHandler(consumerKey: "consumerKey", consumerKeySecret: "consumerKeySecret", accessToken: "accessToken", accessTokenSecret: "accessTokenSecret")
                .UseUnitTestHandler(request => new HttpResponseMessage
                {
                    Content = new StringContent("{ 'fake' : 'response' }")

                }));

            HttpResponseMessage httpResponseMessage = await restClient.Media.Upload.Post( new { media = new byte[] {} } );
            
            Assert.True(httpResponseMessage.RequestMessage.RequestUri.GetLeftPart(UriPartial.Path).StartsWith("https://upload.twitter.com"));
        }

        [Test]
        public async Task Send_PostToTwitterApi_GeneratesCorrectAuthorizationHeaders()
        {
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config()
                .UseTwitterHandler(consumerKey: "consumerKey", consumerKeySecret: "consumerKeySecret", accessToken: "accessToken", accessTokenSecret: "accessTokenSecret")
                .UseUnitTestHandler(request => new HttpResponseMessage { Content = new StringContent("{ 'fake' : 'response' }") }));

            HttpResponseMessage httpResponseMessage = await restClient.Search.Tweets.Json.Query(new { q = "Hello World" }).Get();

            Assert.IsTrue(httpResponseMessage.RequestMessage.Headers.Authorization.ToString().Contains("oauth_consumer_key=\"consumerKey\", "));
            Assert.IsTrue(httpResponseMessage.RequestMessage.Headers.Authorization.ToString().Contains("oauth_signature_method=\"HMAC-SHA1\", "));
            Assert.IsTrue(httpResponseMessage.RequestMessage.Headers.Authorization.ToString().Contains("oauth_signature="));
            Assert.IsTrue(httpResponseMessage.RequestMessage.Headers.Authorization.ToString().Contains("oauth_token=\"accessToken\", "));
            Assert.IsTrue(httpResponseMessage.RequestMessage.Headers.Authorization.ToString().Contains("oauth_version=\"1.0\""));
        }

        [Test] 
        public async Task Send_BinaryOrStreamDataUsingTwitterApi_CreatesMultipartFormDataHandler()
        {
            HttpRequestMessage requestMessage = null;
            
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config()
                .UseTwitterHandler(consumerKey: "consumerKey", consumerKeySecret: "consumerKeySecret", accessToken: "accessToken", accessTokenSecret: "accessTokenSecret")
                .UseUnitTestHandler(request =>
                {
                    requestMessage = request;
                    
                    return new HttpResponseMessage
                    {
                        Content = new StringContent("{ 'fake' : 'response' }")

                    };
                }));

            HttpResponseMessage httpResponseMessage = await restClient.Media.Upload.Post( new { media = new byte[] {} } );
            var multipartFormData  = await ((StreamContent)requestMessage.Content).ReadAsStringAsync();

            Assert.True(httpResponseMessage.RequestMessage.Content.Headers.ContentType.ToString().Contains("multipart/form-data"));
            Assert.True(multipartFormData.Contains("Content-Disposition: form-data; name=media"));
        }

        [Test]
        public async Task Send_PostToTwitterApi_CreatesFormUrlEncodedContent()
        {
            HttpRequestMessage requestMessage = null;
            
            dynamic restClient = new RestClient("https://api.twitter.com/1.1", new Config()
                .UseTwitterHandler(consumerKey: "consumerKey", consumerKeySecret: "consumerKeySecret", accessToken: "accessToken", accessTokenSecret: "accessTokenSecret")
                .UseUnitTestHandler(request =>
                {
                    requestMessage = request;
                    
                    return new HttpResponseMessage
                    {
                        Content = new StringContent("{ 'fake' : 'response' }")

                    };
                }));

            HttpResponseMessage httpResponseMessage = await restClient.Statuses.Update.Post( new { status = "Test" , trim_user = "1" } );
            var formUrlEncodedContent  = await ((StreamContent)requestMessage.Content).ReadAsStringAsync();

            Assert.AreEqual("status=Test&trim_user=1", formUrlEncodedContent);
        }
    }
}
