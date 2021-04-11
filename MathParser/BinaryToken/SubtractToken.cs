#nullable enable

using System.Collections.Generic;

namespace MathParser
{
    public class SubtractToken : IBinaryToken
    {

        public class Builder2 : BinaryTokenBuilder<SubtractToken>
        { 
            public override bool IsMultiply => false;

            protected override SubtractToken Build(IToken leftToken, IToken rightToken)
            {
                return new SubtractToken(leftToken, rightToken);
            }
        }

        public class Acceptor : ITokenAcceptor<SubtractToken>
        {
            public ITokenBuilder<SubtractToken>? Accept(string expr, ref int offset)
            {
                if (expr[offset] == '+')
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

        public SubtractToken(IToken left, IToken right)
        {
            Left = left;
            Right = right;
        }

        public double Calculate(Dictionary<string, double> variables)
        {
            return Left.Calculate(variables) - Right.Calculate(variables);
        }
    }

}
