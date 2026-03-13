#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Expressions;
using Script.Core.Statements.ControlFlow;
using Script.Core.Types;
using Script.Core.Variables;

namespace Script.Core.Statements
{
    public sealed class AssignStatement: IStatement
    {
        public Variable Var { get; private set; }

        public List<StatementArgument> Arguments { get; } = new();

        public IStatement? Next { get; set; } = null;

        public AssignStatement(Variable var)
        {
            Var = var;
            Arguments.Add(new StatementArgument("Value", new List<ScriptType> { var.Type }));
        }

        public Expression ToAssign
        {
            get => Arguments[0];
            set => Arguments[0].Attached = value;
        }

        IReadOnlyList<StatementArgument> IStatement.Arguments => Arguments;

        public ControlFlowResult Execute()
        {
            Var.Assign(ToAssign!);
            return Next?.Execute() ?? ControlFlowResult.None;
        }
    }
}