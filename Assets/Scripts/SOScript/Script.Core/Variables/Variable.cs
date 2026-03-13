#nullable enable

using System;
using System.Threading.Tasks;
using Script.Core.Expressions;
using Script.Core.Types;

namespace Script.Core.Variables
{
    public abstract class Variable
    {
        public ScriptType Type { get; set; }
        public string Name { get; private set; }

        public abstract object Raw { get; }

        protected Variable(ScriptType type, string name)
        {
            Type = type;
            Name = name;
        }
    //TODO Сделать метод TryUpdate
        public void Update(Expression e)
        {
            ValidateType(e.Type);
            Assign(e);
        }

        public async Task UpdateAsync(Expression e)
        {
            if (e is null)
                throw new ArgumentNullException(nameof(e));

            ValidateType(e.Type);
            await AssignAsync(e);
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

        public abstract Task AssignAsync(Expression e);
    }
}