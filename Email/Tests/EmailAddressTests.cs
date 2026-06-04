using Odin.Email;

namespace Tests.Odin.Email
{
    public sealed class EmailAddressTests
    {
        [TestCase("a@t.com", "a@t.com", null)]
        [TestCase("  a@t.com", "a@t.com", null)]
        [TestCase("a@t.com  ", "a@t.com", null)]
        [TestCase("<a@t.com>", "a@t.com", null)]
        [TestCase("B<a@t.com>", "a@t.com", "B")]
        [TestCase("     B<a@t.com>", "a@t.com", "B")]
        [TestCase("B     <a@t.com>", "a@t.com", "B")]
        [TestCase("B<   a@t.com>", "a@t.com", "B")]
        [TestCase("B<a@t.com   >", "a@t.com", "B")]
        [TestCase("B<a@t.com   >IGNORED", "a@t.com", "B")]
        public void EmailAddress_construction_from_address_only_string(string testEmailAddress, string expectedAddress,
            string? expectedDisplayName)
        {
            EmailAddress sut = new EmailAddress(testEmailAddress);

            Assert.That(sut.Address, Is.EqualTo(expectedAddress));
            Assert.That(sut.DisplayName, Is.EqualTo(expectedDisplayName));
        }

    }
}