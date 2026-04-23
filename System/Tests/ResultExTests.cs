#pragma warning disable CS0618

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Odin.System;

namespace Tests.Odin.System
{
    public sealed class ResultExTests
    {
        [Fact]
        public void Success()
        {
            ResultEx sut = ResultEx.Success();

            Assert.True(sut.IsSuccess);
            Assert.Equal(string.Empty, sut.MessagesToString());
            Assert.Empty(sut.Messages);
        }

        [Theory]
        [InlineData("Reason", true, false)]
        [InlineData("Reason", false, false)]
        [InlineData(null, true, false)]
        [InlineData(null, false, true)]
        [InlineData(" ", true, false)]
        [InlineData("", true, false)]
        [InlineData(" ", false, true)]
        [InlineData("", false, true)]
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
                Assert.False(sut.IsSuccess);
                Assert.Equal(1, sut.Messages.Count);
            }
        }

        [Fact]
        public void Succeed_with_message()
        {
            ResultEx sut = ResultEx.Success("lovely");

            Assert.True(sut.IsSuccess);
            Assert.Equal("lovely", sut.Messages[0].Message);
            Assert.Equal("Information: lovely", sut.MessagesToString());
            Assert.Equal(1, sut.Messages.Count);
        }

        [Theory]
        [InlineData("message", true)]
        [InlineData("message", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Constructor_with_success_and_failure(string? message, bool isSuccess)
        {
            ResultEx sut = new ResultEx(isSuccess, new MessageEx() { Message = message! });

            Assert.Equal(isSuccess, sut.IsSuccess);
        }

        [Fact]
        public void Default_result_is_a_failure()
        {
            ResultEx sut = new ResultEx();

            Assert.False(sut.IsSuccess);
            Assert.Empty(sut.Messages);
        }

        [Fact]
        public void ResultEx_serialises_with_system_dot_text_dot_json()
        {
            ResultEx sut = ResultEx.Success("cool man",LogLevel.Trace);

            string result = JsonSerializer.Serialize(sut);

            Assert.Contains("cool man", result);
        }

        [Theory]
        [MemberData(nameof(LogLevels))]
        public void ResultEx_deserialises_with_system_dot_text_dot_json(LogLevel severity)
        {
            string level = ((short) severity).ToString();
            string serialised = """{"IsSuccess":true,"Messages":[{"Message":"cool man","Severity":""" + level + ""","Error":null}]}""";

            ResultEx result = JsonSerializer.Deserialize<ResultEx>(serialised)!;

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("cool man", result.Messages[0].Message);
            Assert.Equal(severity, result.Messages[0].Severity);
            Assert.Null(result.Messages[0].Error);
        }
        
        [Fact]
        public void Combine_with_success_and_failures()
        {
            ResultEx r1 = ResultEx.Success();
            ResultEx r2 = ResultEx.Failure("r2",LogLevel.Critical);
            ResultEx r3 = ResultEx.Failure("r3",LogLevel.Warning);
            
            ResultEx sut = ResultEx.Combine(r1, r2, r3);

            Assert.False(sut.IsSuccess);
            // First failure is returned...
            Assert.Equal(1, sut.Messages.Count);
            Assert.Equal("r2", sut.Messages[0].Message);
            Assert.Equal(LogLevel.Critical, sut.Messages[0].Severity);
        }

        [Fact]
        public void Combine_with_only_success()
        {
            ResultEx r1 = ResultEx.Success("r1");
            ResultEx r2 = ResultEx.Success("r2");
            ResultEx r3 = ResultEx.Success("r3");
            
            ResultEx sut = ResultEx.Combine(r1, r2, r3);

            Assert.True(sut.IsSuccess);
            Assert.Empty(sut.Messages);
        }

        public static IEnumerable<object?[]> LogLevels()
        {
            foreach (LogLevel severity in Enum.GetValues<LogLevel>())
            {
                yield return [severity];
            }
        }
        
    }
}

#pragma warning restore CS0618
