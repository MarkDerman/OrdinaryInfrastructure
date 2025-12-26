using System.Reflection;
using Targets;

namespace Tests.Odin.DesignContracts;

public enum AttributeFlavour
{
    Odin,
    BaseClassLibrary
}

public static class TestSupport
{
    public static Type GetTargetTestTypeFor(AttributeFlavour testCase)
    {
        if (testCase == AttributeFlavour.Odin)
        {
            return typeof(OdinInvariantTestTarget);
        }

        if (testCase == AttributeFlavour.BaseClassLibrary)
        {
            return typeof(BclInvariantTestTarget);
        }

        throw new NotSupportedException(testCase.ToString());
    }
    
    public static void SetPrivateField(Type declaringType, object instance, string fieldName, object? value)
    {
        FieldInfo f = declaringType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                      ?? throw new InvalidOperationException($"Missing field '{fieldName}'.");
        f.SetValue(instance, value);
    }
    
    public static object? Invoke(Type declaringType, object instance, string methodName)
    {
        MethodInfo m = declaringType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public)
                       ?? throw new InvalidOperationException($"Missing method '{methodName}'.");
        try
        {
            return m.Invoke(instance, null);
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            throw tie.InnerException;
        }
    }
    
}


