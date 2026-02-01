using Script.Core.Expressions;
using Script.Core.Statements.ControlFlow;
using Script.Core.Types;
using Script.Core.Variables;

namespace Script.Core.Statements
{
    public sealed class AssignStatement : IStatement
    {
        private Expression? toAssign;

        public required Variable Var { get; init; }
        public Expression? ToAssign
        {
            get => toAssign;
            set
            {
                var newType = value?.Type ?? ScriptType.Undefined;
                if (newType != ScriptType.Undefined && newType != Var.Type)
                {
                    throw new ArgumentException($"Incorrect assign expression type: {newType}. Expected: {Var.Type} or {ScriptType.Undefined}", nameof(ToAssign));
                }
                toAssign = value;
            }
        } 

        public ControlFlowResult Execute()
        {
            var type = ToAssign?.Type ?? ScriptType.Undefined;
            if (type != Var.Type)
            {
                throw new ArgumentException($"At runtime assign expression type required to be {Var.Type}");
            }
            Var.Assign(ToAssign!);
            return ControlFlowResult.None;
        }
    }
}