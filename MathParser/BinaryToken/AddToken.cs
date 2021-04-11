#nullable enable

using System.Collections.Generic;

namespace MathParser
{
    public class AddToken : IBinaryToken
    {

        public class Builder2 : BinaryTokenBuilder<AddToken>
        { 
            public override bool IsMultiply => false;

            protected override AddToken Build(IToken leftToken, IToken rightToken)
            {
                return new AddToken(leftToken, rightToken);
            }
        }

        public class Acceptor : ITokenAcceptor<AddToken>
        {
            public ITokenBuilder<AddToken>? Accept(string expr, ref int offset)
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

        public AddToken(IToken left, IToken right)
        {
            Left = left;
            Right = right;
        }

        public double Calculate(Dictionary<string, double> variables)
        {
            return Left.Calculate(variables) + Right.Calculate(variables);
        }
    }

}
