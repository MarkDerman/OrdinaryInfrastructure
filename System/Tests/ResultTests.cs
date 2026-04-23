using System.Text.Json;
using Odin.System;

namespace Tests.Odin.System
{
    public sealed class ResultTests
    {
        [Fact]
        public void Simple_Success()
        {
            Result sut = Result.Success();

            Assert.True(sut.IsSuccess);
            Assert.Equal(string.Empty, sut.MessagesToString());
            Assert.Empty(sut.Messages);
        }

        [Theory]
        [InlineData("Reason", false)]
        [InlineData(null, true)]
        [InlineData(" ", true)]
        [InlineData("", true)]
        public void Failure_requires_a_non_whitespace_message(string? message, bool shouldThrow)
        {
            if (shouldThrow)
            {
                ArgumentException error = Assert.Throws<ArgumentException>(() => Result.Failure(message!));
                Assert.NotNull(error);
            }
            else
            {
                Result sut = Result.Failure(message!);
                Assert.False(sut.IsSuccess);
                Assert.Equal(message, sut.MessagesToString());
                Assert.Equal(message, sut.Messages[0]);
                Assert.Equal(1, sut.Messages.Count);
            }
        }

        [Fact]
        public void Succeed_with_message()
        {
            Result sut = Result.Success("lovely");

            Assert.True(sut.IsSuccess);
            Assert.Equal("lovely", sut.MessagesToString());
            Assert.Equal("lovely", sut.Messages[0]);
            Assert.Equal(1, sut.Messages.Count);
        }

        [Theory]
        [InlineData("message", true)]
        [InlineData("message", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Constructor_with_success_and_failure(string? message, bool isSuccess)
        {
            Result sut = new Result(isSuccess, message);

            Assert.Equal(isSuccess, sut.IsSuccess);
        }

        [Fact]
        public void Default_result_is_a_failure()
        {
            Result sut = new Result();

            Assert.False(sut.IsSuccess);
            Assert.Empty(sut.Messages);
        }

        [Theory]
        [MemberData(nameof(ResultCaseData))]
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

            Assert.NotNull(result);
            Assert.Equal(ResultJsonFor(success, message), result);
        }

        [Theory]
        [MemberData(nameof(ResultCaseData))]
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

            Assert.NotNull(sut);
            Assert.Equal(success, sut.IsSuccess);
            Assert.Equal(message, sut.Messages[0]);
        }

        [Fact]
        public void Combine_with_success_and_failures()
        {
            Result r1 = Result.Success();
            Result r2 = Result.Failure("r2");
            Result r3 = Result.Failure("r3");

            Result sut = Result.Combine(r1, r2, r3);

            Assert.False(sut.IsSuccess);
            // First failure is returned...
            Assert.Equal("r2", sut.Messages[0]);
        }

        [Fact]
        public void Combine_with_only_success()
        {
            Result r1 = Result.Success("r1");
            Result r2 = Result.Success("r2");
            Result r3 = Result.Success("r3");

            Result sut = Result.Combine(r1, r2, r3);

            Assert.True(sut.IsSuccess);
            Assert.Empty(sut.Messages);
        }

        [Fact]
        public void CombineAll_aggregates_all_failure_messages()
        {
            Result r1 = Result.Success();
            Result r2 = Result.Failure("r2");
            Result r3 = Result.Failure("r3");

            Result sut = Result.CombineAll(r1, r2, r3);

            Assert.False(sut.IsSuccess);
            Assert.Equal(["r2", "r3"], sut.Messages);
        }

        [Fact]
        public void Match_returns_the_selected_branch()
        {
            string success = Result.Success().Match(
                onSuccess: () => "ok",
                onFailure: messages => string.Join(",", messages));
            string failure = Result.Failure("bad").Match(
                onSuccess: () => "ok",
                onFailure: messages => string.Join(",", messages));

            Assert.Equal("ok", success);
            Assert.Equal("bad", failure);
        }

        [Fact]
        public void Bind_propagates_failures_without_invoking_next_step()
        {
            bool invoked = false;

            Result sut = Result.Failure("bad").Bind(() =>
            {
                invoked = true;
                return Result.Success();
            });

            Assert.False(invoked);
            Assert.False(sut.IsSuccess);
            Assert.Equal("bad", sut.MessagesToString());
        }

        [Fact]
        public void Tap_runs_only_for_success()
        {
            int count = 0;

            Result.Success().Tap(() => count++);
            Result.Failure("bad").Tap(() => count++);

            Assert.Equal(1, count);
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
