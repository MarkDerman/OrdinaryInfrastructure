## About Odin.System.StringEnum

[![NuGet](https://img.shields.io/nuget/v/Odin.System.StringEnum.svg)](https://www.nuget.org/packages/Odin.System.StringEnum)  ![Nuget](https://img.shields.io/nuget/dt/Odin.System.StringEnum)

[Odin.System.StringEnum](https://www.nuget.org/packages/Odin.System.StringEnum), part of the [OrDinary INfrastructure](https://github.com/MarkDerman/OrdinaryInfrastructure) libraries, provides enum-like behaviour for a set of string values via StringEnum,
as well as a useful StringEnumMemberAttribute.

## Getting Started

### 1 - Define your string 'enum'

Create your string 'enum' class by inheriting from StringEnum and adding public string constant members.

```csharp
    public class LoaderTypes : StringEnum<LoaderTypes>
    {
        public const string File = "FILE";
        public const string DynamicSql = "DYNAMIC-SQL";
    }
```
### 2 - Use the class like an enum

```csharp
    if (loaderOptions.LoaderType == LoaderTypes.DynamicSql)
    ...    
```
### 3 - Use the static HasValue(string value) convenience method

```csharp
    bool memberExists = LoaderTypes.HasValue("CUSTOM"); // returns false
```

### 4 - Enumerate all values via the static Values property

```csharp
    string message = $"Valid members are: {string.Join(" | ", LoaderTypes.Values)}"
```

### 5 - There is also a Validation attribute

```csharp
    public record LoaderEditModel : IValidatableObject
    {
        [Required(AllowEmptyStrings = false)]
        [StringEnumMember<LoaderTypes>]
        public required string Loader { get; set; }
        ...
    }
```
