using System.Text.Json;
using NUnit.Framework;
using Odin.System;

namespace Tests.Odin.System
{
    [TestFixture]
    public sealed class ResultTests
    {
        [Test]
        public void Success()
        {
            Result sut = Result.Success();

            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.MessagesToString(), Is.Empty);
            Assert.That(sut.Messages, Is.Empty);
        }

        [Test]
        [TestCase("Reason", true)]
        [TestCase(null, false)]
        [TestCase(" ", true)]
        [TestCase("", true)]
        public void Failure(string? message, bool messageExpected)
        {
            Result sut = Result.Failure(message);

            Assert.That(sut.IsSuccess, Is.False);
            if (messageExpected)
            {
                Assert.That(sut.MessagesToString(), Is.EqualTo(message));
                Assert.That(sut.Messages[0], Is.EqualTo(message));
                Assert.That(sut.Messages.Count, Is.EqualTo(1));
            }
            else
            {
                Assert.That(sut.MessagesToString(), Is.EqualTo(string.Empty));
                Assert.That(sut.Messages.Count, Is.EqualTo(0));
            }
                
        }

        [Test]
        public void Succeed_with_message()
        {
            Result sut = Result.Success("lovely");

            Assert.That(sut.IsSuccess, Is.True);
            Assert.That("lovely", Is.EqualTo(sut.MessagesToString()));
            Assert.That("lovely", Is.EqualTo(sut.Messages[0]));
            Assert.That(1, Is.EqualTo(sut.Messages.Count));
        }

        [Test]
        [TestCase("message",true)]
        [TestCase("message",false)]
        [TestCase("",false)]
        [TestCase(null,false)]
        public void Constructor_with_success_and_failure(string?  message, bool isSuccess)
        {
            Result sut = new Result(isSuccess, message);

            Assert.That(sut.IsSuccess, Is.EqualTo(isSuccess));
        }

        [Test]
        public void Default_result_is_a_failure()
        {
            Result sut = new Result();

            Assert.That(sut.IsSuccess, Is.False);
            Assert.That(sut.Messages, Is.Empty);
        }

        [Test]
        public void Result_serialises_with_system_dot_text_dot_json()
        {
            Result sut = Result.Success("cool man");

            string result = JsonSerializer.Serialize(sut);

            Assert.That(result, Is.EqualTo("""{"IsSuccess":true,"Messages":["cool man"]}"""));
        }

        [Test]
        public void Result_deserialises_with_system_dot_text_dot_json()
        {
            string serialised = """{"IsSuccess":true,"Messages":["cool man"]}""";

            Result result = JsonSerializer.Deserialize<Result>(serialised)!;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Messages[0], Is.EqualTo("cool man"));
        }
        
        [Test]
        public void Combine_with_success_and_failures()
        {
            Result r1 = Result.Success();
            Result r2 = Result.Failure("r2");
            Result r3 = Result.Failure("r3");
            
            Result sut = Result.Combine(r1, r2, r3);

            Assert.That(sut.IsSuccess, Is.False);
            // First failure is returned...
            Assert.That(sut.Messages[0], Is.EqualTo("r2"));
        }

        [Test]
        public void Combine_with_only_success()
        {
            Result r1 = Result.Success("r1");
            Result r2 = Result.Success("r2");
            Result r3 = Result.Success("r3");
            
            Result sut = Result.Combine(r1, r2, r3);

            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.Messages, Is.Empty);
        }

        
    }
}