#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MathParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string expr = "1+a*3";

            Dictionary<string, double> variables = new();

            List<ITokenAcceptor<IToken>> constantAcceptors = new();
            constantAcceptors.Add(new ConstantToken.Acceptor());
            constantAcceptors.Add(new VariableToken.Acceptor());

            List<ITokenAcceptor<IToken>> binaryAcceptors = new();
            binaryAcceptors.Add(new AddToken.Acceptor());
            binaryAcceptors.Add(new SubtractToken.Acceptor());
            binaryAcceptors.Add(new MultiplyToken.Acceptor());

            int index = 0;
            ITokenBuilder<IToken>? root = null;
            IBinaryTokenBuilder<IBinaryToken>? lastBinary = null;
            ITokenBuilder<IToken>? _left = null;
            List<ITokenAcceptor<IToken>> acceptors = constantAcceptors;
            while (index < expr.Length)
            {
                bool foundAcceptor = false;
                foreach (ITokenAcceptor<IToken> acceptor in acceptors)
                {
                    ITokenBuilder<IToken>? builder = acceptor.Accept(expr, ref index);
                    if (builder != null)
                    {
                        IBinaryTokenBuilder<IBinaryToken>? builderAsBinary = builder as IBinaryTokenBuilder<IBinaryToken>;
                        if (root == null)
                        {
                            if (builderAsBinary != null)
                            {
                                throw new Exception("First token cannot be a binary token");
                            }

                            root = builder;
                        }

                        if (builderAsBinary != null)
                        {
                            if (lastBinary == null || lastBinary.IsMultiply || !builderAsBinary.IsMultiply)
                            {
                                if (lastBinary != null)
                                {
                                    lastBinary.Right = _left;
                                }
                                builderAsBinary.Left = root;
                                root = builder;
                            }
                            else
                            {
                                builderAsBinary.Left = _left;
                                lastBinary.Right = builder;
                            }
                            lastBinary = builderAsBinary;
                            _left = null;
                            acceptors = constantAcceptors;
                        }
                        else
                        {
                            _left = builder;
                            acceptors = binaryAcceptors;

                            if (builder is VariableToken variableToken)
                            {
                                variables[variableToken.Name] = 0;
                            }
                        }

                        foundAcceptor = true;
                        break;
                    }
                }

                if (!foundAcceptor)
                {
                    Console.WriteLine($"Cannot determine token '{expr[index]}'");
                    Console.WriteLine(expr);
                    Console.WriteLine($"{new string(' ', index)}^{new string('-', expr.Length - index + 2)}");

                    root = null;
                    break;
                }
            }

            if (_left != null && lastBinary != null)
            {
                lastBinary.Right = _left;
            }

            if (root != null)
            {
                foreach (string key in variables.Keys)
                {
                    Console.WriteLine($"Define the value for {key}:");
                    while (true)
                    {
                        string answer = Console.ReadLine() ?? "";
                        if (double.TryParse(answer, out double result))
                        {
                            variables[key] = result;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Not a double!");
                        }
                    }
                }
                IToken rootToken = root.Build();
                PrintToken(rootToken, 0, variables);
                Console.WriteLine($"Calculated: {expr} = {rootToken.Calculate(variables)}");
            }
            else
            {
                Console.WriteLine("No token defined!");
            }
        }

        private static void PrintToken(IToken token, int indent, Dictionary<string, double> variables)
        {
            string indentStr = new string(' ', indent);
            switch (token)
            {
                case IBinaryToken binaryToken:
                    Console.WriteLine($"{indentStr}+ {token.GetType().Name}");
                    PrintToken(binaryToken.Left, indent + 2, variables);
                    PrintToken(binaryToken.Right, indent + 2, variables);
                    break;
                case ConstantToken constant:
                    Console.WriteLine($"{indentStr}- {token.GetType().Name} ({constant.Value})");
                    break;
                case VariableToken variable:
                    Console.WriteLine($"{indentStr}- {token.GetType().Name} ({variable.Name}: {variables[variable.Name]})");
                    break;
                default:
                    throw new InvalidOperationException($"Unknown token type {token.GetType().FullName}");
            }
            
        }
    }

    public class Subformula
    {
        ITokenBuilder<IToken>? root = null;
        IBinaryTokenBuilder<IBinaryToken>? lastBinary = null;
        ITokenBuilder<IToken>? left = null;
        bool isBinary = false;

        private static List<ITokenAcceptor<IToken>> constantAcceptors = new();
        private static List<ITokenAcceptor<IToken>> binaryAcceptors = new();

        static Subformula()
        {
            constantAcceptors.Add(new ConstantToken.Acceptor());
            constantAcceptors.Add(new VariableToken.Acceptor());
            binaryAcceptors.Add(new AddToken.Acceptor());
            binaryAcceptors.Add(new SubtractToken.Acceptor());
            binaryAcceptors.Add(new MultiplyToken.Acceptor());
        }

        public static List<ITokenAcceptor<IToken>> GetAcceptors(bool binary)
        {
            return binary ? binaryAcceptors : constantAcceptors;
        }
    }

    public interface ITokenAcceptor<out T> where T: IToken
    {
        ITokenBuilder<T>? Accept(string expr, ref int offset);
    }

    public interface ITokenBuilder<out T> where T: IToken
    {
        bool Binary { get; }
        T Build();
    }

    public interface IBinaryTokenBuilder<out T> : ITokenBuilder<T> where T : IBinaryToken
    {
        ITokenBuilder<IToken> Left { get; set; }
        ITokenBuilder<IToken> Right { get; set; }

        bool IsMultiply { get; }
    }

    public interface IToken
    {
        double Calculate(Dictionary<string, double> variables);
    }

    public interface IBinaryToken : IToken
    {
        IToken Left { get; }
        IToken Right { get; }
    }

    public class ConstantToken : IToken, ITokenBuilder<ConstantToken>
    {
        public class Acceptor : ITokenAcceptor<ConstantToken>
        {
            public ITokenBuilder<ConstantToken>? Accept(string expr, ref int offset)
            {
                // "^" with a startat-parameter does not properly work, so it uses \G instead
                Regex r = new Regex(@"\G[+-]?([0-9]*\.)?[0-9]+");
                Match match = r.Match(expr, offset);
                if (match.Success)
                {
                    string matched = match.Value;
                    double value = double.Parse(matched, CultureInfo.InvariantCulture);
                    offset += matched.Length;
                    return new ConstantToken(value);
                }
                else
                {
                    return null;
                }
            }
        }

        public bool Binary => false;

        public double Value { get; }

        public ConstantToken(double value)
        {
            Value = value;
        }

        public double Calculate(Dictionary<string, double> variables)
        {
            return Value;
        }

        public ConstantToken Build()
        {
            return this;
        }
    }

    public class VariableToken : IToken, ITokenBuilder<VariableToken>
    {
        public class Acceptor : ITokenAcceptor<VariableToken>
        {
            public ITokenBuilder<VariableToken>? Accept(string expr, ref int offset)
            {
                // "^" with a startat-parameter does not properly work, so it uses \G instead
                Regex r = new Regex(@"\G[a-z][a-z0-9]*");
                Match match = r.Match(expr, offset);
                if (match.Success)
                {
                    string matched = match.Value;
                    offset += matched.Length;
                    return new VariableToken(matched);
                }
                else
                {
                    return null;
                }
            }
        }

        public bool Binary => false;

        public string Name { get; }

        public VariableToken(string name)
        {
            Name = name;
        }

        public double Calculate(Dictionary<string, double> variables)
        {
            return variables[Name];
        }

        public VariableToken Build()
        {
            return this;
        }
    }
}
