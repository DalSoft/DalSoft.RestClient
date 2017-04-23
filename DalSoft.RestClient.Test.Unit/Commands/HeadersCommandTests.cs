using System;
using System.Collections.Generic;
using DalSoft.RestClient.Commands;
using NUnit.Framework;

namespace DalSoft.RestClient.Test.Unit.Commands
{
    public class HeadersCommandTests
    {
        [Test]
        public void IsCommandFor_MethodIsHeaders_ReturnsTrue()
        {
            var headerCommand = new HeadersCommand();
                
            var result = headerCommand.IsCommandFor("Headers", null);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsCommandFor_MethodIsNotHeaders_ReturnsTrue()
        {
            var headerCommand = new HeadersCommand();

            var result = headerCommand.IsCommandFor("Something", null);

            Assert.False(result);
        }

        [Test]
        public void Execute_HeadersArgumentsIsNull_ThrowsArgumentException()
        {
            var headerCommand = new HeadersCommand();

            var exception = Assert.Throws<ArgumentException>(() => headerCommand.Execute(null, new MemberAccessWrapper(new HttpClientWrapper(), "http://test", "testresource", new Dictionary<string, string>())));

            Assert.That(exception.Message, Is.EqualTo("Headers must have one not null argument only"));
        }

        [Test]
        public void Execute_HeadersArgumentsIsEmpty_ThrowsArgumentException()
        {
            var headerCommand = new HeadersCommand();

            var exception = Assert.Throws<ArgumentException>(() => headerCommand.Execute(new object[] { }, new MemberAccessWrapper(new HttpClientWrapper(), "http://test", "testresource", new Dictionary<string, string>())));

            Assert.That(exception.Message, Is.EqualTo("Headers must have one not null argument only"));
        }

        [Test]
        public void Execute_HeadersHasMoreThanOneArgument_ThrowsArgumentException()
        {
            var headerCommand = new HeadersCommand();

            var exception = Assert.Throws<ArgumentException>(() => headerCommand.Execute(new object[] { 1, 2 }, new MemberAccessWrapper(new HttpClientWrapper(), "http://test", "testresource", new Dictionary<string, string>())));

            Assert.That(exception.Message, Is.EqualTo("Headers must have one not null argument only"));
        }

        [TestCase("Not IDictionary Of String")]
        [TestCase(1)]
        [TestCase(new [] {1})]
        public void Execute_HeadersArgumentIsNotIDictionaryOrObjectRepresentingHeaders_ThrowsArgumentException(object notIDictionaryOrObjectRepresentingHeaders)
        {
            var headerCommand = new HeadersCommand();

            var exception = Assert.Throws<ArgumentException>(() => headerCommand.Execute(new object[] { notIDictionaryOrObjectRepresentingHeaders }, new MemberAccessWrapper(new HttpClientWrapper(), "http://test", "testresource", new Dictionary<string, string>())));

            Assert.That(exception.Message, Is.EqualTo("Headers must be Dictionary<string, string> or a simple object representing the Headers new { ContentType = \"application/json\", Accept = \"application/json\" }"));
        }

        [Test]
        public void Execute_HeadersArgumentObjectRepresentingHeadersIsInvalid_ThrowsArgumentException()
        {
            var headerCommand = new HeadersCommand();
            var invalidObjectRepresentingHeaders = new { Accept = 1 };
            var exception = Assert.Throws<ArgumentException>(() => headerCommand.Execute(new object[] { invalidObjectRepresentingHeaders }, new MemberAccessWrapper(new HttpClientWrapper(), "http://test", "testresource", new Dictionary<string, string>())));

            Assert.That(exception.Message, Is.EqualTo("Headers must be Dictionary<string, string> or a simple object representing the Headers new { ContentType = \"application/json\", Accept = \"application/json\" }"));
        }

        [Test]
        public void Execute_HeadersArgumentDictionary_CorrectAddedToMemberAccessWrapperHeaders()
        {
            var headerCommand = new HeadersCommand();
            var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Accept", "text/html"}
            };

            var result  = (MemberAccessWrapper)headerCommand.Execute(new object[] { headers }, new MemberAccessWrapper(new HttpClientWrapper(), "http://test", "testresource", new Dictionary<string, string>()));

            Assert.That(result.Headers["Content-Type"], Is.EqualTo("application/json"));
            Assert.That(result.Headers["Accept"], Is.EqualTo("text/html"));
        }

        [Test]
        public void Execute_HeadersArgumentObject_CorrectAddedToMemberAccessWrapperHeaders()
        {
            var headerCommand = new HeadersCommand();
            var headers = new
            {
                Accept = "text/html",
                ContentType = "application/json"
            };

            var result = (MemberAccessWrapper)headerCommand.Execute(new object[] { headers }, new MemberAccessWrapper(new HttpClientWrapper(), "http://test", "testresource", new Dictionary<string, string>()));

            Assert.That(result.Headers["Content-Type"], Is.EqualTo("application/json"));
            Assert.That(result.Headers["Accept"], Is.EqualTo("text/html"));
        }

        [Test]
        public void Execute_HeadersArgumentDictionaryAlreadyInMemberAccessWrapperHeaders_ReplacesExistingHeaderInMemberAccessWrapperHeader()
        {
            var headerCommand = new HeadersCommand();
            var existingHeaders = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
            };

            var newHeaders = new Dictionary<string, string>
            {
                {"Content-Type", "text/html"}
            };

            var result = (MemberAccessWrapper)headerCommand.Execute(new object[] { newHeaders }, new MemberAccessWrapper(new HttpClientWrapper(), "http://test", "testresource", existingHeaders));

            Assert.That(result.Headers["Content-Type"], Is.EqualTo("text/html"));
        }
    }
}
