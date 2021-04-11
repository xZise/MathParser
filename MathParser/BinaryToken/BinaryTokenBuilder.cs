#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

namespace MathParser
{
    public abstract class BinaryTokenBuilder<T>: IBinaryTokenBuilder<T> where T : IBinaryToken
    {
        private ITokenBuilder<IToken>? left = null;
        private ITokenBuilder<IToken>? right = null;
        public ITokenBuilder<IToken> Left
        {
            get
            {
                ThrowIfNull(left, nameof(Left));
                return left;
            }
            set
            {
                ThrowIfSetNull(value, nameof(Left));
                left = value;
            }
        }
        public ITokenBuilder<IToken> Right
        {
            get
            {
                ThrowIfNull(right, nameof(Right));
                return right;
            }
            set
            {
                ThrowIfSetNull(value, nameof(Right));
                right = value;
            }
        }

        public bool Binary => true;

        public abstract bool IsMultiply { get; }

        public static void ThrowIfNull<T2>([NotNull] T2 value, string name)
        {
            if (value is null)
            {
                throw new NullReferenceException($"Property {name} has not been initialized to a non-null value!");
            }
        }

        public static void ThrowIfSetNull<T2>([NotNull] T2 value, string name)
        {
            if (value is null)
            {
                throw new NullReferenceException($"Property {name} has wasn't set with a non-null value!");
            }
        }

        public T Build()
        {
            return Build(Left.Build(), Right.Build());
        }

        protected abstract T Build(IToken leftToken, IToken rightToken);
    }

}
