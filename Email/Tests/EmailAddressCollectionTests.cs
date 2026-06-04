using Odin.Email;

namespace Tests.Odin.Email
{
    public sealed class EmailAddressCollectionTests
    {
        public static IEnumerable<TestCaseData> ConstructionFromAddressesCases()
        {
            yield return new TestCaseData(
                "Bob <B@a.com> , Geoff <G@y.com>       ;,,",
                new[] { "B@a.com", "G@y.com" });
        }
        [TestCaseSource(nameof(ConstructionFromAddressesCases))]
        public void Construction_from_addresses(string testEmailAddresses,
            string[] expectedAddresses)
        {
            EmailAddressCollection sut = new EmailAddressCollection(testEmailAddresses);

            for (int i = 0; i < expectedAddresses.GetLength(0); i++)
            {
                Assert.That(sut[i].Address, Is.EqualTo(expectedAddresses[i]));
            }
        }
        [TestCase("")]
        [TestCase(null)]
        [TestCase("    ")]
        public void Add_an_address_ignores_blank_address(string? testAddress)
        {
            EmailAddressCollection sut = new EmailAddressCollection();

            sut.AddAddress(testAddress!);

            Assert.That(sut, Is.Empty);
        }
        [TestCase("bob@a.com", "Bob", "bob@a.com", "Bob")]
        public void Add_an_address(string testAddress, string testDisplayName, string expectedAddress, string expectedName)
        {
            EmailAddressCollection sut = new EmailAddressCollection();

            sut.AddAddress(testAddress, testDisplayName);

            EmailAddress emailAddress = sut.Single();
            Assert.That(emailAddress.DisplayName, Is.EqualTo(expectedName));
            Assert.That(emailAddress.Address, Is.EqualTo(expectedAddress));
        }
    }
}
