using Script.Core.Types;

namespace Script.Core.Expressions
{
    public abstract class Expression
    {
        private ScriptType type;

        public ScriptType Type
        {
            get => type;
            set
            {
                if (value != type)
                {
                    type = value;
                    Parent?.UpdateTypes();
                }
            }
        }
        public Expression? Parent { get; set; } = null;

        public virtual void UpdateTypes()
        {
            
        }

        public abstract object? Evaluate();
    }
}