#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Expressions;
using Script.Core.Statements.ControlFlow;
using Script.Core.Types;

namespace Script.Core.Statements
{
    public sealed class IfStatement : IStatement
    {

        public List<StatementArgument> Arguments { get; } = new() { 
            new StatementArgument("Condition", new List<ScriptType> { ScriptType.Boolean }) 
        };

        public Expression Condition
        {
            get => Arguments[0];
            set => Arguments[0].Attached = value;
        }

        public IStatement? Next { get; set; }

        public IStatement? Do { get; set; }
        public IStatement? Else { get; set; }

        IReadOnlyList<StatementArgument> IStatement.Arguments => Arguments;

        public ControlFlowResult Execute()
        {
            ControlFlowResult result;

            if (Convert.ToBoolean(Condition?.Evaluate()))
            {
                result = Do?.Execute() ?? ControlFlowResult.None;
            }
            else
            {
                result =  Else?.Execute() ?? ControlFlowResult.None;
            }

            if (result.Kind != ControlFlowKind.None)
            {
                return result;
            }

            return Next?.Execute() ?? ControlFlowResult.None;
        }
    }
}