using System.Text.Json;
using NUnit.Framework;
using Odin.System;

namespace Tests.Odin.System
{
    [TestFixture]
    public sealed class Result22Tests
    {
        [Test]
        public void Success()
        {
            Result2 sut = Result2.Success();

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
            Result2 sut = Result2.Failure(message);

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
            Result2 sut = Result2.Success("lovely");

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
            Result2 sut = new Result2(isSuccess, new ResultMessage2() { Message = message! });

            Assert.That(sut.IsSuccess, Is.EqualTo(isSuccess));
        }

        [Test]
        public void Default_result_is_a_failure()
        {
            Result2 sut = new Result2();

            Assert.That(sut.IsSuccess, Is.False);
            Assert.That(sut.Messages, Is.Empty);
        }

        [Test]
        public void Result2_serialises_with_system_dot_text_dot_json()
        {
            Result2 sut = Result2.Success("cool man");

            string result = JsonSerializer.Serialize(sut);

            Assert.That(result, Contains.Substring("cool man"));
        }

        [Test]
        public void Result2_deserialises_with_system_dot_text_dot_json()
        {
            string serialised = "{\"IsSuccess\":true,\"Messages\":[ \"cool man\" ]}";

            Result2 result = JsonSerializer.Deserialize<Result2>(serialised)!;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Messages[0], Is.EqualTo("cool man"));
        }
        
        [Test]
        public void Combine_with_success_and_failures()
        {
            Result2 r1 = Result2.Success();
            Result2 r2 = Result2.Failure("r2");
            Result2 r3 = Result2.Failure("r3");
            
            Result2 sut = Result2.Combine(r1, r2, r3);

            Assert.That(sut.IsSuccess, Is.False);
            // First failure is returned...
            Assert.That(sut.Messages[0], Is.EqualTo("r2"));
        }

        [Test]
        public void Combine_with_only_success()
        {
            Result2 r1 = Result2.Success("r1");
            Result2 r2 = Result2.Success("r2");
            Result2 r3 = Result2.Success("r3");
            
            Result2 sut = Result2.Combine(r1, r2, r3);

            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.Messages, Is.Empty);
        }

        
    }
}