#nullable enable

using System;
using Script.Core.Types;

namespace Script.Core.Expressions.BinaryExpressions
{
    public class BinaryExpression: Expression
    {
        private Expression? leftArg;
        private Expression? rightArg;
        private BinaryOperatorOverload? currentOverload;
        public Expression? LeftArg
        {
            get => leftArg;
            set
            {
                leftArg = value;
                if (leftArg is not null)
                {
                    leftArg.Parent = this;
                }
                UpdateTypes();
            }
        }
        public Expression? RightArg
        {
            get => rightArg;
            set
            {
                rightArg = value;
                if (rightArg is not null)
                {
                    rightArg.Parent = this;
                }
                UpdateTypes();
            }
        }
        public BinaryOperatorTag Tag => system.Tag;

        private BinaryOperatorOverloadSystem system;
        private BinaryOperatorOverload? CurrentOverload
        {
            get => currentOverload;
            set
            {
            currentOverload = value;
            Type = currentOverload?.ResultType ?? ScriptType.Undefined; 
            }
        } 

        public override void UpdateTypes()
        {
            CurrentOverload = system.Resolve(
                LeftArg?.Type ?? ScriptType.Undefined,
                RightArg?.Type ?? ScriptType.Undefined
            );
        }

        public override object? Evaluate()
        {
            if (LeftArg is null)
            {
                throw new ArgumentNullException(nameof(LeftArg));
            }
            if (RightArg is null)
            {
                throw new ArgumentNullException(nameof(RightArg));
            }
            if (CurrentOverload is null)
            {
                throw new ArgumentNullException(nameof(CurrentOverload));
            }
            return CurrentOverload.Evaluate(LeftArg, RightArg);
        }

        public BinaryExpression(BinaryOperatorOverloadSystem system)
        {
            this.system = system;
        }
    }
}