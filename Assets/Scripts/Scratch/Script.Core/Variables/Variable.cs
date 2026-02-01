using Script.Core.Expressions;
using Script.Core.Types;

namespace Script.Core.Variables
{
    public abstract class Variable
    {
        public ScriptType Type { get; init; }

        public abstract object Raw { get; }

        protected Variable(ScriptType type)
        {
            Type = type;
        }
    //TODO Сделать метод TryUpdate
        public void Update(Expression e)
        {
            ValidateType(e.Type);
            Assign(e);
        }
    //TODO Сделать return bool
        public virtual void ValidateType(ScriptType type)
        {
            if (type != ScriptType.Undefined && type != Type)
            {
                throw new ArgumentException($"Incorrect argument type: {type}. Expected: {Type}");
            }
        } 

        public abstract void Assign(Expression e);
    }
}