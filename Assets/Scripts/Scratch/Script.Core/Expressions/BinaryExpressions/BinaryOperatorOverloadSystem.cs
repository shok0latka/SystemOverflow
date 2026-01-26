using System.Reflection;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions;

public abstract class BinaryOperatorOverloadSystem(BinaryOperatorTag tag)
{
    public BinaryOperatorTag Tag { get; init; } = tag;

    protected Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload> overloads = [];

    public BinaryOperatorOverload? Resolve(ScriptType left, ScriptType right)
    {
        return overloads.GetValueOrDefault((left, right));
    }
}

public class BinaryOperatorOverloadSystem<TOverload> : BinaryOperatorOverloadSystem
    where TOverload : BinaryOperatorOverload, ITaggedBinaryOperator
{
    public BinaryOperatorOverloadSystem() : base(TOverload.Tag)
    {
        RegisterAllOverloadsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void RegisterAllOverloadsFromAssembly(Assembly assembly)
    {
        var overloadBaseType = typeof(TOverload);
        var selfRegistrableInterface = typeof(ISelfRegistrableOverload);

        var types = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                overloadBaseType.IsAssignableFrom(t));

        foreach (var type in types)
        {
            if (ImplementsSelfRegistrable(type, selfRegistrableInterface))
            {
                InvokeSelfRegister(type);
                continue;
            }

            RegisterDefault(type);
        }
    }
    private static bool ImplementsSelfRegistrable(
        Type concreteType,
        Type selfRegistrableInterface)
    {
        return concreteType
            .GetInterfaces()
            .Any(i =>
                i.IsGenericType == false &&
                i == selfRegistrableInterface);
    }

    private void InvokeSelfRegister(Type type)
    {
        var method = type.GetMethod(
            nameof(ISelfRegistrableOverload.Register),
            BindingFlags.Public | BindingFlags.Static) ?? throw new InvalidOperationException(
                $"Type {type.FullName} implements ISelfRegistrableOverload but does not define Register");
        var args = new object[] { overloads };
        method.Invoke(null, args);

        overloads = (Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload>)args[0];
    }

    private void RegisterDefault(Type type)
    {
        var instance = (BinaryOperatorOverload)Activator.CreateInstance(type)!;

        var key = (instance.LeftArg, instance.RightArg);

        if (!overloads.TryAdd(key, instance))
        {
            throw new InvalidOperationException(
                $"Duplicate overload found for {Tag} with key {key}");
        }
    }
}