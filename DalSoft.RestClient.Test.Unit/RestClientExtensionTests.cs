using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Test.Unit.TestData.Models;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit
{
    [TestFixture]
    public class RestClientExtensionTests
    {
        private const string Json = "{ 'name': 'Leanne Graham', 'username': 'Bret' }";
        private RestClient _internalServerErrorRestClient;
        private RestClient _restClient;


        [SetUp]
        public void RestExtensionTestsSetUp()
        {
            _internalServerErrorRestClient = new RestClient("https://jsonplaceholder.typicode.com/",
                new Config().UseUnitTestHandler(message =>
                    new HttpResponseMessage
                    {
                        Content = new StringContent(Json),
                        StatusCode = HttpStatusCode.InternalServerError
                    }));

            _restClient = new RestClient("https://jsonplaceholder.typicode.com/",
                new Config().UseUnitTestHandler(message =>
                    new HttpResponseMessage
                    {
                        Content = new StringContent(Json)
                    }));
        }


        [Test]
        public async Task Verify_NoFailingVerification_ShouldReturnResponse()
        {
           var result = await _restClient.Resource("users/1").Get()
                .Verify<HttpResponseMessage>(response => response.IsSuccessStatusCode) // Verify using HttpResponseMessage
                .Verify<string>(s => s.Contains("Leanne Graham")) // Verify string response body
                .Verify<User>(user => user.username == "Bret") // Verify model
                .Verify(o => o.username == "Bret") // Verify dynamically
                .Verify(o => o.HttpResponseMessage.IsSuccessStatusCode); // Verify dynamically

           Assert.AreEqual("Bret", result.username);
        }

        [Test]
        public void Verify_FailingVerifications_ThrowsAggregateVerifiedFailedException()
        {
            var verifyIsSuccessStatusCode = Verify.Expression<HttpResponseMessage>(response => response.IsSuccessStatusCode);
            var verifyIsSuccessStatusCodeDynamically = Verify.Expression(verifySuccessStatusCode => verifySuccessStatusCode.HttpResponseMessage.IsSuccessStatusCode);

            var verifiedFailedException = Assert.ThrowsAsync<AggregateException>(async () =>
            {
                await _internalServerErrorRestClient.Resource("users/1").Get()
                    .Verify<HttpResponseMessage>(verifyIsSuccessStatusCode) // Verify using HttpResponseMessage this should fail
                    .Verify<string>(s => s.Contains("Leanne Graham")) // Verify string response body
                    .Verify<User>(user => user.username == "Bret") // Verify model
                    .Verify(o => o.username == "Bret") // Verify dynamically
                    .Verify(verifyIsSuccessStatusCodeDynamically); // Verify dynamically this should fail
            });

            Assert.AreEqual(2, verifiedFailedException.InnerExceptions.Count);
            Assert.IsNotNull(verifiedFailedException.InnerExceptions.SingleOrDefault(x => x.Message == Verify.FailedMessage(verifyIsSuccessStatusCode)));
            Assert.IsNotNull(verifiedFailedException.InnerExceptions.SingleOrDefault(x => x.Message == Verify.FailedMessage(verifyIsSuccessStatusCodeDynamically)));
        }


        [Test]
        public async Task OnVerifyFailed_FailingVerificationsByDefault_ExceptionNotThrownAndOnlyFirstOnVerifyFailedCalled()
        {
            var invoked = 0;
            await _internalServerErrorRestClient.Resource("users/1").Get()
                .Verify<HttpResponseMessage>(response => response.IsSuccessStatusCode)                    // Verify using HttpResponseMessage
                .Verify<string>(s => s.Contains("Leanne GrahamXXX"))                                      // Verify string response body
                .Verify<User>(user => user.username == "Bret")                                            // Verify model
                .Verify(o => o.username == "Bret")                                                        // Verify dynamically
                .Verify(o => o.HttpResponseMessage.IsSuccessStatusCode)                                   // Verify dynamically
                .OnException((exception, response) =>
                {
                    Assert.AreEqual("Bret", response.username);
                    Assert.AreEqual("Bret", ((User)response).username);
                    invoked++;
                })
                .OnException<HttpResponseMessage>((exception, httpResponseMessage) => { invoked++; }); // Not Called

            Assert.AreEqual(1, invoked);
        }

        [Test]
        public async Task OnVerifyFailed_FailingVerificationByDefault_ExceptionNotThrownAndResponseCanBeUsedByFurtherContinuations()
        {
            var invoked = 0;

            var result = await _internalServerErrorRestClient.Resource("users/1").Get()
                .Verify<HttpResponseMessage>(response => response.IsSuccessStatusCode) 
                .Verify<string>(s => s.Contains("Leanne GrahamXXX")) 
                .Verify<User>(user => user.username == "Bret") 
                .OnException((exception, response) => { invoked++; })
                .Verify(verifyUsername => verifyUsername.username == "BretX")
                .Verify(verifyIsSuccessStatusCode => verifyIsSuccessStatusCode.HttpResponseMessage.IsSuccessStatusCode)
                .OnException<User>((exception, user) => {  invoked++; })
                .As<User>();

            Assert.AreEqual(2, invoked);
            Assert.AreEqual("Leanne Graham", result.name);
        }

        [Test]
        public async Task OnVerifyFailed_FailingVerificationAndThrowOnVerifyFailedTrue_NextOnVerifyFailedCalledAndExceptionsAreCorrectBetweenCallbacks()
        {
            var invoked = 0;
            await _internalServerErrorRestClient.Resource("users/1").Get()
                .Verify<HttpResponseMessage>(response => response.IsSuccessStatusCode)
                .Verify<string>(s => s.Contains("Leanne GrahamXXX"))
                .Verify<User>(user => user.username == "Bret")
                .OnException((exception, response) =>
                {
                    Assert.AreEqual("Bret", response.username);
                    Assert.AreEqual("Bret", ((User)response).username);
                    Assert.True(exception.InnerExceptions.Count == 2);
                    invoked++;
                }, throwException:true)                                                // Throw original verification exceptions to next handler 
                .Verify(o => o.username == "Bret")
                .Verify(o => o.HttpResponseMessage.IsSuccessStatusCode)
                .OnException<HttpResponseMessage>((exception, httpResponseMessage) =>   // Next handler is called
                {
                    Assert.False(httpResponseMessage.IsSuccessStatusCode);
                    Assert.True(exception.InnerExceptions.Count == 3);                     // Original verification exceptions + this verification exception == 3
                    invoked++;
                });

            Assert.AreEqual(2, invoked);
        }

        [Test]
        public void OnVerifyFailed_FailingVerificationAndThrowOnVerifyFailedTrueOnLastCallback_ThrowsAggregateVerifiedFailedExceptionAndExceptionsAreCorrectBetweenCallbacks()
        {
            var invoked = 0;

            var verifiedFailedException = Assert.ThrowsAsync<AggregateException>(async () =>
            {
                await _internalServerErrorRestClient.Resource("users/1").Get()
                    .Verify<HttpResponseMessage>(response => response.IsSuccessStatusCode) 
                    .Verify<string>(s => s.Contains("Leanne GrahamXXX")) 
                    .Verify<User>(user => user.username == "Bret") 
                    .OnException((exception, response) =>
                    {
                        Assert.AreEqual("Bret", response.username);
                        Assert.AreEqual("Bret", ((User)response).username);
                        Assert.True(exception.InnerExceptions.Count == 2);
                        invoked++;
                    })                                                               // Do not throw verification exceptions to next handler 
                    .Verify(verifyUsername => verifyUsername.username == "BretX") 
                    .Verify(verifyIsSuccessStatusCode => verifyIsSuccessStatusCode.HttpResponseMessage.IsSuccessStatusCode) 
                    .OnException<User>((exception, user) =>
                    {
                        Assert.AreEqual("Bret", user.username);
                        Assert.True(exception.InnerExceptions.Count == 2);
                        invoked++;
                    }, throwException: true);                                    // Last handler throws only it's verification exceptions
            });

            Assert.AreEqual(2, invoked);
            Assert.True(verifiedFailedException.InnerExceptions.Count == 2);
        }

        [Test]
        public void OnVerifyFailed_FailingVerificationAndThrowOnVerifyFailedTrueOnAllCallbacks_ExceptionThrownAndResponseCanNotBeUsedByFurtherContinuations()
        {
            var invoked = 0;

            var verifiedFailedException = Assert.ThrowsAsync<AggregateException>(async () =>
            {
                await _internalServerErrorRestClient.Resource("users/1").Get()
                    .Verify<HttpResponseMessage>(response => response.IsSuccessStatusCode)
                    .Verify<string>(s => s.Contains("Leanne GrahamXXX"))
                    .Verify<User>(user => user.username == "Bret")
                    .OnException((exception, response) =>
                    {
                        invoked++;
                    }, throwException: true)             // Throw verification exceptions to next handler                                                                                        
                    .Verify(verifyUsername => verifyUsername.username == "BretX")
                    .Verify(verifyIsSuccessStatusCode => verifyIsSuccessStatusCode.HttpResponseMessage.IsSuccessStatusCode)
                    .OnException<User>((exception, user) =>
                    {
                        invoked++;
                    }, throwException: true)            // Last handler throws only all verification exceptions
                    .As<User>();                             // No Further Continuations will be executed except OnVerifyFailed
            });

            Assert.AreEqual(2, invoked);
            Assert.AreEqual(4, verifiedFailedException.InnerExceptions.Count);
        }

        [Test]
        public async Task As_CastAsHttpResponseMessage_CastAsExpected()
        {
            var httpResponseMessage = await _restClient.Resource("users/1").Get()
                .As<HttpResponseMessage>();
            
            Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
        }

        [Test]
        public async Task As_CastAsString_CastAsExpected()
        {
            var responseBody = await _restClient.Resource("users/1").Get()
                .As<string>();

            Assert.AreEqual(Json, responseBody);
        }

        [Test]
        public async Task As_CastAsObject_CastAsExpected()
        {
            var user = await _restClient.Resource("users/1").Get()
                .As<User>();

            Assert.AreEqual("Leanne Graham", user.name);
        }

        [Test]
        public async Task Map_MapFromDynamicObject_MapsAsExpected()
        {
            var user = await _restClient.Resource("users/1").Get()
                .Map(response => new UserCamelCase
                {
                    Name = response.name,
                    UserName = response.username
                });

            Assert.AreEqual("Leanne Graham", user.Name);
            Assert.AreEqual("Bret", user.UserName);
        }

        [Test]
        public async Task Map_MapFromObject_MapsAsExpected()
        {
            var user = await _restClient.Resource("users/1").Get()
                .Map<User, UserCamelCase>(response => new UserCamelCase
                {
                    Name = response.name,
                    UserName = response.username
                });

            Assert.AreEqual("Leanne Graham", user.Name);
            Assert.AreEqual("Bret", user.UserName);
        }

        [Test]
        public void Map_MapFromDynamicObjectNullMap_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _restClient.Resource("users/1").Get().Map<User>(null);
            });
        }

        [Test]
        public void Map_MapFromObjectNullMap_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _restClient.Resource("users/1").Get().Map<User, UserCamelCase>(null);
            });
        }

        [Test]
        public async Task Act_ActOnDynamicObject_ActsAsExpected()
        {
            await _restClient.Resource("users/1").Get()
                .Act(response =>
                { 
                    Assert.AreEqual("Leanne Graham", response.name);
                    Assert.AreEqual("Bret", response.username);
                });
        }

        [Test]
        public async Task Act_ActOnObject_ActsAsExpected()
        {
            await _restClient.Resource("users/1").Get()
                .Act<User>(response =>
                {
                    Assert.AreEqual("Leanne Graham", response.name);
                    Assert.AreEqual("Bret", response.username);
                });
        }

        [Test]
        public void Act_ActOnDynamicObjectNullAct_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _restClient.Resource("users/1").Get().Act(null);
            });
        }
        
        [Test]
        public async Task Act_ThrowsException_ThrowsCorrectException()
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _restClient.Resource("users/1").Get()
                    .Act<User>(response =>
                    {
                        Assert.AreEqual("Leanne Graham", response.name);
                        Assert.AreEqual("Bret", response.username);
                    })
                    .Act(x => throw new InvalidOperationException("testing exception is thrown"));
            });
        }
        
        [Test]
        public async Task Act_ThrowsException_ThrowsCorrectExceptionAndEndsTheChain()
        {
            var shouldNotBeCalled = false;
            
            var exception = Assert.ThrowsAsync<AggregateException>(async () =>
            {
                await _restClient.Resource("users/1").Get()
                    .Act<User>(response =>
                    {
                        Assert.AreEqual("Leanne Graham", response.name);
                        Assert.AreEqual("Bret", response.username);
                    })
                    .Act(x => throw new InvalidOperationException("testing exception is thrown"))
                    .Act(x =>
                    {
                        shouldNotBeCalled = true;
                    });
            });
            Assert.AreEqual(typeof(InvalidOperationException), exception.InnerExceptions.First().GetType());
            Assert.False(shouldNotBeCalled);
        }
        
        [Test]
        public async Task Act_ThrowsExceptionsWithOnExceptionsInChain_OnExceptionsAreCalledAsExpectedAndChainContinuesToTheEnd()
        {
            var shoulBeCalled = false;
            AggregateException aggregateException1 = null;
            AggregateException aggregateException2 = null;
            dynamic response1 = null;
            dynamic response2 = null;

            Assert.DoesNotThrowAsync(async () =>
            {
                await _restClient.Resource("users/1").Get()
                    .Act<User>(response =>
                    {
                        Assert.AreEqual("Leanne Graham", response.name);
                        Assert.AreEqual("Bret", response.username);
                    })
                    .Act(x => throw new InvalidOperationException("testing exception is not thrown"))
                    .OnException((ex, response) =>
                    {
                        // If you want to handle swallow exception you need .OnException after each call to Act, As, Map, Verify
                        aggregateException1 = ex;
                        response1 = response;
                    })
                    .Act(x => throw new IOException("testing exception is not thrown"))
                    .OnException((ex, response) =>
                    {
                        // If you want to handle swallow exception you need .OnException after each call to Act, As, Map, Verify
                        aggregateException2 = ex;
                        response2 = response;

                    })
                    .Act(x => { shoulBeCalled = true; });
            });
            
            Assert.True(shoulBeCalled);
            Assert.AreEqual(typeof(InvalidOperationException), aggregateException1.InnerException?.GetType());
            Assert.AreEqual(typeof(IOException), aggregateException2.InnerException?.GetType());
            Assert.AreEqual(response1?.name, "Leanne Graham");
            Assert.AreEqual(response2?.name, "Leanne Graham");
        }
        
        [Test]
        public async Task Act_ThrowsExceptionWithOnExceptionWithThrowExceptionTrue_ThrowsCorrectExceptionAndEndsTheChain()
        {
            var shouldNotBeCalled = false;
            dynamic response1 = null;
            
            var exception = Assert.ThrowsAsync<AggregateException>(async () =>
            {
                await _restClient.Resource("users/1").Get()
                    .Act<User>(response =>
                    {
                        Assert.AreEqual("Leanne Graham", response.name);
                        Assert.AreEqual("Bret", response.username);
                    })
                    .Act(x => throw new InvalidOperationException("testing exception is thrown"))
                    .OnException((ex, response) =>
                    {
                        response1 = response;

                    }, throwException: true)
                    .Act(x =>
                    {
                        shouldNotBeCalled = true;
                    });
            });
            Assert.AreEqual(typeof(InvalidOperationException), exception.InnerExceptions.First().GetType());
            Assert.AreEqual(response1?.name, "Leanne Graham");
            Assert.False(shouldNotBeCalled);
        }
        
        [Test]
        public void Map_ActOnObjectNullAct_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _restClient.Resource("users/1").Get().Act<User>(null);
            });
        }
    }
}
