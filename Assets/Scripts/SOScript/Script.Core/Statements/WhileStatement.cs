#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Script.Core.Expressions;
using Script.Core.Statements.ControlFlow;
using Script.Core.Types;

namespace Script.Core.Statements
{
    public sealed class WhileStatement: IStatement
    {
        public event Func<Task>? OnExecuteAsync;

        public List<StatementArgument> Arguments { get; } = new() { 
            new StatementArgument("Condition", new List<ScriptType> { ScriptType.Boolean }) 
        };

        public Expression Condition
        {
            get => Arguments[0];
            set => Arguments[0].Attached = value;
        }

        public IStatement? Body { get; set; }

        public IStatement? Next { get; set; }

        IReadOnlyList<StatementArgument> IStatement.Arguments => Arguments;

        public string Name => "While";

        public ControlFlowResult Execute()
        {
            while (Convert.ToBoolean(Condition.Evaluate()))
            {
                var result = Body?.Execute() ?? ControlFlowResult.None;
                switch (result.Kind)
                {
                    case ControlFlowKind.Break:
                        {
                            return Next?.Execute() ?? ControlFlowResult.None;
                        }
                    case ControlFlowKind.Return:
                        {
                            return result;
                        }
                }
            }
            return Next?.Execute() ?? ControlFlowResult.None;
        }

        public async Task<ControlFlowResult> ExecuteAsync()
        {
            if (OnExecuteAsync != null)
                await OnExecuteAsync();

            while (Convert.ToBoolean(await Condition.EvaluateAsync()))
            {
                var result = await (Body?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
                switch (result.Kind)
                {
                    case ControlFlowKind.Break:
                        {
                            return await (Next?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
                        }
                    case ControlFlowKind.Return:
                        {
                            return result;
                        }
                }
            }

            return await (Next?.ExecuteAsync() ?? Task.FromResult(ControlFlowResult.None));
        }
    }
}