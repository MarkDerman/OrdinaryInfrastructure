using Odin.Testing;
using Odin.Testing.NUnit;
using Odin.Testing.XUnit;
using Odin.Testing.XUnitV2;

namespace Tests.Odin.Testing;

public static class Adaptors
{
    public static IReadOnlyList<IAssertionAdaptor> GetAllTestFrameworkAdaptors()
    {
        return new List<IAssertionAdaptor>
        {
            new XUnitAssertionAdaptor(),
            new NUnitAssertionAdaptor(),
            new XUnitV2AssertionAdaptor()
        };
    }
}