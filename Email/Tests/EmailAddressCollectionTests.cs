using Odin.Email;

namespace Tests.Odin.Email
{
    public sealed class EmailAddressCollectionTests
    {
        public static TheoryData<string, string[]> ConstructionFromAddressesCases() =>
            new()
            {
                { "Bob <B@a.com> , Geoff <G@y.com>       ;,,", ["B@a.com", "G@y.com"] }
            };

        [Theory]
        [MemberData(nameof(ConstructionFromAddressesCases))]
        public void Construction_from_addresses(string testEmailAddresses,
            string[] expectedAddresses)
        {
            EmailAddressCollection sut = new EmailAddressCollection(testEmailAddresses);
            
            for (int i = 0; i < expectedAddresses.GetLength(0); i++)
            {
                Assert.Equal(expectedAddresses[i], sut[i].Address);
            }
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("    ")]
        public void Add_an_address_ignores_blank_address(string? testAddress)
        {
            EmailAddressCollection sut = new EmailAddressCollection();
            
            sut.AddAddress(testAddress!);
           
            Assert.Empty(sut);
        }
        
        [Theory]
        [InlineData("bob@a.com", "Bob", "bob@a.com", "Bob")]
        public void Add_an_address(string testAddress, string testDisplayName, string expectedAddress, string expectedName)
        {
            EmailAddressCollection sut = new EmailAddressCollection();
            
            sut.AddAddress(testAddress, testDisplayName);
           
            EmailAddress emailAddress = Assert.Single(sut);
            Assert.Equal(expectedName, emailAddress.DisplayName);
            Assert.Equal(expectedAddress, emailAddress.Address);
        }
    }
}
