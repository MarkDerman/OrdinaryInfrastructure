namespace Odin.Emailing;

/// <summary>
/// Converts from EmailAddress to Microsoft.Graph.Models.EmailAddress
/// </summary>
public static class EmailAddressExtensions
{
    /// <summary>
    /// Converts from EmailAddress to Microsoft.Graph.Models.EmailAddress
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static Microsoft.Graph.Models.EmailAddress ToOffice365EmailAddress(this EmailAddress address)
    {
        return new Microsoft.Graph.Models.EmailAddress
        {
            Address = address.Address,
            Name = address.DisplayName
        };
    }
}