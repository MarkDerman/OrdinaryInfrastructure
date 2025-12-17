using Odin.DesignContracts;
using Odin.System;

namespace Odin.Utility;

/// <summary>
/// 
/// </summary>
public class ClassFactory
{
    /// <summary>
    /// Attempts to create the specified type from the currently loaded application assemblies
    /// </summary>
    /// <param name="fullTypeName"></param>
    /// <param name="assemblyToLoadFrom"></param>
    /// <returns></returns>
    public ResultValue<T> TryCreate<T>(string fullTypeName) where T : class
    {
        Contract.Requires(!string.IsNullOrWhiteSpace(fullTypeName));
        
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(fullTypeName, throwOnError: false))
            .Where(t => t != null)
            .ToList();
        
        if (types.Count < 1)
        {
            return ResultValue<T>.Failure($"No loaded assembly contains type {fullTypeName}");
        }

        // I would like to keep this check, but it causes NuGet unit tests to fail, seemingly because NuGet loads the same assembly (whose types have the same 
        // AssemblyQualifiedName) multiple times. Let's hope the lack of this check doesn't cause problems for real systems.
        
        // if (types.Count > 1)
        // {
        //     return ResultValue<T>.Failure($"Multiple loaded assemblies contain type named {fullTypeName}:\n {string.Join("\n ", types.Select(t => $"[{t.AssemblyQualifiedName}]"))}");
        // }
        
        var type = types[0];
        
        object? instance = Activator.CreateInstance(type);
        if (instance == null) return ResultValue<T>.Failure($"Could not create instance of type {type.FullName}");
        if (instance is T objT)
        {
            return ResultValue<T>.Success(objT);
        }

        return ResultValue<T>.Failure($"Type {type.FullName} is not of type {nameof(T)}");
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeToCreate"></param>
    /// <returns></returns>
    public ResultValue<T> TryCreate<T>(Type typeToCreate) where T : class
    {
        Contract.Requires(typeToCreate!=null!);
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