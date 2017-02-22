using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DalSoft.RestClient.Handlers;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.Handlers
{
    [TestFixture]
    public class UnitTestHandlerTests
    {
        [Test]
        public async Task Ctor_HttpResponseMessagePassed_HttpResponseMessageReturnAndPipelineEnds()
        {
            dynamic restClient = new RestClient("http://headers.jsontest.com/", new Config
            (
                new UnitTestHandler 
                (
                    request => new HttpResponseMessage(HttpStatusCode.Created)
                ),
                new UnitTestHandler
                (
                    request => new HttpResponseMessage(HttpStatusCode.InternalServerError)
                ))
            );

            HttpResponseMessage result = await restClient.Get();

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }
    }
}
