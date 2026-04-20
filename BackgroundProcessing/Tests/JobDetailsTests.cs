using Odin.BackgroundProcessing;

namespace Tests.Odin.BackgroundProcessing
{
    public sealed class JobDetailsTests
    {
        [Fact]
        public void Constructor_sets_properties()
        {
            string id = "123";
            DateTimeOffset when = DateTimeOffset.Now;
            
            JobDetails sut = new JobDetails(id,when);
         
            Assert.Equal(id, sut.JobId);
            Assert.Equal(when, sut.ScheduledFor);
        }
    }
}
