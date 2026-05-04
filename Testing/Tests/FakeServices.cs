namespace Tests.Odin.Testing;

public interface IFakeService
{
    string Foo();
}

public class FakeService : IFakeService
{
    public string Foo()
    {
        return "Foo";
    }
}

public interface IFakeService2
{
    string Bar();
}

public class FakeService2 : IFakeService2
{
    public string Bar()
    {
        return "Bar";
    }
}

public record FakeOptions(string Bar, int Foo);
