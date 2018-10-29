using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DalSoft.RestClient.Extensions;
using Object = DalSoft.RestClient.Extensions.Object;

namespace DalSoft.RestClient.Handlers
{
    public class MultipartFormDataHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (IsMultipartFormDataHandler(request))
            {
                request.Content = GetContent(request);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false); //next in the pipeline
        }

        private bool IsMultipartFormDataHandler(HttpRequestMessage request)
        {
            return request.GetContentType() != null && request.GetContentType().StartsWith("multipart/form-data");
        }

        internal static MultipartFormDataContent GetContent(HttpRequestMessage request)
        {
            var content = request.GetContent();
            if (content == null)
                return null;

            var contentType = request.GetContentType().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var boundry = contentType.Length > 1 ? contentType.SingleOrDefault(_=>_.Contains("boundary="))?.Replace("boundary=", string.Empty).Replace("\"", string.Empty) : null;
            var multipartFormDataContent = contentType.Any(_ => _.Contains("boundary=")) ? new MultipartFormDataContent(boundry) : new MultipartFormDataContent();

            var formData = content.FlattenObjectToKeyValuePairs<object>(includeThisType:Object.IsValueTypeOrPrimitiveOrStringOrGuidOrDateTimeOrByteArrayOrStream);

            foreach (var pairs in formData.GroupBy(_ => _.Key.Split(".".ToCharArray()).Length))
            {
                foreach (var groupedPair in pairs)
                {
                    if (groupedPair.Key.ToLower() == "filename") continue;

                    var bytes = groupedPair.Value as byte[];
                    var stream = groupedPair.Value as Stream;

                    if (bytes != null || stream!=null)
                    {
                        stream = stream ?? new MemoryStream(bytes);
                        var filename = pairs.Where(_ => _.Key.ToLower() == "filename").ToList();
                        
                        if (filename.Any())
                            multipartFormDataContent.Add(new StreamContent(stream), groupedPair.Key, filename.First().Value.ToString());
                        else
                            multipartFormDataContent.Add(new StreamContent(stream), groupedPair.Key);
                    }
                    else
                    {
                        multipartFormDataContent.Add(new StringContent(groupedPair.Value.ToString()), groupedPair.Key);
                    }
                }
            }

            return multipartFormDataContent;
        }
    }
}
