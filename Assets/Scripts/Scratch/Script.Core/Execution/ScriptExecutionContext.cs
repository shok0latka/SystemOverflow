using Script.Core.Variables;

namespace Script.Core.Execution;

public sealed class ScriptExecutionContext
{
    public Dictionary<string, Variable> Variables { get; } = [];
}