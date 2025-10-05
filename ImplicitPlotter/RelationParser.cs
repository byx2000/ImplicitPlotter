using System.Text;

namespace ImplicitPlotter
{
    public class RelationParser
    {
        private static readonly Dictionary<string, Expr> CONST_FUNCTIONS = new Dictionary<string, Expr>()
        {
            { "pi", Expr.PI },
            { "e", Expr.E },
            { "r", Expr.RADIUS_X_Y },
            { "a", Expr.ANGLE_X_Y },
        };

        private static readonly Dictionary<string, Func<Expr, Expr>> UNARY_FUNCTIONS = new Dictionary<string, Func<Expr, Expr>>
        {
            { "exp", e => Expr.Exp(e) },
            { "cosh", e => Expr.Cosh(e) },
            { "cos", e => Expr.Cos(e) },
            { "sinh", e => Expr.Sinh(e) },
            { "sin", e => Expr.Sin(e) },
            { "tanh", e => Expr.Tanh(e) },
            { "tan", e => Expr.Tan(e) },
            { "sqrt", e => Expr.Sqrt(e) },
            { "abs", e => Expr.Abs(e) },
            { "ln", e => Expr.Ln(e) },
            { "log", e => Expr.Log(e) },
            { "arcsin", e => Expr.Arcsin(e) },
            { "arccos", e => Expr.Arccos(e) },
            { "arctan", e => Expr.Arctan(e) },
            { "floor", e => Expr.Floor(e) },
            { "ceil", e => Expr.Ceil(e) },
        };

        private static readonly Dictionary<string, Func<Expr, Expr, Expr>> BINARY_FUNCTIONS = new Dictionary<string, Func<Expr, Expr, Expr>>
        {
            { "pow", (e1, e2) => e1.Pow(e2) },
            { "root", (e1, e2) => e1.Root(e2) },
            { "max", (e1, e2) => Expr.Max(e1, e2) },
            { "min", (e1, e2) => Expr.Min(e1, e2) },
        };

        public static Relation Parse(string s)
        {
            s = s.Replace(" ", "");
            int i = 0;
            Relation relation = ParseRelation(s, ref i);
            if (i != s.Length)
            {
                throw new Exception("unexpected end of input");
            }
            return relation;
        }

        // relation = expr ('>' | '<' | '=') expr
        private static Relation ParseRelation(string s, ref int i)
        {
            Expr lhs = ParseExpr(s, ref i);
            string sub = s.Substring(i);
            if (sub.StartsWith(">="))
            {
                i += 2;
                Expr rhs = ParseExpr(s, ref i);
                return new GreaterEqualThan(Simplify(lhs), Simplify(rhs));
            }
            else if (sub.StartsWith('>'))
            {
                i++;
                Expr rhs = ParseExpr(s, ref i);
                return new GreaterThan(Simplify(lhs), Simplify(rhs));
            }
            else if (sub.StartsWith("<="))
            {
                i += 2;
                Expr rhs = ParseExpr(s, ref i);
                return new LessEqualThan(Simplify(lhs), Simplify(rhs));
            }
            else if (sub.StartsWith('<'))
            {
                i++;
                Expr rhs = ParseExpr(s, ref i);
                return new LessThan(Simplify(lhs), Simplify(rhs));
            }
            else if (sub.StartsWith('='))
            {
                i++;
                Expr rhs = ParseExpr(s, ref i);
                return new Equal(Simplify(lhs), Simplify(rhs));
            }
            else
            {
                throw new Exception("relation operator missing");
            }
        }

        private static Expr Simplify(Expr e)
        {
            return e.Visit(ConstFoldVisitor.Instance).Visit(PowRootVisitor.Instance);
        }

        // expr = multiplicative ([+-] multiplicative)*
        private static Expr ParseExpr(string s, ref int i)
        {
            Expr e = ParseMultiplicative(s, ref i);
            while (i < s.Length && (s[i] == '+' || s[i] == '-'))
            {
                char op = s[i++];
                if (op == '+')
                {
                    e = e.Add(ParseMultiplicative(s, ref i));
                }
                else
                {
                    e = e.Sub(ParseMultiplicative(s, ref i));
                }
            }
            return e;
        }

        // multiplicative = exponential ([*/] exponential)*
        private static Expr ParseMultiplicative(string s, ref int i)
        {
            Expr e = ParseExponential(s, ref i);
            while (i < s.Length && (s[i] == '*' || s[i] == '/'))
            {
                char op = s[i++];
                if (op == '*')
                {
                    e = e.Mul(ParseExponential(s, ref i));
                }
                else
                {
                    e = e.Div(ParseExponential(s, ref i));
                }
            }
            return e;
        }

        // exponential = element (^element)*
        private static Expr ParseExponential(string s, ref int i)
        {
            List<Expr> exprs = [ParseElement(s, ref i)];
            while (i < s.Length && s[i] == '^')
            {
                i++;
                exprs.Add(ParseElement(s, ref i));
            }

            Expr e = exprs[exprs.Count - 1];
            for (int index = exprs.Count - 2; index >= 0; index--)
            {
                e = exprs[index].Pow(e);
            }
            return e;
        }

        // element = integer | decimal | x | y | (expr) | -element | func(expr)
        private static Expr ParseElement(string s, ref int i)
        {
            char first = s[i];
            if (first >= '0' && first <= '9')
            {
                StringBuilder sb = new StringBuilder();
                while (i < s.Length && (s[i] >= '0' && s[i] <= '9' || s[i] == '.'))
                {
                    sb.Append(s[i++]);
                }
                return Expr.C(double.Parse(sb.ToString()));
            }
            else if (first == 'x')
            {
                i++;
                return Expr.X;
            }
            else if (first == 'y')
            {
                i++;
                return Expr.Y;
            }
            else if (first == '(')
            {
                i++;
                Expr e = ParseExpr(s, ref i);
                i++; // ')'
                return e;
            }
            else if (first == '-')
            {
                i++;
                return Expr.Neg(ParseMultiplicative(s, ref i));
            }

            string temp = s.Substring(i);

            foreach (var item in UNARY_FUNCTIONS)
            {
                if (temp.StartsWith(item.Key))
                {
                    i += item.Key.Length + 1;
                    Expr e = item.Value(ParseExpr(s, ref i));
                    i++;
                    return e;
                }
            }

            foreach (var item in BINARY_FUNCTIONS)
            {
                if (temp.StartsWith(item.Key))
                {
                    i += item.Key.Length + 1;
                    Expr e1 = ParseExpr(s, ref i);
                    i++;
                    Expr e2 = ParseExpr(s, ref i);
                    i++;
                    return item.Value(e1, e2);
                }
            }

            foreach (var item in CONST_FUNCTIONS)
            {
                if (temp.StartsWith(item.Key))
                {
                    i += item.Key.Length;
                    return item.Value;
                }
            }

            throw new Exception("invalid expression: parseElement");
        }
    }
}
