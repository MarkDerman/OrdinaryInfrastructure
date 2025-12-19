using System.Runtime.Remoting;
using Odin.DesignContracts;
using Odin.System;

namespace Odin.Utility;

/// <summary>
/// 
/// </summary>
public static class ClassFactory
{
    /// <summary>
    /// Attempts to create the specified type from the currently loaded application assemblies, from the assembly specified.
    /// </summary>
    /// <param name="typeName">The fully qualified name of the type to create an instance of.</param>
    /// <param name="assemblyName">The name of the assembly where the type named typeName is sought.</param>
    /// <returns></returns>
    public static ResultValue<T> TryCreate<T>(string typeName, string assemblyName) where T : class
    {
        Precondition.Requires(!string.IsNullOrWhiteSpace(typeName));
        Precondition.Requires(!string.IsNullOrWhiteSpace(assemblyName));
        ObjectHandle? handle;
        try
        {
            handle = Activator.CreateInstance(assemblyName, typeName);
        }
        catch (Exception err)
        {
            return CreateInstanceFailure<T>(typeName, assemblyName, err.Message);
        }
        if (handle == null) return CreateInstanceFailure<T>(typeName, assemblyName);
        object? instance = handle.Unwrap();
        if (instance == null) return CreateInstanceFailure<T>(typeName, assemblyName);
        if (instance is T objT)
        {
            return ResultValue<T>.Success(objT);
        }
        return ResultValue<T>.Failure($"Type {typeName} is not of type {nameof(T)}");
    }

    private static ResultValue<T> CreateInstanceFailure<T>(string typeName, string assemblyName, string? errorMessage = "")
    {
        return ResultValue<T>.Failure($"Could not create instance of type {typeName} from assembly {assemblyName}. {errorMessage}");
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeToCreate"></param>
    /// <returns></returns>
    public static ResultValue<T> TryCreate<T>(Type typeToCreate) where T : class
    {
        Precondition.Requires(typeToCreate!=null!);
        try
        {
            object? obj = Activator.CreateInstance(typeToCreate);
            if (obj == null) return ResultValue<T>.Failure($"Could not create instance of type {typeToCreate.Name}");
            if (obj is T objT)
            {
                return ResultValue<T>.Success(objT);
            }

            return ResultValue<T>.Failure($"Type {typeToCreate.FullName} is not of type {nameof(T)}");
        }
        catch (Exception e)
        {
            return ResultValue<T>.Failure($"Type {typeToCreate.FullName} could not be created. {e.Message}");
        }
    }
}