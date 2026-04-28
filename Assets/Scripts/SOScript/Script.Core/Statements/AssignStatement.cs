#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Script.Core.Expressions;
using Script.Core.Statements.ControlFlow;
using Script.Core.Types;
using Script.Core.Variables;

namespace Script.Core.Statements
{
    public sealed class AssignStatement: IStatement
    {
        public event Func<Task>? OnExecuteAsync;

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

        public string Name => $"Assign ({Var.Name})";

        public ControlFlowResult Execute()
        {
            Var.Update(ToAssign);
            return Next?.Execute() ?? ControlFlowResult.None;
        }

        public async Task<ControlFlowResult> ExecuteAsync()
        {
            if (OnExecuteAsync != null)
                await OnExecuteAsync();

            if (ToAssign is null)
                throw new ArgumentNullException(nameof(ToAssign));

            await Var.UpdateAsync(ToAssign);

            return await (Next?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
        }
    }
}