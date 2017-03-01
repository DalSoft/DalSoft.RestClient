//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Threading;
//using System.Threading.Tasks;

//namespace DalSoft.RestClient.Handlers
//{
//    internal class MultipartFormDataHandler : DelegatingHandler
//    {
//        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {
//            if (IsMultipartFormDataHandler(request))
//            {
//                request.Content = GetContent(request);

//            }

//            return await base.SendAsync(request, cancellationToken); //next in the pipeline
//        }

//        private MultipartFormDataContent GetContent(HttpRequestMessage request)
//        {
//            var content = request.GetContent();
//            var contentType = request.GetContentType().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
//            var boundry = contentType.Length > 1 ? contentType[1].Replace("boundary=", string.Empty) : null;

//            var multipartFormDataContent = contentType.Length > 1 ? new MultipartFormDataContent(boundry) : new MultipartFormDataContent();


//            var stringContent = new StringContent(JsonConvert.SerializeObject(request));
//            stringContent.Headers.Add("Content-Disposition", "form-data; name=\"json\"");
//            content.Add(stringContent, "json");

//            FileStream fs = File.OpenRead(path);

//            var streamContent = new StreamContent(fs);
//            streamContent.Headers.Add("Content-Type", "application/octet-stream");
//            //Content-Disposition: form-data; name="file"; filename="C:\B2BAssetRoot\files\596090\596090.1.mp4";
//            streamContent.Headers.Add("Content-Disposition", "form-data; name=\"file\"; filename=\"" + Path.GetFileName(path) + "\"");
//            content.Add(streamContent, "file", Path.GetFileName(path));

//            //content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");


//        }
//    }
//}
