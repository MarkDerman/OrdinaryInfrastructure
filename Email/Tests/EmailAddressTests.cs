using Odin.Email;

namespace Tests.Odin.Email
{
    public sealed class EmailAddressTests
    {
        [Theory]
        [InlineData("a@t.com", "a@t.com", null)]
        [InlineData("  a@t.com", "a@t.com", null)]
        [InlineData("a@t.com  ", "a@t.com", null)]
        [InlineData("<a@t.com>", "a@t.com", null)]
        [InlineData("B<a@t.com>", "a@t.com", "B")]
        [InlineData("     B<a@t.com>", "a@t.com", "B")]
        [InlineData("B     <a@t.com>", "a@t.com", "B")]
        [InlineData("B<   a@t.com>", "a@t.com", "B")]
        [InlineData("B<a@t.com   >", "a@t.com", "B")]
        [InlineData("B<a@t.com   >IGNORED", "a@t.com", "B")]
        public void EmailAddress_construction_from_address_only_string(string testEmailAddress, string expectedAddress,
            string? expectedDisplayName)
        {
            EmailAddress sut = new EmailAddress(testEmailAddress);
         
            Assert.Equal(expectedAddress, sut.Address);
            Assert.Equal(expectedDisplayName, sut.DisplayName);
        }
        
    }
}
