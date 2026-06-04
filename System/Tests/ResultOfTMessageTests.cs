using Odin.System;

namespace Tests.Odin.System
{
    public abstract class ResultOfTMessageTests<TMessage> where TMessage : class
    {

        [Test]
        public void Success()
        {
            Result<TMessage> sut = Result<TMessage>.Success();

            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.Messages, Is.Empty);
        }

        [Test]
        public void Failure_without_TMessage()
        {
            Result<TMessage> sut = Result<TMessage>.Failure((null as TMessage)!);

            Assert.That(sut.IsSuccess, Is.False);
            Assert.That(sut.Messages, Is.Empty);
        }

        [Test]
        public void Default_result_is_a_failure()
        {
            Result<TMessage> sut = new Result<TMessage>();

            Assert.That(sut.IsSuccess, Is.False);
            Assert.That(sut.Messages, Is.Empty);
        }
    }

    public sealed class ResultOfMessageErrorTests : ResultOfTMessageTests<MessageError>;

    public sealed class ResultOfMessageLoggingInfoTests : ResultOfTMessageTests<MessageLoggingInfo>;

    public sealed class ResultOfMessageSeverityTests : ResultOfTMessageTests<MessageSeverity>;
}