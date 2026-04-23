using Odin.System;

namespace Tests.Odin.System
{
    public abstract class ResultOfTMessageTests<TMessage> where TMessage : class
    {

        [Fact]
        public void Success()
        {
            Result<TMessage> sut = Result<TMessage>.Success();

            Assert.True(sut.IsSuccess);
            Assert.Empty(sut.Messages);
        }
        
        [Fact]
        public void Failure_requires_TMessage()
        {
            Assert.Throws<ArgumentNullException>(() => Result<TMessage>.Failure((null as TMessage)!));
        }

        [Fact]
        public void Default_result_is_a_failure()
        {
            Result<TMessage> sut = new Result<TMessage>();

            Assert.False(sut.IsSuccess);
            Assert.Empty(sut.Messages);
        }
    }

    public sealed class ResultOfMessageErrorTests : ResultOfTMessageTests<MessageError>;

    public sealed class ResultOfMessageLoggingInfoTests : ResultOfTMessageTests<MessageLoggingInfo>;

    public sealed class ResultOfMessageSeverityTests : ResultOfTMessageTests<MessageSeverity>;
}
