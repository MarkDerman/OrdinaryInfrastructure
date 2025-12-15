using System.Reflection;
using System.Runtime.Loader;
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
    public ResultValue<T> TryCreate<T>(string fullTypeName, string assemblyToLoadFrom) where T : class
    {
        Contract.Requires(!string.IsNullOrWhiteSpace(fullTypeName));
        Contract.Requires(!string.IsNullOrWhiteSpace(assemblyToLoadFrom));

        AssemblyName assemblyToLoad = new AssemblyName(assemblyToLoadFrom);
        Assembly? assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyToLoad);
        if (assembly == null!) return ResultValue<T>.Failure($"Unable to load assembly {assemblyToLoadFrom}");
        
        Type? type = assembly.GetType(fullTypeName);
        if (type == null) return ResultValue<T>.Failure($"Unable to create type {fullTypeName} from assembly {assemblyToLoadFrom}");

        object? instance = Activator.CreateInstance(type);
        if (instance == null) return ResultValue<T>.Failure($"Could not create instance of type {type.Name}");
        if (instance is T objT)
        {
            return ResultValue<T>.Succeed(objT);
        }

        return ResultValue<T>.Failure($"Type {type.FullName} is not of type {nameof(T)}");
    }


    /// <summary>
    /// Attempts to create the specified type from the currently loaded application assemblies
    /// </summary>
    /// <param name="fullTypeName"></param>
    /// <returns></returns>
    public ResultValue<T> TryCreate<T>(string fullTypeName) where T : class
    {
        Contract.Requires(!string.IsNullOrWhiteSpace(fullTypeName));
        Type? typeToCreate = Type.GetType(fullTypeName);
        if (typeToCreate != null)
        {
            return TryCreate<T>(typeToCreate);
        }

        List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        foreach (Assembly asm in assemblies)
        {
            typeToCreate = asm.GetType(fullTypeName);
            if (typeToCreate != null)
                return TryCreate<T>(typeToCreate);
        }

        return ResultValue<T>.Failure($"No assembly contains {fullTypeName}");
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
                return ResultValue<T>.Succeed(objT);
            }

            return ResultValue<T>.Failure($"Type {typeToCreate.FullName} is not of type {nameof(T)}");
        }
        catch (Exception e)
        {
            return ResultValue<T>.Failure($"Type {typeToCreate.FullName} could not be created. {e.Message}");
        }
    }
}