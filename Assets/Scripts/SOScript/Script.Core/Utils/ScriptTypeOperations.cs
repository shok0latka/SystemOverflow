using System;
using Script.Core.Types;
using Script.Core.Variables;
using Script.Core.Variables.Implementations;

public static class ScriptTypeOperations
{
    public static string GetTypeText(ScriptType type)
    {
        return type switch
        {
            ScriptType.Undefined => "undef",
            ScriptType.Float => "float",
            ScriptType.Integer => "int",
            ScriptType.String => "str",
            ScriptType.Boolean => "bool",
            _ => throw new NotImplementedException()
        };
    }

    public static Variable CreateVariable(ScriptType type, string name)
    {
        return type switch
        {
            ScriptType.Float => new FloatVariable(name),
            ScriptType.Integer => new IntVariable(name),
            ScriptType.String => new StringVariable(name),
            ScriptType.Boolean => new BooleanVariable(name),
            ScriptType.Undefined => throw new ArgumentException("Cound not create variable with undefined type"),
            _ => throw new NotImplementedException()
        };
    }
}