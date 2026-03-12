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

        public override int Arity()
        {
            return 2;
        }

        protected override void SetInput(int index, Expression? value)
        {
            switch (index)
            {
                case 0: 
                    LeftArg = value;
                    break;
                case 1:
                    RightArg = value;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        protected override Expression? GetInput(int index)
        {
            return index switch
            {
                0 => LeftArg,
                1 => RightArg,
                _ => throw new IndexOutOfRangeException()
            };
        }

        public BinaryExpression(BinaryOperatorOverloadSystem system)
        {
            this.system = system;
        }
    }
}