using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.Handlers
{
    [TestFixture]
    public class RetryHandlerTests
    {
        [Test]
        public void Ctor_ParameterLess_SetDefaultCorrectly()
        {
            const int defaultMaxRetries = 3;
            const double defaultWaitToRetryInSeconds = 1.44;
            const int defaultMaxWaitToRetryInSeconds = 10;
            const RetryHandler.BackOffStrategy defaultBackOffStrategy = RetryHandler.BackOffStrategy.Exponential;

            var retryHandler = new RetryHandler { BackOffFunc = seconds => Task.FromResult(0) };
       
            Assert.That(retryHandler.MaxRetries, Is.EqualTo(defaultMaxRetries));
            Assert.That(retryHandler.WaitToRetryInSeconds, Is.EqualTo(defaultWaitToRetryInSeconds));
            Assert.That(retryHandler.MaxWaitToRetryInSeconds, Is.EqualTo(defaultMaxWaitToRetryInSeconds));
            Assert.That(retryHandler.CurrentBackOffStrategy, Is.EqualTo(defaultBackOffStrategy));
        }


        [Test]
        public void Ctor_PassMaxRetries_MaxRetriesShouldBeSet()
        {
            const int maxRetries = 10;

            var retryHandler = new RetryHandler(maxRetries, 2, 10, RetryHandler.BackOffStrategy.Exponential) { BackOffFunc = seconds => Task.FromResult(0) };

            Assert.That(retryHandler.MaxRetries, Is.EqualTo(maxRetries));
        }

        [Test]
        public void Ctor_PassWaitToRetryInSeconds_WaitToRetryInSecondsShouldBeSet()
        {
            const double waitToRetryInSeconds = 10.4d;

            var retryHandler = new RetryHandler(3, waitToRetryInSeconds, 10, RetryHandler.BackOffStrategy.Exponential) { BackOffFunc = seconds => Task.FromResult(0) };

            Assert.That(retryHandler.WaitToRetryInSeconds, Is.EqualTo(waitToRetryInSeconds));
        }

        [Test]
        public void Ctor_PassMaxWaitToRetryInSeconds_MaxWaitToRetryInSecondsShouldBeSet()
        {
            const double maxWaitToRetryInSeconds = 100.49d;

            var retryHandler = new RetryHandler(3, 2, maxWaitToRetryInSeconds, RetryHandler.BackOffStrategy.Exponential) { BackOffFunc = seconds => Task.FromResult(0) };

            Assert.That(retryHandler.MaxWaitToRetryInSeconds, Is.EqualTo(maxWaitToRetryInSeconds));
        }

        [Test]
        public void Ctor_PassBackOffStrategy_CurrentBackOffStrategyShouldBeSet()
        {
            const RetryHandler.BackOffStrategy backOffStrategy = RetryHandler.BackOffStrategy.Linear;

            var retryHandler = new RetryHandler(3, 2, 10, backOffStrategy) { BackOffFunc = seconds => Task.FromResult(0) };

            Assert.That(retryHandler.CurrentBackOffStrategy, Is.EqualTo(backOffStrategy));
        }

        [TestCase(1)]
        [TestCase(101)]
        public void Ctor_PassMaxRetriesLessThan2OrGreaterThan100_ThrowsArgumentException(int maxRetries)
        {
            Assert.Throws<ArgumentException>(() => new RetryHandler(maxRetries, 2, 10, RetryHandler.BackOffStrategy.Exponential) { BackOffFunc = seconds => Task.FromResult(0) });
        }


        [TestCase(1)]
        [TestCase(601)]
        public void Ctor_PassWaitToRetryInSecondsLessThan1dot1OrGreaterThen600_ThrowsArgumentException(double waitToRetryInSeconds)
        {
            Assert.Throws<ArgumentException>(() => new RetryHandler(3, waitToRetryInSeconds, 10, RetryHandler.BackOffStrategy.Exponential) { BackOffFunc = seconds => Task.FromResult(0) });
        }

        [TestCase(1)]
        [TestCase(601)]
        public void Ctor_PassMaxWaitToRetryInSecondsLessThan10OrGreaterThan600_ThrowsArgumentException(int maxWaitToRetryInSeconds)
        {
            Assert.Throws<ArgumentException>(() => new RetryHandler(3, 2, maxWaitToRetryInSeconds, RetryHandler.BackOffStrategy.Exponential) { BackOffFunc = seconds => Task.FromResult(0) });
        }

        [Test]
        public async Task Send_NoTransientException_ShouldNotBeRetried()
        {
            const int numberTimesToRetry = 5;
            var retryCount = 0;

            var retryHandler = new RetryHandler(numberTimesToRetry, 2, 10, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    retryCount++;
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler).UseUnitTestHandler(request => new HttpResponseMessage()));

            await restClient.Get();

            Assert.That(retryCount, Is.EqualTo(0));
        }

        [Test]
        public async Task Send_NoTransientException_ShouldReturnTheResponseCorrectly()
        {
            var retryHandler = new RetryHandler
            {
                BackOffFunc = seconds => Task.FromResult(0)
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler).UseUnitTestHandler(request => new HttpResponseMessage
            {
                Content = new StringContent("{ 'foo' : 'bar' }")
            }));

            var result = await restClient.Get();

            Assert.That(result.foo, Is.EqualTo("bar"));
        }

        [Test]
        public async Task Send_NoTransientExceptionOnRetry_ShouldReturnTheResponseCorrectly()
        {
            var retryCount = 0;
            var retryHandler = new RetryHandler(5, 2, 10, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    retryCount++;
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler).UseUnitTestHandler(
            request =>
            {
                if (retryCount == 4)
                    return new HttpResponseMessage { Content = new StringContent("{ 'foo' : 'bar' }")};

                throw new HttpRequestException(WebExceptionStatus.SendFailure.ToString(), new WebException(WebExceptionStatus.SendFailure.ToString(), WebExceptionStatus.SendFailure));
            }));

            var result = await restClient.Get();

            Assert.That(result.foo, Is.EqualTo("bar"));
        }

        [Test]
        public async Task Send_NoTransientExceptionOnRetryWindowsOnly_ShouldReturnTheResponseCorrectly()
        {
            var retryCount = 0;
            var retryHandler = new RetryHandler(5, 2, 10, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    retryCount++;
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler).UseUnitTestHandler(
            request =>
            {
                if (retryCount == 4)
                    return new HttpResponseMessage { Content = new StringContent("{ 'foo' : 'bar' }") };

                throw new HttpRequestException(WebExceptionStatus.SendFailure.ToString(), new Win32Exception((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_CONNECTION_ERROR));
            }));

            var result = await restClient.Get();

            Assert.That(result.foo, Is.EqualTo("bar"));
        }

        [Test]
        public void Send_HttpRequestExceptionButNotTransient_ThrowsExceptionAndDoesNotRetry()
        {
            var retryCount = 0;
            var retryHandler = new RetryHandler(5, 2, 10, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    retryCount++;
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler).UseUnitTestHandler(
            request =>
            {
                throw new HttpRequestException(WebExceptionStatus.TrustFailure.ToString(), new WebException(WebExceptionStatus.TrustFailure.ToString(), WebExceptionStatus.TrustFailure));
            }));

            var exception = Assert.ThrowsAsync<HttpRequestException>(async () => await restClient.Get());
            Assert.That((exception?.InnerException as WebException)?.Status, Is.EqualTo(WebExceptionStatus.TrustFailure));
            Assert.That(retryCount, Is.EqualTo(0));
        }

        [Test]
        public void Send_HttpRequestExceptionButNotTransientWindowsONLY_ThrowsExceptionAndDoesNotRetry()
        {
            const int nativeErrorCode = 1234;
            var retryCount = 0;
            var retryHandler = new RetryHandler(5, 2, 10, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    retryCount++;
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler).UseUnitTestHandler(
            request =>
            {
                throw new HttpRequestException("TrustFailure", new Win32Exception(nativeErrorCode));
            }));

            var exception = Assert.ThrowsAsync<HttpRequestException>(async () => await restClient.Get());
            Assert.That((exception?.InnerException as Win32Exception)?.NativeErrorCode, Is.EqualTo(nativeErrorCode));
            Assert.That(retryCount, Is.EqualTo(0));
        }

        [Test]
        public void Send_ExceptionButNotAHttpRequestException_ThrowsExceptionAndDoesNotRetry()
        {
            var retryCount = 0;
            var retryHandler = new RetryHandler(5, 2, 10, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    retryCount++;
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler).UseUnitTestHandler(
            request =>
            {
                throw new InvalidOperationException();
            }));

            Assert.ThrowsAsync<InvalidOperationException>(async () => await restClient.Get());
            Assert.That(retryCount, Is.EqualTo(0));
        }

        [Test]
        public void Send_TaskCanceledExceptionThrown_ThrowsHttpRequestException()
        {
            var retryHandler = new RetryHandler
            {
                BackOffFunc = seconds => Task.FromResult(0)
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler)
                .UseUnitTestHandler(request => { throw new TaskCanceledException(); }));

            var httpRequestException = Assert.ThrowsAsync<HttpRequestException>(async () => await restClient.Get());

            Assert.That(httpRequestException.Message, Is.EqualTo("Request timed out"));
        }

        [TestCase(WebExceptionStatus.SendFailure)]
        [TestCase(WebExceptionStatus.ReceiveFailure)]
        [TestCase(WebExceptionStatus.ConnectFailure)]
        [TestCase(WebExceptionStatus.NameResolutionFailure)]
        [TestCase(WebExceptionStatus.RequestCanceled)]
        [TestCase(WebExceptionStatus.ConnectionClosed)]
        [TestCase(WebExceptionStatus.ProxyNameResolutionFailure)]
        [TestCase(WebExceptionStatus.KeepAliveFailure)]
        [TestCase(WebExceptionStatus.Timeout)]
        [TestCase(WebExceptionStatus.Pending)]
        public void Send_TransientExceptionEncountered_ShouldBeRetried(WebExceptionStatus webExceptionStatus)
        {
            const int numberTimesToRetry = 5;
            var retryCount = 0;

            var retryHandler = new RetryHandler(numberTimesToRetry, 2, 10, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    retryCount++;
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler)
                .UseUnitTestHandler(request =>
                {
                    throw new HttpRequestException(webExceptionStatus.ToString(), new WebException(webExceptionStatus.ToString(), webExceptionStatus));
                }));

            var exception = Assert.ThrowsAsync<HttpRequestException>(async () => await restClient.Get());
            var webException = exception.InnerException as WebException;

            Assert.That(exception.Data["IsTransient"], Is.EqualTo(true));
            Assert.That(webException?.Status, Is.EqualTo(webExceptionStatus));
            Assert.That(retryCount, Is.EqualTo(numberTimesToRetry));
        }

#if (NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NET461) //.NET CORE < 2.1 & .NET Standard 2.0 on WINDOWS 
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_CONNECTION_ERROR)]
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_AUTO_PROXY_SERVICE_ERROR)]
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_CANNOT_CALL_AFTER_OPEN)]
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_CANNOT_CALL_AFTER_SEND)]
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_CANNOT_CALL_BEFORE_OPEN)]
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_CANNOT_CALL_BEFORE_SEND)]
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_CANNOT_CONNECT)]
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_NAME_NOT_RESOLVED)]
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_OPERATION_CANCELLED)]
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_RESEND_REQUEST)]
        [TestCase((int)RetryHandler.WinHttpNativeErrorCode.ERROR_WINHTTP_SHUTDOWN)]
#endif
#if (NETCOREAPP2_1 || NETCOREAPP2_2) // .NET CORE > 2.1 on WINDOWS 
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSA_OPERATION_ABORTED)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAEWOULDBLOCK)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAENETDOWN)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAENETUNREACH)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAENETRESET)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAECONNABORTED)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAECONNRESET)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAENOBUFS)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAETIMEDOUT)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAECONNREFUSED)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAEHOSTDOWN)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAEHOSTUNREACH)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSAHOST_NOT_FOUND)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSATRY_AGAIN)]
        [TestCase((int)RetryHandler.WinWSANativeErrorCode.WSANO_DATA)]
#endif
        public void Send_TransientExceptionEncounteredWindowsOnly_ShouldBeRetried(int winHttpNativeErrorCode)
        {
            const int numberTimesToRetry = 5;
            var retryCount = 0;

            var retryHandler = new RetryHandler(numberTimesToRetry, 2, 10, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    retryCount++;
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler)
                .UseUnitTestHandler(request => throw new HttpRequestException(winHttpNativeErrorCode.ToString(), new Win32Exception(winHttpNativeErrorCode))));

            var exception = Assert.ThrowsAsync<HttpRequestException>(async () => await restClient.Get());
            var win32Exception = exception.InnerException as Win32Exception;

            Assert.That(exception.Data["IsTransient"], Is.EqualTo(true));
            Assert.That(win32Exception?.NativeErrorCode, Is.EqualTo(winHttpNativeErrorCode));
            Assert.That(retryCount, Is.EqualTo(numberTimesToRetry));
        }

        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.GatewayTimeout)]
        [TestCase(HttpStatusCode.BadGateway)]
        [TestCase(HttpStatusCode.HttpVersionNotSupported)]
        public async Task Send_InternalServerStatusCodeEncountered_ShouldBeRetried(HttpStatusCode statusCode)
        {
            const int numberTimesToRetry = 5;
            var retryCount = 0;

            var retryHandler = new RetryHandler(numberTimesToRetry, 2, 10, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    retryCount++;
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler)
                .UseUnitTestHandler(request => new HttpResponseMessage(statusCode)));

            var result = await restClient.Get();

            Assert.That(retryCount, Is.EqualTo(numberTimesToRetry));
            Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(statusCode));
        }

        [Test]
        public void Send_BackOffLinear_ShouldBackOffByWaitToRetryInSeconds()
        {
            const int waitToRetryInSeconds = 5;
          
            var retryHandler = new RetryHandler(5, waitToRetryInSeconds, 10, RetryHandler.BackOffStrategy.Linear)
            {
                BackOffFunc = seconds =>
                {
                    Assert.That(seconds, Is.EqualTo(waitToRetryInSeconds));
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler)
                .UseUnitTestHandler(request =>
                {
                    throw new HttpRequestException(WebExceptionStatus.ConnectFailure.ToString(), new WebException(WebExceptionStatus.ConnectFailure.ToString(), WebExceptionStatus.ConnectFailure));
                }));

            Assert.ThrowsAsync<HttpRequestException>(async () => await restClient.Get());
        }

        [Test]
        public void Send_BackOffExponential_ShouldBackOffByWaitToRetryInSecondsGrowingExponentially()
        {
            var retryCount = 0;
            const double waitToRetryInSeconds = 2.1;

            var retryHandler = new RetryHandler(5, waitToRetryInSeconds, 60, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    retryCount++;
                    Assert.That(seconds, Is.EqualTo(Math.Pow(waitToRetryInSeconds, retryCount)));
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler)
                .UseUnitTestHandler(request =>
                {
                    throw new HttpRequestException(WebExceptionStatus.ConnectFailure.ToString(), new WebException(WebExceptionStatus.ConnectFailure.ToString(), WebExceptionStatus.ConnectFailure));
                }));

            Assert.ThrowsAsync<HttpRequestException>(async () => await restClient.Get());
        }

        [Test]
        public void Send_BackOffExponential_ShouldNotBackOffHigherThanMaxWaitToRetryInSeconds()
        {
            const int waitToRetryInSeconds = 5;
            const int maxWaitToRetryInSeconds = 10;

            var retryHandler = new RetryHandler(5, waitToRetryInSeconds, maxWaitToRetryInSeconds, RetryHandler.BackOffStrategy.Exponential)
            {
                BackOffFunc = seconds =>
                {
                    Assert.That(seconds, Is.LessThanOrEqualTo(maxWaitToRetryInSeconds));
                    return Task.FromResult(0);
                }
            };

            dynamic restClient = new RestClient("http://test.test", new Config(retryHandler)
                .UseUnitTestHandler(request =>
                {
                    throw new HttpRequestException(WebExceptionStatus.ConnectFailure.ToString(), new WebException(WebExceptionStatus.ConnectFailure.ToString(), WebExceptionStatus.ConnectFailure));
                }));

            Assert.ThrowsAsync<HttpRequestException>(async () => await restClient.Get());
        }
    }
}
