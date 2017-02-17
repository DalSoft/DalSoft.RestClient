using System;
using System.Linq;
using System.Net.Http;
using DalSoft.RestClient.Handlers;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void Ctor_PassingMoreThan1HttpHttpClientHandler_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Config(new HttpClientHandler(), new HttpClientHandler()));
        }

        [Test]
        public void Ctor_Pipeline_DefaultJsonHandlerAlwaysAdded()
        {
            var config = new Config();

            Assert.True(config.Pipeline.OfType<DefaultJsonHandler>().Any());
        }

        [Test]
        public void Ctor_Pipeline_DefaultJsonHandlerAlwaysFirst()
        {
            var config = new Config(new HttpClientHandler(), new UnitTestHandler());

            Assert.IsInstanceOf<DefaultJsonHandler>(config.Pipeline.ElementAt(0));
        }

        [Test]
        public void Ctor_HandlersPassedToPipeline_SetsPipelinePropertyCorrectly()
        {
            var config = new Config(new HttpClientHandler(), new UnitTestHandler());

            Assert.That(config.Pipeline.Count(), Is.EqualTo(3)); //count + DefaultJsonHandler
        }

        [Test]
        public void Ctor_HandlerFuncsPassedToPipeline_SetsPipelinePropertyCorrectly()
        {
            var config = new Config((request, token, next) => next(request, token), (request, token, next) => next(request, token));

            Assert.That(config.Pipeline.Count(), Is.EqualTo(3)); //count + DefaultJsonHandler
        }

        [Test]
        public void Ctor_Timeout_SetToDefault()
        {
            var config = new Config();

            Assert.That(config.Timeout, Is.EqualTo(TimeSpan.FromSeconds(100.0))); //count + DefaultJsonHandler
        }

        [Test]
        public void Ctor_MaxResponseContentBufferSize_SetToDefault()
        {
            var config = new Config();

            Assert.That(config.MaxResponseContentBufferSize, Is.EqualTo(int.MaxValue)); //count + DefaultJsonHandler
        }

        [Test]
        public void Ctor_UseDefaultHandlers_SetToDefault()
        {
            var config = new Config();

            Assert.That(config.UseDefaultHandlers, Is.EqualTo(true)); //count + DefaultJsonHandler
        }
    }
}
