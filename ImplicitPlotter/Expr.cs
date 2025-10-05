namespace ImplicitPlotter
{
    public abstract class Expr
    {
        public abstract double Eval(double x, double y);
        public abstract IntervalSet Eval(in IntervalSet x, in IntervalSet y);

        public abstract R Visit<R>(IExprVisitor<R> visitor);

        public static readonly Expr ZERO = new Const(0);
        public static readonly Expr ONE = new Const(1);
        public static readonly Expr PI = new Const(Math.PI);
        public static readonly Expr E = new Const(Math.E);

        public static Expr C(double val) => new Const(val);
        public static Expr X => XVar.INSTANCE;
        public static Expr Y => YVar.INSTANCE;
        public static Expr Neg(Expr e) => ZERO.Sub(e);
        public static Expr Sqrt(Expr e) => new UnaryExpr(UnaryOp.Sqrt, e);
        public static Expr Exp(Expr e) => new UnaryExpr(UnaryOp.Exp, e);
        public static Expr Cos(Expr e) => new UnaryExpr(UnaryOp.Cos, e);
        public static Expr Sin(Expr e) => new UnaryExpr(UnaryOp.Sin, e);
        public static Expr Tan(Expr e) => Sin(e).Div(Cos(e));
        public static Expr Abs(Expr e) => new UnaryExpr(UnaryOp.Abs, e);
        public static Expr Ln(Expr e) => new UnaryExpr(UnaryOp.Ln, e);
        public static Expr Log(Expr e) => Ln(e).Div(Math.Log(10));
        public static Expr Arcsin(Expr e) => new UnaryExpr(UnaryOp.Arcsin, e);
        public static Expr Arccos(Expr e) => new UnaryExpr(UnaryOp.Arccos, e);
        public static Expr Arctan(Expr e) => new UnaryExpr(UnaryOp.Arctan, e);
        public static Expr Sinh(Expr e) => new UnaryExpr(UnaryOp.Sinh, e);
        public static Expr Cosh(Expr e) => new UnaryExpr(UnaryOp.Cosh, e);
        public static Expr Tanh(Expr e) => new UnaryExpr(UnaryOp.Tanh, e);
        public static Expr Floor(Expr e) => new UnaryExpr(UnaryOp.Floor, e);
        public static Expr Ceil(Expr e) => new UnaryExpr(UnaryOp.Ceil, e);

        public Expr Add(Expr rhs) => new BinaryExpr(BinaryOp.Add, this, rhs);
        public Expr Add(double val) => Add(new Const(val));
        public Expr Sub(Expr rhs) => new BinaryExpr(BinaryOp.Sub, this, rhs);
        public Expr Sub(double val) => Sub(new Const(val));
        public Expr Mul(Expr rhs) => new BinaryExpr(BinaryOp.Mul, this, rhs);
        public Expr Mul(double val) => Mul(new Const(val));
        public Expr Div(Expr rhs) => new BinaryExpr(BinaryOp.Div, this, rhs);
        public Expr Div(double val) => Div(new Const(val));
        public Expr Pow(int n) => n >= 0 ? new PowN(this, n) : ONE.Div(new PowN(this, -n));
        public Expr Pow(Expr rhs) => new BinaryExpr(BinaryOp.Pow, this, rhs);
        public Expr Root(int n) => new RootN(this, n);
        public Expr Root(Expr rhs) => new BinaryExpr(BinaryOp.Root, this, rhs);

        public static Expr Max(Expr lhs, Expr rhs) => new BinaryExpr(BinaryOp.Max, lhs, rhs);
        public static Expr Min(Expr lhs, Expr rhs) => new BinaryExpr(BinaryOp.Min, lhs, rhs);
        public static Expr Angle(Expr lhs, Expr rhs) => new BinaryExpr(BinaryOp.Angle, lhs, rhs);

        public static Expr RADIUS_X_Y = Sqrt(X.Pow(2).Add(Y.Pow(2)));
        public static Expr ANGLE_X_Y = Angle(X, Y);
    }

    public class Const : Expr
    {
        public double Val { get; }
        private readonly IntervalSet intervalSet;

        public Const(double val)
        {
            Val = val;
            if (double.IsNaN(val))
            {
                intervalSet = IntervalSet.EMPTY;
            }
            else
            {
                intervalSet = new IntervalSet([new Interval(val)]);
            }
        }

        public override double Eval(double x, double y) => Val;
        public override IntervalSet Eval(in IntervalSet x, in IntervalSet y) => intervalSet;

        public override R Visit<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class XVar : Expr
    {
        public static readonly XVar INSTANCE = new XVar();

        private XVar() {}

        public override double Eval(double x, double y) => x;
        public override IntervalSet Eval(in IntervalSet x, in IntervalSet y) => x;

        public override R Visit<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class YVar : Expr
    {
        public static readonly YVar INSTANCE = new YVar();

        private YVar() {}

        public override double Eval(double x, double y) => y;
        public override IntervalSet Eval(in IntervalSet x, in IntervalSet y) => y;

        public override R Visit<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public enum BinaryOp
    {
        Add, Sub, Mul, Div, Pow, Root, Max, Min, Angle
    }

    public class BinaryExpr(BinaryOp op, Expr lhs, Expr rhs) : Expr
    {
        public BinaryOp Op { get; } = op;
        public Expr Lhs { get; } = lhs;
        public Expr Rhs { get; } = rhs;

        public override double Eval(double x, double y) => DoEval(Lhs.Eval(x, y), Rhs.Eval(x, y));
        public override IntervalSet Eval(in IntervalSet x, in IntervalSet y) => DoEval(Lhs.Eval(x, y), Rhs.Eval(x, y));

        public override R Visit<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public double DoEval(double x, double y)
        {
            return Op switch
            {
                BinaryOp.Add => x + y,
                BinaryOp.Sub => x - y,
                BinaryOp.Mul => x * y,
                BinaryOp.Div => x / y,
                BinaryOp.Pow => Math.Pow(x, y),
                BinaryOp.Root => Math.Pow(x, 1 / y),
                BinaryOp.Max => Math.Max(x, y),
                BinaryOp.Min => Math.Min(x, y),
                BinaryOp.Angle => MathUtils.Angle(x, y),
                _ => throw new NotImplementedException(),
            };
        }

        public IntervalSet DoEval(in IntervalSet x, in IntervalSet y)
        {
            return Op switch
            {
                BinaryOp.Add => x.Add(y),
                BinaryOp.Sub => x.Sub(y),
                BinaryOp.Mul => x.Mul(y),
                BinaryOp.Div => x.Div(y),
                BinaryOp.Pow => x.Pow(y),
                BinaryOp.Root => x.Root(y),
                BinaryOp.Max => IntervalSet.Max(x, y),
                BinaryOp.Min => IntervalSet.Min(x, y),
                BinaryOp.Angle => IntervalSet.Angle(x, y),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public enum UnaryOp
    {
        Sqrt, Exp, Cos, Sin, Abs, Ln, Arcsin, Arccos, Arctan, Sinh, Cosh, Tanh, Floor, Ceil
    }

    public class UnaryExpr(UnaryOp op, Expr e) : Expr
    {
        public UnaryOp Op { get; } = op;
        public Expr Expr { get; } = e;

        public override double Eval(double x, double y) => DoEval(Expr.Eval(x, y));
        public override IntervalSet Eval(in IntervalSet x, in IntervalSet y) => DoEval(Expr.Eval(x, y));

        public override R Visit<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }

        public double DoEval(double x)
        {
            return Op switch
            {
                UnaryOp.Sqrt => Math.Sqrt(x),
                UnaryOp.Exp => Math.Exp(x),
                UnaryOp.Cos => Math.Cos(x),
                UnaryOp.Sin => Math.Sin(x),
                UnaryOp.Abs => Math.Abs(x),
                UnaryOp.Ln => Math.Log(x),
                UnaryOp.Arcsin => Math.Asin(x),
                UnaryOp.Arccos => Math.Acos(x),
                UnaryOp.Arctan => Math.Atan(x),
                UnaryOp.Sinh => Math.Sinh(x),
                UnaryOp.Cosh => Math.Cosh(x),
                UnaryOp.Tanh => Math.Tanh(x),
                UnaryOp.Floor => Math.Floor(x),
                UnaryOp.Ceil => Math.Ceiling(x),
                _ => throw new NotImplementedException(),
            };
        }

        public IntervalSet DoEval(in IntervalSet x)
        {
            return Op switch
            {
                UnaryOp.Sqrt => IntervalSet.Sqrt(x),
                UnaryOp.Exp => IntervalSet.Exp(x),
                UnaryOp.Cos => IntervalSet.Cos(x),
                UnaryOp.Sin => IntervalSet.Sin(x),
                UnaryOp.Abs => IntervalSet.Abs(x),
                UnaryOp.Ln => IntervalSet.Ln(x),
                UnaryOp.Arcsin => IntervalSet.Arcsin(x),
                UnaryOp.Arccos => IntervalSet.Arccos(x),
                UnaryOp.Arctan => IntervalSet.Arctan(x),
                UnaryOp.Sinh => IntervalSet.Sinh(x),
                UnaryOp.Cosh => IntervalSet.Cosh(x),
                UnaryOp.Tanh => IntervalSet.Tanh(x),
                UnaryOp.Floor => IntervalSet.Floor(x),
                UnaryOp.Ceil => IntervalSet.Ceil(x),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public class PowN(Expr e, int n) : Expr
    {
        public Expr Expr { get; } = e;
        public int N { get; } = n;

        public override double Eval(double x, double y) => MathUtils.FastPow(Expr.Eval(x, y), N);
        public override IntervalSet Eval(in IntervalSet x, in IntervalSet y) => Expr.Eval(x, y).Pow(N);

        public override R Visit<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class RootN(Expr e, int n) : Expr
    {
        public Expr Expr { get; } = e;
        public int N { get; } = n;

        public override double Eval(double x, double y) => MathUtils.Root(Expr.Eval(x, y), N);
        public override IntervalSet Eval(in IntervalSet x, in IntervalSet y) => Expr.Eval(x, y).Root(N);

        public override R Visit<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
