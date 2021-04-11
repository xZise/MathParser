#nullable enable

using System.Collections.Generic;

namespace MathParser
{
    public class MultiplyToken : IBinaryToken
    {

        public class Builder2 : BinaryTokenBuilder<MultiplyToken>
        {
            public override bool IsMultiply => true;

            protected override MultiplyToken Build(IToken leftToken, IToken rightToken)
            {
                return new MultiplyToken(leftToken, rightToken);
            }
        }

        public class Acceptor : ITokenAcceptor<MultiplyToken>
        {
            public ITokenBuilder<MultiplyToken>? Accept(string expr, ref int offset)
            {
                if (expr[offset] == '*')
                {
                    offset++;
                    return new Builder2();
                }
                else
                {
                    return null;
                }
            }
        }

        public IToken Left { get; }
        public IToken Right { get; }

        public MultiplyToken(IToken left, IToken right)
        {
            Left = left;
            Right = right;
        }

        public double Calculate(Dictionary<string, double> variables)
        {
            return Left.Calculate(variables) * Right.Calculate(variables);
        }
    }

}
