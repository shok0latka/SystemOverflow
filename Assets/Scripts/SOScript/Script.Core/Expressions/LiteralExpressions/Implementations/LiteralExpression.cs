#nullable enable

using System;
using System.Text;
using Script.Core.Types;

namespace Script.Core.Expressions.LiteralExpressions.Implementations
{
    public sealed class LiteralExpression : UserInputExpression
    {
        private string _value = string.Empty;

        protected override void Reparse()
        {
            _value = ParseEscapes(RawText);
            Type = ScriptType.String;
        }

        public override object? Evaluate()
            => _value;

        private static string ParseEscapes(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sb = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '\\' && i + 1 < input.Length)
                {
                    char next = input[++i];

                    sb.Append(next switch
                    {
                        'n' => '\n',
                        't' => '\t',
                        '\\' => '\\',
                        '"' => '"',
                        _ => '\\' + next.ToString()
                    });
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}