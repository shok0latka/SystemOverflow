#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions
{
    public abstract class BinaryOperatorOverloadSystem
    {
        public BinaryOperatorTag Tag { get; set; }

        protected Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload> overloads = new();

        public BinaryOperatorOverload? Resolve(ScriptType left, ScriptType right)
        {
            return overloads.GetValueOrDefault((left, right));
        }

        public BinaryOperatorOverloadSystem(BinaryOperatorTag tag)
        {
            Tag = tag;
        }
    }

    public class BinaryOperatorOverloadSystem<TOverload> : BinaryOperatorOverloadSystem
        where TOverload : BinaryOperatorOverload, ITaggedBinaryOperator
    {
        public BinaryOperatorOverloadSystem() : base(CreateTag())
        {
            RegisterAllOverloadsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private static BinaryOperatorTag CreateTag()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var overloadBaseType = typeof(TOverload);

            var concreteType = assembly.GetTypes()
                .FirstOrDefault(t => !t.IsAbstract && overloadBaseType.IsAssignableFrom(t));

            if (concreteType is null)
            {
                throw new InvalidOperationException($"No concrete implementation of {overloadBaseType.FullName} found to determine Tag");
            }

            var instance = (TOverload)Activator.CreateInstance(concreteType)!;
            return instance.Tag;
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
            var instance = (BinaryOperatorOverload)Activator.CreateInstance(type)!;
            if (instance is not ISelfRegistrableOverload registrable)
            {
                throw new InvalidOperationException(
                    $"Type {type.FullName} implements ISelfRegistrableOverload but does not define Register");
            }

            registrable.Register(overloads);
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
}