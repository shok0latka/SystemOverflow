using Script.Core.Expressions;
using Script.Core.Statements.ControlFlow;
using Script.Core.Types;

namespace Script.Core.Statements
{
    public sealed class WhileStatement: IStatement
    {
        private Expression? condition;
        public Expression? Condition
        {
            get => condition;
            set
            {
                var newType = value?.Type ?? ScriptType.Undefined;
                if (newType != ScriptType.Undefined && newType != ScriptType.Boolean)
                {
                    throw new ArgumentException($"Incorrect condition type {newType}. Expected: {ScriptType.Boolean} or {ScriptType.Undefined}", nameof(Condition));
                }
                condition = value;
            }
        }

        public SequenceStatement? Body { get; set; }

        public ControlFlowResult Execute()
        {
            if (Condition?.Type is not ScriptType.Boolean)
            {
                throw new ArgumentException("At runtime condition type must be bool", nameof(Condition));
            }

            while (Convert.ToBoolean(Condition?.Evaluate()))
            {
                var result = Body?.Execute() ?? ControlFlowResult.None;
                switch (result.Kind)
                {
                    case ControlFlowKind.Break:
                        {
                            return ControlFlowResult.None;
                        }
                    case ControlFlowKind.Return:
                        {
                            return result;
                        }
                }
            }
            return ControlFlowResult.None;
        }
    }
}