using System.Text.Json;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Odin.System;

namespace Tests.Odin.System
{
    [TestFixture]
    public sealed class ResultExTests
    {
        [Test]
        public void Success()
        {
            ResultEx sut = ResultEx.Success();

            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.MessagesToString(), Is.Empty);
            Assert.That(sut.Messages, Is.Empty);
        }

        [Test]
        [TestCase("Reason", true,false)]
        [TestCase("Reason", false,false)]
        [TestCase(null, true, false)]
        [TestCase(null, false, true)]
        [TestCase(" ", true, false)]
        [TestCase("", true, false)]
        [TestCase(" ", false, true)]
        [TestCase("", false, true)]
        public void Failure_requires_a_non_whitespace_message_or_an_error(string? message, bool passAnException, bool shouldThrow)
        {
            Exception? exception = passAnException ? new Exception("ABC") : null;

            if (shouldThrow)
            {
                Assert.Throws<ArgumentException>(() => ResultEx.Failure(message,LogLevel.Error,exception));
            }
            else
            {
                ResultEx sut = ResultEx.Failure(message,LogLevel.Error,exception);
                Assert.That(sut.IsSuccess, Is.False);
                Assert.That(sut.Messages.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void Succeed_with_message()
        {
            ResultEx sut = ResultEx.Success("lovely");

            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.Messages[0].Message, Is.EqualTo("lovely"));
            Assert.That(sut.MessagesToString(), Is.EqualTo("Information: lovely"));
            Assert.That(1, Is.EqualTo(sut.Messages.Count));
        }

        [Test]
        [TestCase("message",true)]
        [TestCase("message",false)]
        [TestCase("",false)]
        [TestCase(null,false)]
        public void Constructor_with_success_and_failure(string? message, bool isSuccess)
        {
            ResultEx sut = new ResultEx(isSuccess, new MessageEx() { Message = message! });

            Assert.That(sut.IsSuccess, Is.EqualTo(isSuccess));
        }

        [Test]
        public void Default_result_is_a_failure()
        {
            ResultEx sut = new ResultEx();

            Assert.That(sut.IsSuccess, Is.False);
            Assert.That(sut.Messages, Is.Empty);
        }

        [Test]
        public void ResultEx_serialises_with_system_dot_text_dot_json()
        {
            ResultEx sut = ResultEx.Success("cool man",LogLevel.Trace);

            string result = JsonSerializer.Serialize(sut);

            Assert.That(result, Contains.Substring("cool man"));
        }

        [Test]
        public void ResultEx_deserialises_with_system_dot_text_dot_json([Values] LogLevel severity)
        {
            string level = ((short) severity).ToString();
            string serialised = """{"IsSuccess":true,"Messages":[{"Message":"cool man","Severity":""" + level + ""","Error":null}]}""";

            ResultEx result = JsonSerializer.Deserialize<ResultEx>(serialised)!;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Messages[0].Message, Is.EqualTo("cool man"));
            Assert.That(result.Messages[0].Severity, Is.EqualTo(severity));
            Assert.That(result.Messages[0].Error, Is.Null);
        }
        
        [Test]
        public void Combine_with_success_and_failures()
        {
            ResultEx r1 = ResultEx.Success();
            ResultEx r2 = ResultEx.Failure("r2",LogLevel.Critical);
            ResultEx r3 = ResultEx.Failure("r3",LogLevel.Warning);
            
            ResultEx sut = ResultEx.Combine(r1, r2, r3);

            Assert.That(sut.IsSuccess, Is.False);
            // First failure is returned...
            Assert.That(sut.Messages.Count, Is.EqualTo(1));
            Assert.That(sut.Messages[0].Message, Is.EqualTo("r2"));
            Assert.That(sut.Messages[0].Severity, Is.EqualTo(LogLevel.Critical));
        }

        [Test]
        public void Combine_with_only_success()
        {
            ResultEx r1 = ResultEx.Success("r1");
            ResultEx r2 = ResultEx.Success("r2");
            ResultEx r3 = ResultEx.Success("r3");
            
            ResultEx sut = ResultEx.Combine(r1, r2, r3);

            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.Messages, Is.Empty);
        }

        
    }
}