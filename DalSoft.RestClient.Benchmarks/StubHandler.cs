using System.Net;

namespace DalSoft.RestClient.Benchmarks
{
    //Returns a canned payload without touching the network or cloning content, so we benchmark the library not the test infrastructure
    public class StubHandler : DelegatingHandler
    {
        private readonly byte[] _payload;

        public StubHandler(byte[] payload)
        {
            _payload = payload;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(_payload),
                RequestMessage = request
            });
        }
    }
}
