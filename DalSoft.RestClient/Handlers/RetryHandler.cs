using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DalSoft.RestClient.Handlers
{
    public class RetryHandler : DelegatingHandler
    {
        internal Func<double, Task> BackOffFunc = seconds => Task.Delay(TimeSpan.FromSeconds(seconds)); //Test seam

        internal readonly int MaxRetries;
        internal readonly double WaitToRetryInSeconds;
        internal readonly double MaxWaitToRetryInSeconds;
        internal readonly BackOffStrategy CurrentBackOffStrategy;

        private Exception _lastException;
       
        //https://docs.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific
        public RetryHandler() : this(3, 1.44, 10, BackOffStrategy.Exponential) { }
        
        public RetryHandler(int maxRetries, double waitToRetryInSeconds, double maxWaitToRetryInSeconds, BackOffStrategy backOffStrategy)
        {
            MaxRetries = maxRetries;
            WaitToRetryInSeconds = waitToRetryInSeconds;
            MaxWaitToRetryInSeconds = maxWaitToRetryInSeconds;
            CurrentBackOffStrategy = backOffStrategy;

            Validate();
        }

        private void Validate()
        {
            if (MaxRetries < 2 || MaxRetries > 100)
                throw new ArgumentException("maxRetries must be between 2 and 100", nameof(MaxRetries));

            if (WaitToRetryInSeconds < 1.1d || WaitToRetryInSeconds > 600)
                throw new ArgumentException("waitToRetryInSeconds must be between 1.1 and 600", nameof(WaitToRetryInSeconds));

            if (MaxWaitToRetryInSeconds < 10 || MaxWaitToRetryInSeconds > 600)
                throw new ArgumentException("maxWaitToRetryInSeconds must be between 10 and 600", nameof(MaxWaitToRetryInSeconds));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            for (var retryCount = 0; retryCount < MaxRetries + 1; retryCount++)
            {
                if (retryCount!=0)
                    await BackOff(retryCount); //start backing off after the first try

                _lastException = null;

                try
                {
                    response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false); //next in the pipeline
                }
                catch (TaskCanceledException taskCanceledException) //Task times out before HttpClient 
                {
                    if (taskCanceledException.CancellationToken != cancellationToken)
                        _lastException = new HttpRequestException("Request timed out", taskCanceledException); //not cancelled by the caller
                    else
                        throw;
                }
                catch (HttpRequestException httpRequestException)
                {
                    var handled = HandleTransientExceptionDotNetCore21AndAboveWindowsOnly(httpRequestException);

                    if (!handled)
                        handled = HandleTransientExceptionDotNetStandardPreCore21WindowsOnly(httpRequestException);

                    if (!handled)
                        handled = HandleTransientExceptionEveryThingElse(httpRequestException);

                    if (!handled)
                        throw;    
                }

                if (!IsServerErrorStatusCode(response?.StatusCode) && _lastException == null)
                {
                    return response;
                }

            }

            if (_lastException != null)
                throw _lastException;

            return response;
        }

        private async Task BackOff(int retryCount)
        {
            if (CurrentBackOffStrategy == BackOffStrategy.Linear)
                await BackOffFunc(WaitToRetryInSeconds).ConfigureAwait(false);

            if (CurrentBackOffStrategy == BackOffStrategy.Exponential)
            {
                var exponentiallyRetryInSeconds = Math.Pow(WaitToRetryInSeconds, retryCount);
                exponentiallyRetryInSeconds = exponentiallyRetryInSeconds > MaxWaitToRetryInSeconds ? MaxWaitToRetryInSeconds : exponentiallyRetryInSeconds;

                await BackOffFunc(exponentiallyRetryInSeconds).ConfigureAwait(false);
            }
        }

        private bool HandleTransientExceptionDotNetCore21AndAboveWindowsOnly(Exception exception)
        {
            /* .NET Core 2.1 and above uses SocketsHttpHandler by default which means yet another set of low level exceptions to handle  https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-core-2-1 */

            if (!(exception?.InnerException is Win32Exception win32Exception))
                return false;

            // ReSharper disable once SwitchStatementMissingSomeCases this is done by design
            switch (win32Exception.NativeErrorCode) // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socketexception?view=netframework-4.7.2
            {
                case (int)WinWSANativeErrorCode.WSA_OPERATION_ABORTED:
                case (int)WinWSANativeErrorCode.WSAEWOULDBLOCK:
                case (int)WinWSANativeErrorCode.WSAENETDOWN:
                case (int)WinWSANativeErrorCode.WSAENETUNREACH:
                case (int)WinWSANativeErrorCode.WSAENETRESET:
                case (int)WinWSANativeErrorCode.WSAECONNABORTED:
                case (int)WinWSANativeErrorCode.WSAECONNRESET:
                case (int)WinWSANativeErrorCode.WSAENOBUFS:
                case (int)WinWSANativeErrorCode.WSAETIMEDOUT:
                case (int)WinWSANativeErrorCode.WSAECONNREFUSED:
                case (int)WinWSANativeErrorCode.WSAEHOSTDOWN:
                case (int)WinWSANativeErrorCode.WSAEHOSTUNREACH:
                case (int)WinWSANativeErrorCode.WSAHOST_NOT_FOUND:
                case (int)WinWSANativeErrorCode.WSATRY_AGAIN:
                case (int)WinWSANativeErrorCode.WSANO_DATA:
                    _lastException = exception;
                    _lastException.Data.Add("IsTransient", true);
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleTransientExceptionDotNetStandardPreCore21WindowsOnly(Exception exception)
        {
            /* The .NET Standard Windows platform exception handling is a bit basic https://github.com/dotnet/corefx/blob/master/src/Common/src/System/Net/Http/WinHttpException.cs
             * So for .NET Standard Windows only I'm having to check the WinHttp Status const https://msdn.microsoft.com/en-us/library/windows/desktop/aa383770(v=vs.85).aspx 
             * Issue raised here https://github.com/dotnet/corefx/issues/19185 */

            if (exception?.InnerException is SocketException)
                return false;

            if (!(exception?.InnerException is Win32Exception win32Exception))
                return false;

            // ReSharper disable once SwitchStatementMissingSomeCases this is done by design
            switch (win32Exception.NativeErrorCode) //https://msdn.microsoft.com/en-us/library/windows/desktop/aa383770(v=vs.85).aspx
            {
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_AUTO_PROXY_SERVICE_ERROR:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_CANNOT_CALL_AFTER_OPEN:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_CANNOT_CALL_AFTER_SEND:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_CANNOT_CALL_BEFORE_OPEN:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_CANNOT_CALL_BEFORE_SEND:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_CANNOT_CONNECT:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_CONNECTION_ERROR:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_NAME_NOT_RESOLVED:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_OPERATION_CANCELLED:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_RESEND_REQUEST:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_SHUTDOWN:
                case (int)WinHttpNativeErrorCode.ERROR_WINHTTP_TIMEOUT:
                    _lastException = exception;
                    _lastException.Data.Add("IsTransient", true);
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleTransientExceptionEveryThingElse(Exception exception)
        {
            if (!(exception?.InnerException is WebException webException))
                return false;

            //https://msdn.microsoft.com/en-us/library/es54hw8e(v=vs.110).aspx https://msdn.microsoft.com/en-us/library/ms346609(v=vs.110).aspx
            // ReSharper disable once SwitchStatementMissingSomeCases this is done by design
            switch (webException.Status)
            {
                case WebExceptionStatus.SendFailure:
                case WebExceptionStatus.ReceiveFailure:
                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.NameResolutionFailure:
                case WebExceptionStatus.RequestCanceled:
                case WebExceptionStatus.ConnectionClosed:
                case WebExceptionStatus.ProxyNameResolutionFailure:
                case WebExceptionStatus.KeepAliveFailure:
                case WebExceptionStatus.Timeout:
                case WebExceptionStatus.Pending:
                    _lastException = exception;
                    _lastException.Data.Add("IsTransient", true);
                    return true;
                default:
                    return false;
            }  
        }
        
        private static bool IsServerErrorStatusCode(HttpStatusCode? statusCode)
        {
            return statusCode == null || (int)statusCode >= 500;
        }

        internal enum WinHttpNativeErrorCode
        {
            // ReSharper disable InconsistentNaming https://msdn.microsoft.com/en-us/library/windows/desktop/aa383770(v=vs.85).aspx
            // ReSharper disable IdentifierTypo
            ERROR_WINHTTP_AUTO_PROXY_SERVICE_ERROR = 12178,
            ERROR_WINHTTP_CANNOT_CALL_AFTER_OPEN = 12103,
            ERROR_WINHTTP_CANNOT_CALL_AFTER_SEND = 12102,
            ERROR_WINHTTP_CANNOT_CALL_BEFORE_OPEN = 12100,
            ERROR_WINHTTP_CANNOT_CALL_BEFORE_SEND = 12101,
            ERROR_WINHTTP_CANNOT_CONNECT = 12029,
            ERROR_WINHTTP_CONNECTION_ERROR = 12030,
            ERROR_WINHTTP_NAME_NOT_RESOLVED = 12007,
            ERROR_WINHTTP_OPERATION_CANCELLED = 12017,
            ERROR_WINHTTP_RESEND_REQUEST = 12032,
            ERROR_WINHTTP_SHUTDOWN = 12012,
            ERROR_WINHTTP_TIMEOUT = 12002
            // ReSharper restore IdentifierTypo
            // ReSharper restore InconsistentNaming
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        internal enum WinWSANativeErrorCode // Guessing here documentation is pretty poor on this low level stuff // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socketexception?view=netframework-4.7.2
        {
            WSA_OPERATION_ABORTED = 995,
            WSAEWOULDBLOCK = 10035,
            WSAENETDOWN = 10050,
            WSAENETUNREACH = 10051,
            WSAENETRESET = 10052,
            WSAECONNABORTED = 10053,
            WSAECONNRESET = 10054,
            WSAENOBUFS = 10055,
            WSAETIMEDOUT = 10060,
            WSAECONNREFUSED = 10061,
            WSAEHOSTDOWN = 10064,
            WSAEHOSTUNREACH = 10065,
            WSAHOST_NOT_FOUND = 11001,
            WSATRY_AGAIN = 11002,
            WSANO_DATA = 11004,
        }
        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming

        public enum BackOffStrategy
        {
            Exponential = 1,
            Linear = 2
        }
    }
}
