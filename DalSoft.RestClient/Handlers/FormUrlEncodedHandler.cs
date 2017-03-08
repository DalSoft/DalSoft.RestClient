using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DalSoft.RestClient.Extensions;

namespace DalSoft.RestClient.Handlers
{
    public class FormUrlEncodedHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (IsFormUrlEncodedContentType(request))
            {
                var content = request.GetContent();
                request.Content = content == null ? null : new FormUrlEncodedContent
                (
                    content.FlattenObjectToKeyValuePairs<string>(includeThisType:Object.IsValueTypeOrPrimitiveOrStringOrGuidOrDateTime)
                );
            }

            return await base.SendAsync(request, cancellationToken); //next in the pipeline
        }

        private static bool IsFormUrlEncodedContentType(HttpRequestMessage request)
        {
            return request.GetContentType() == "application/x-www-form-urlencoded";
        }
    }
}
