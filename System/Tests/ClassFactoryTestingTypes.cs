namespace Tests.Odin.System;

public interface Interface1
{
    public string? Member1 { get; set; }
}

public class Inherited2 : Interface1
{
    public string? Member1 { get; set; }
    public string? Member2 { get; set; }
}

public class Class3
{
}
