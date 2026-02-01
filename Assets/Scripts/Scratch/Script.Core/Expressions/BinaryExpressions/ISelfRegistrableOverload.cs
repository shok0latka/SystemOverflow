#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions
{
    public interface ISelfRegistrableOverload
    {
        void Register(Dictionary<(ScriptType, ScriptType), BinaryOperatorOverload> overloads);
    }
}