using Odin.BackgroundProcessing;

namespace Tests.Odin.BackgroundProcessing
{
    public sealed class JobDetailsTests
    {
        [Test]
        public void Constructor_sets_properties()
        {
            string id = "123";
            DateTimeOffset when = DateTimeOffset.Now;

            JobDetails sut = new JobDetails(id, when);

            Assert.That(sut.JobId, Is.EqualTo(id));
            Assert.That(sut.ScheduledFor, Is.EqualTo(when));
        }
    }
}