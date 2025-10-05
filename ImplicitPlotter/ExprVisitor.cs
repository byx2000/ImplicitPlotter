namespace ImplicitPlotter
{
    public interface IExprVisitor<R>
    {
        R Visit(Const e);
        R Visit(XVar e);
        R Visit(YVar e);
        R Visit(UnaryExpr e);
        R Visit(BinaryExpr e);
        R Visit(PowN e);
        R Visit(RootN e);
    }

    public class ConstFoldVisitor : IExprVisitor<Expr>
    {
        public static readonly ConstFoldVisitor Instance = new ConstFoldVisitor();

        private ConstFoldVisitor() {}

        public Expr Visit(Const e)
        {
            return e;
        }

        public Expr Visit(XVar e)
        {
            return e;
        }

        public Expr Visit(YVar e)
        {
            return e;
        }

        public Expr Visit(UnaryExpr e)
        {
            Expr expr = e.Expr.Visit(this);
            if (expr is Const c)
            {
                return new Const(e.DoEval(c.Val));
            }
            else
            {
                return e.Op switch
                {
                    UnaryOp.Sqrt => Expr.Sqrt(expr),
                    UnaryOp.Exp => Expr.Exp(expr),
                    UnaryOp.Cos => Expr.Cos(expr),
                    UnaryOp.Sin => Expr.Sin(expr),
                    UnaryOp.Abs => Expr.Abs(expr),
                    UnaryOp.Ln => Expr.Ln(expr),
                    UnaryOp.Arcsin => Expr.Arcsin(expr),
                    UnaryOp.Arccos => Expr.Arccos(expr),
                    UnaryOp.Arctan => Expr.Arctan(expr),
                    UnaryOp.Sinh => Expr.Sinh(expr),
                    UnaryOp.Cosh => Expr.Cosh(expr),
                    UnaryOp.Tanh => Expr.Tanh(expr),
                    UnaryOp.Floor => Expr.Floor(expr),
                    UnaryOp.Ceil => Expr.Ceil(expr),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        public Expr Visit(BinaryExpr e)
        {
            Expr lhs = e.Lhs.Visit(this);
            Expr rhs = e.Rhs.Visit(this);

            if (lhs is Const c1 && rhs is Const c2)
            {
                if (e.Op == BinaryOp.Root && MathUtils.IsInteger(c2.Val))
                {
                    return new Const(MathUtils.Root(c1.Val, (int)c2.Val));
                }
                else
                {
                    return new Const(e.DoEval(c1.Val, c2.Val));
                }
            }
            else
            {
                return e.Op switch
                {
                    BinaryOp.Add => lhs.Add(rhs),
                    BinaryOp.Sub => lhs.Sub(rhs),
                    BinaryOp.Mul => lhs.Mul(rhs),
                    BinaryOp.Div => lhs.Div(rhs),
                    BinaryOp.Pow => lhs.Pow(rhs),
                    BinaryOp.Root => lhs.Root(rhs),
                    BinaryOp.Max => Expr.Max(lhs, rhs),
                    BinaryOp.Min => Expr.Min(lhs, rhs),
                    BinaryOp.Angle => Expr.Angle(lhs, rhs),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        public Expr Visit(PowN e)
        {
            Expr expr = e.Expr.Visit(this);
            if (expr is Const c)
            {
                return new Const(MathUtils.FastPow(c.Val, e.N));
            }
            else
            {
                return expr.Pow(e.N);
            }
        }

        public Expr Visit(RootN e)
        {
            Expr expr = e.Expr.Visit(this);
            if (expr is Const c)
            {
                return new Const(MathUtils.Root(c.Val, e.N));
            }
            else
            {
                return expr.Root(e.N);
            }
        }
    }

    public class PowRootVisitor : IExprVisitor<Expr>
    {
        public static readonly PowRootVisitor Instance = new PowRootVisitor();

        private PowRootVisitor() {}

        public Expr Visit(Const e)
        {
            return e;
        }

        public Expr Visit(XVar e)
        {
            return e;
        }

        public Expr Visit(YVar e)
        {
            return e;
        }

        public Expr Visit(UnaryExpr e)
        {
            Expr expr = e.Expr.Visit(this);
            return e.Op switch
            {
                UnaryOp.Sqrt => Expr.Sqrt(expr),
                UnaryOp.Exp => Expr.Exp(expr),
                UnaryOp.Cos => Expr.Cos(expr),
                UnaryOp.Sin => Expr.Sin(expr),
                UnaryOp.Abs => Expr.Abs(expr),
                UnaryOp.Ln => Expr.Ln(expr),
                UnaryOp.Arcsin => Expr.Arcsin(expr),
                UnaryOp.Arccos => Expr.Arccos(expr),
                UnaryOp.Arctan => Expr.Arctan(expr),
                UnaryOp.Sinh => Expr.Sinh(expr),
                UnaryOp.Cosh => Expr.Cosh(expr),
                UnaryOp.Tanh => Expr.Tanh(expr),
                UnaryOp.Floor => Expr.Floor(expr),
                UnaryOp.Ceil => Expr.Ceil(expr),
                _ => throw new NotImplementedException(),
            };
        }

        public Expr Visit(BinaryExpr e)
        {
            Expr lhs = e.Lhs.Visit(this);
            Expr rhs = e.Rhs.Visit(this);
            
            switch (e.Op)
            {
                case BinaryOp.Add:
                    return lhs.Add(rhs);
                case BinaryOp.Sub:
                    return lhs.Sub(rhs);
                case BinaryOp.Mul:
                    return lhs.Mul(rhs);
                case BinaryOp.Div:
                    return lhs.Div(rhs);
                case BinaryOp.Pow:
                    if (rhs is Const c && MathUtils.IsInteger(c.Val))
                    {
                        return lhs.Pow((int)c.Val);
                    }
                    else
                    {
                        return lhs.Pow(rhs);
                    }
                case BinaryOp.Root:
                    if (rhs is Const c2 && MathUtils.IsInteger(c2.Val))
                    {
                        return lhs.Root((int)c2.Val);
                    }
                    else
                    {
                        return lhs.Root(rhs);
                    }
                case BinaryOp.Max:
                    return Expr.Max(lhs, rhs);
                case BinaryOp.Min:
                    return Expr.Min(lhs, rhs);
                case BinaryOp.Angle:
                    return Expr.Angle(lhs, rhs);
                default:
                    throw new NotImplementedException();
            }
        }

        public Expr Visit(PowN e)
        {
            return e;
        }

        public Expr Visit(RootN e)
        {
            return e;
        }
    }
}
