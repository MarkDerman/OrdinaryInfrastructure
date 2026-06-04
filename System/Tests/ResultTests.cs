using Odin.System;
using System.Text.Json;

namespace Tests.Odin.System
{
    public sealed class ResultTests
    {
        [Test]
        public void Simple_Success()
        {
            Result sut = Result.Success();

            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.MessagesToString(), Is.EqualTo(string.Empty));
            Assert.That(sut.Messages, Is.Empty);
        }
        [TestCase("Reason", false)]
        [TestCase(null, true)]
        [TestCase(" ", true)]
        [TestCase("", true)]
        public void Failure_requires_a_non_whitespace_message(string? message, bool shouldThrow)
        {
            if (shouldThrow)
            {
                ArgumentException error = Assert.Catch<ArgumentException>(() => Result.Failure(message!));
                Assert.That(error, Is.Not.Null);
            }
            else
            {
                Result sut = Result.Failure(message!);
                Assert.That(sut.IsSuccess, Is.False);
                Assert.That(sut.MessagesToString(), Is.EqualTo(message));
                Assert.That(sut.Messages[0], Is.EqualTo(message));
                sut.Messages.Single();
            }
        }

        [Test]
        public void Succeed_with_message()
        {
            Result sut = Result.Success("lovely");

            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.MessagesToString(), Is.EqualTo("lovely"));
            Assert.That(sut.Messages[0], Is.EqualTo("lovely"));
            sut.Messages.Single();
        }
        [TestCase("message", true)]
        [TestCase("message", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void Constructor_with_success_and_failure(string? message, bool isSuccess)
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
        [TestCaseSource(nameof(ResultCaseData))]
        public void Result_serialises(bool success, string message, string serializer)
        {
            if (!success && string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            Result sut = new Result(success, message);
            string result;
            if (serializer == "Newton")
            {
                result = Newtonsoft.Json.JsonConvert.SerializeObject(sut);
            }
            else
            {
                result = JsonSerializer.Serialize(sut);
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(ResultJsonFor(success, message)));
        }
        [TestCaseSource(nameof(ResultCaseData))]
        public void Result_deserialises(bool success, string message, string serializer)
        {
            if (!success && string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            string json = ResultJsonFor(success, message);
            Result sut;
            if (serializer == "Newton")
            {
                sut = Newtonsoft.Json.JsonConvert.DeserializeObject<Result>(json)!;
            }
            else
            {
                sut = JsonSerializer.Deserialize<Result>(json)!;
            }

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.IsSuccess, Is.EqualTo(success));
            Assert.That(sut.Messages[0], Is.EqualTo(message));
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

        internal string ResultJsonFor(bool isSuccess, string? message)
        {
            return $"{{\"IsSuccess\":{isSuccess.ToString().ToLower()},\"Messages\":[\"{message}\"]}}";
        }

        public static IEnumerable<object?[]> ResultCaseData()
        {
            bool[] successValues = [true, false];
            string?[] messageValues = ["ABC", "", "  "];
            string[] serializers = ["Newton", "System"];

            foreach (bool success in successValues)
            {
                foreach (string? message in messageValues)
                {
                    foreach (string serializer in serializers)
                    {
                        yield return [success, message, serializer];
                    }
                }
            }
        }
    }
}