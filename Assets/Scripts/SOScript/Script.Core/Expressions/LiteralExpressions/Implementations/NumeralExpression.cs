#nullable enable

using System;
using System.Globalization;
using System.Threading.Tasks;
using Script.Core.Types;

namespace Script.Core.Expressions.LiteralExpressions.Implementations
{
    public sealed class NumeralExpression : UserInputExpression
    {
        private object? _value;

        protected override void Reparse()
        {
            _value = null;
            Type = ScriptType.Undefined;

            if (string.IsNullOrWhiteSpace(RawText))
                return;
            
            var style = NumberStyles.Float;
            var culture = CultureInfo.InvariantCulture;

            if (int.TryParse(RawText, style, culture, out var i))
            {
                _value = i;
                Type = ScriptType.Integer;
                return;
            }

            if (float.TryParse(RawText, style, culture, out var f))
            {
                _value = f;
                Type = ScriptType.Float;
                return;
            }
        }

        public override object? Evaluate()
        {
            InvokeOnEvaluate();
            return Type == ScriptType.Undefined ? null : _value;
        }

        public override async Task<object?> EvaluateAsync()
        {
            await InvokeOnEvaluateAsync();
            return Type == ScriptType.Undefined ? null : _value;
        }
    }
}