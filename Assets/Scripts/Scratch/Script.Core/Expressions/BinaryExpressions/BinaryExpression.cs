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
        public BinaryOperatorTag Tag => System.Tag;

        private BinaryOperatorOverloadSystem System { get; init; }
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
            CurrentOverload = System.Resolve(
                LeftArg?.Type ?? ScriptType.Undefined,
                RightArg?.Type ?? ScriptType.Undefined
            );
        }

        public override object? Evaluate()
        {
            ArgumentNullException.ThrowIfNull(LeftArg, nameof(LeftArg));
            ArgumentNullException.ThrowIfNull(RightArg, nameof(RightArg));
            ArgumentNullException.ThrowIfNull(CurrentOverload, nameof(CurrentOverload));
            return CurrentOverload.Evaluate(LeftArg, RightArg);
        }

        public BinaryExpression(BinaryOperatorOverloadSystem system)
        {
            System = system;
        }
    }
}