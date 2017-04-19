using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using NUnit.Framework;
using System.Reflection;

namespace DalSoft.RestClient.Test.Unit.Handlers
{
    [TestFixture]
    public class MultipartFormDataHandlerTests
    {

        private const string MultipartFormDataContentType = "multipart/form-data";
        private static readonly Dictionary<string, string> MultipartFormDataHeader = new Dictionary<string, string> { { "Content-Type", MultipartFormDataContentType } };

        [Test]
        public async Task Send_DoNotPassFormUrlEncodedContentTypeHeader_HandlerDoesNotSetContentAsExpected()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new MultipartFormDataHandler(), new UnitTestHandler(request => actualRequest = request)) { UseDefaultHandlers = false }
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), null, new { hello = "world" });

            Assert.That(actualRequest.Content, Is.Null);
            Assert.That(actualRequest.Headers, Is.Empty);
        }

        [Test]
        public async Task Send_NullContent_HandlerDoesNotSetContentAsExpected()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new MultipartFormDataHandler(), new UnitTestHandler(request => actualRequest = request)) { UseDefaultHandlers = false }
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeader, null);

            Assert.That(actualRequest.Content, Is.Null);
            Assert.That(actualRequest.Headers, Is.Empty);
        }

        [Test]
        public async Task Send_SuppliedMultipartFormDataAndBoundry_ExtractsMultipartFormDataAndBoundaryCorrectly()
        {
            // Reference https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Disposition http://chxo.com/be2/20050724_93bf.html
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new MultipartFormDataHandler(), new UnitTestHandler(request => actualRequest = request)) { }
            );

            const string boundary = "MyBoundary";

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeaderWithBoundary(boundary), new {  });
            
            Assert.That(actualRequest.Content.Headers.ContentType.ToString(), Is.EqualTo($"{MultipartFormDataContentType}; boundary=\"{boundary}\""));
        }


        [Test]
        public async Task Send_SuppliedMultipartFormDataAndNoBoundry_ExtractsMultipartFormDataCorrectlyAndCreateGuidBoundry()
        {
            // Reference https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Disposition http://chxo.com/be2/20050724_93bf.html
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config(new MultipartFormDataHandler(), new UnitTestHandler(request => actualRequest = request)) { }
            );

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeader, new { });

            Assert.That(actualRequest.Content.Headers.ContentType.MediaType, Is.EqualTo(MultipartFormDataContentType));

            var boundary = actualRequest.Content.Headers.ContentType.Parameters.FirstOrDefault()?.Value.Replace("\"", string.Empty);
            Guid guid;
            var isGuid = Guid.TryParse(boundary, out guid);

            Assert.IsTrue(isGuid);
        }

        [Test]
        public async Task Send_PropertyWithStream_AddsStreamContentCorrectly()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                new Config()
                    .UseMultipartFormDataHandler()
                    .UseUnitTestHandler(request => actualRequest = request)
            );

            const string boundary = "MyBoundary";
            Stream stream = new FileStream(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location) + "/DalSoft.jpg", FileMode.Open, FileAccess.Read);

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeaderWithBoundary(boundary), new
            {
                myStream = stream,
            });

            var requestBody = await actualRequest.Content.ReadAsStringAsync();
            Assert.IsInstanceOf<StreamContent>(actualRequest.Content);
            Assert.That(actualRequest.Content.Headers.ContentLength, Is.GreaterThan(9274));
            Assert.That(requestBody, Does.Contain(boundary));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=myStream"));
        }

        [Test]
        public async Task Send_PropertyWithBytes_AddsStreamContentCorrectly()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                 new Config()
                    .UseMultipartFormDataHandler()
                    .UseUnitTestHandler(request => actualRequest = request)
            );

            const string boundary = "MyBoundary";
            
            var bytes = File.ReadAllBytes(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location) + "/DalSoft.jpg");
            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeaderWithBoundary(boundary), new
            {
                myBytes = bytes,
            });

            var requestBody = await actualRequest.Content.ReadAsStringAsync();
            Assert.IsInstanceOf<StreamContent>(actualRequest.Content);
            Assert.That(actualRequest.Content.Headers.ContentLength, Is.GreaterThan(9274));
            Assert.That(requestBody, Does.Contain(boundary));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=myBytes"));
        }

        [Test]
        public async Task Send_MultipleFiles_AddsStreamContentCorrectly()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                 new Config()
                    .UseMultipartFormDataHandler()
                    .UseUnitTestHandler(request => actualRequest = request)
            );

            const string boundary = "MyBoundary";

            Stream stream = new FileStream(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location) + "/DalSoft.jpg", FileMode.Open, FileAccess.Read);
            var fileBytes = File.ReadAllBytes(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location) + "/DalSoft.jpg");
            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeaderWithBoundary(boundary), new
            {
                myStream = stream,
                myBytes = fileBytes
            });

            var requestBody = await actualRequest.Content.ReadAsStringAsync();
            Assert.IsInstanceOf<StreamContent>(actualRequest.Content);
            Assert.That(actualRequest.Content.Headers.ContentLength, Is.GreaterThan(18548));
            Assert.That(requestBody, Does.Contain(boundary));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=myStream"));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=myBytes"));
        }

        [Test]
        public async Task Send_FileAndFormData_AddsStreamContentCorrectly()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                 new Config()
                    .UseMultipartFormDataHandler()
                    .UseUnitTestHandler(request => actualRequest = request)
            );

            Stream stream = new FileStream(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location) + "/DalSoft.jpg", FileMode.Open, FileAccess.Read);
            
            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeader, new
            {
                myStream = stream,
                myFormData = "Hello this is simple form data"
            });

            var requestBody = await actualRequest.Content.ReadAsStringAsync();
            Assert.IsInstanceOf<StreamContent>(actualRequest.Content);
            
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=myStream"));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=myFormData"));
            Assert.That(requestBody, Does.Contain("Hello this is simple form data"));
        }

        [Test]
        public async Task Send_FileAndFormDataAndFileName_AddsStreamContentCorrectly()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                 new Config()
                    .UseMultipartFormDataHandler()
                    .UseUnitTestHandler(request => actualRequest = request)
            );

            Stream stream = new FileStream(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location) + "/DalSoft.jpg", FileMode.Open, FileAccess.Read);

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeader, new
            {
                myStream = stream,
                myFormData = "Hello this is simple form data",
                FileName = "dalsoft.jpg"
            });

            var requestBody = await actualRequest.Content.ReadAsStringAsync();

            Assert.IsInstanceOf<StreamContent>(actualRequest.Content);
            Assert.That(requestBody, Does.Contain("filename=dalsoft.jpg;"));
        }

        [Test]
        public async Task Send_NestedFileAndFormData_AddsStreamContentCorrectly()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                 new Config()
                    .UseMultipartFormDataHandler()
                    .UseUnitTestHandler(request => actualRequest = request)
            );

            Stream stream = new FileStream(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location) + "/DalSoft.jpg", FileMode.Open, FileAccess.Read);

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeader, new
            {
                nested = new
                {
                    myStream = stream,
                    myFormData = "Hello this is simple form data"
                }
            });

            var requestBody = await actualRequest.Content.ReadAsStringAsync();
            Assert.IsInstanceOf<StreamContent>(actualRequest.Content);

            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=nested.myStream"));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=nested.myFormData"));
            Assert.That(requestBody, Does.Contain("Hello this is simple form data"));
        }

        [Test]
        public async Task Send_SimpleArrayFileAndFormData_AddsStreamContentCorrectly()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                 new Config()
                    .UseMultipartFormDataHandler()
                    .UseUnitTestHandler(request => actualRequest = request)
            );

            var fileByte = File.ReadAllBytes(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location) + "/DalSoft.jpg");

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeader, new
            {
                fileBytes = new[] { fileByte, fileByte },
                myFormData = new [] { "my form data 1", "my form data 2" }
                    
            });

            var requestBody = await actualRequest.Content.ReadAsStringAsync();
            Assert.IsInstanceOf<StreamContent>(actualRequest.Content);

            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=fileBytes"));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=myFormData"));
            Assert.That(requestBody, Does.Contain("my form data 1"));
            Assert.That(requestBody, Does.Contain("my form data 2"));
        }

        [Test]
        public async Task Send_ComplexArrayFileAndFormData_AddsStreamContentCorrectly()
        {
            HttpRequestMessage actualRequest = null;
            var httpClientWrapper = new HttpClientWrapper
            (
                 new Config()
                    .UseMultipartFormDataHandler()
                    .UseUnitTestHandler(request => actualRequest = request)
            );

            var fileByte = File.ReadAllBytes(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location) + "/DalSoft.jpg");
            Stream stream = new FileStream(Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location) + "/DalSoft.jpg", FileMode.Open, FileAccess.Read);

            await httpClientWrapper.Send(HttpMethod.Post, new Uri("http://test.test"), MultipartFormDataHeader, new
            {
                files = new[]
                {
                    new
                    {
                        fileStream = stream,
                        FileName = "dalsoft1.jpg",
                        myFormData = "my form data 1",
                        fileBytes = new[] { fileByte, fileByte },
                        myFormDataArray = new[] { "my form data array 0 0", "my form data array 0 1" }
                    },
                    new
                    {
                        fileStream = stream,
                        FileName = "dalsoft2.jpg",
                        myFormData = "my form data 2",
                        fileBytes = new[] { fileByte, fileByte },
                        myFormDataArray = new[] { "my form data array 1 0", "my form data array 1 1" }
                    }, 
                },
                nested = new
                {
                    files = new[]
                        {
                        new
                        {
                            fileStream = stream,
                            FileName = "nested.dalsoft1.jpg",
                            myFormData = "nested my form data 1",
                            fileBytes = new[] { fileByte, fileByte },
                            myFormDataArray = new[] { "nested my form data array 0 0", "nested my form data array 0 1" }
                        },
                        new
                        {
                            fileStream = stream,
                            FileName = "nested.dalsoft2.jpg",
                            myFormData = "nested my form data 2",
                            fileBytes = new[] { fileByte, fileByte },
                            myFormDataArray = new[] { "nested my form data array 1 0", "nested my form data array 1 1" }
                        },
                    }
                }
            });

            var requestBody = await actualRequest.Content.ReadAsStringAsync();
            Assert.IsInstanceOf<StreamContent>(actualRequest.Content);

            //Bug simple Array nested in complex array loops properties rather the array value
            //Bug Filename in complex array
                
            //BUG Assert.That(requestBody, Does.Contain("filename=dalsoft1.jpg;"));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"files[0].fileStream\""));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"files[0].myFormData\""));
            //BUG Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"files[0].fileBytes\""));
            //BUG Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"files[0].myFormDataArray\""));
            Assert.That(requestBody, Does.Contain("my form data 1"));
            Assert.That(requestBody, Does.Contain("my form data array 0 0"));
            Assert.That(requestBody, Does.Contain("my form data array 0 1"));

            //BUG Assert.That(requestBody, Does.Contain("filename=dalsoft2.jpg;"));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"files[1].fileStream\""));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"files[1].myFormData\""));
            //BUG Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"files[1].fileBytes\""));
            //BUG Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"files[1].myFormDataArray\""));
            Assert.That(requestBody, Does.Contain("my form data 2"));
            Assert.That(requestBody, Does.Contain("my form data array 1 0"));
            Assert.That(requestBody, Does.Contain("my form data array 1 1"));

            //BUG Assert.That(requestBody, Does.Contain("nested.filename=dalsoft.jpg;"));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"nested.files[1].fileStream\""));
            Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"nested.files[1].myFormData\""));
            //BUG Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"nested.files[1].fileBytes\""));
            //BUG Assert.That(requestBody, Does.Contain("Content-Disposition: form-data; name=\"nested.files[1].myFormDataArray\""));
            Assert.That(requestBody, Does.Contain("nested my form data 2"));
            Assert.That(requestBody, Does.Contain("nested my form data array 1 0"));
            Assert.That(requestBody, Does.Contain("nested my form data array 1 1"));
        }

        private Dictionary<string, string> MultipartFormDataHeaderWithBoundary(string boundary)
        {
           return new Dictionary<string, string>
           {
               { "Content-Type", $"{MultipartFormDataContentType};boundary=\"{boundary}\"" }
           };
        }
    }
}
