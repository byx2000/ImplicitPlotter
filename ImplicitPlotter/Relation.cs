namespace ImplicitPlotter
{
    public abstract class Relation(Expr lhs, Expr rhs)
    {
        public Expr Lhs { get; } = lhs;
        public Expr Rhs { get; } = rhs;
        protected readonly Expr expr = lhs.Sub(rhs);

        public TriBool Eval(in Interval x, in Interval y)
        {
            IntervalSet xs = new IntervalSet([x]);
            IntervalSet ys = new IntervalSet([y]);
            IntervalSet i1 = Lhs.Eval(xs, ys);
            IntervalSet i2 = Rhs.Eval(xs, ys);
            return DoEval(i1, i2);
        }

        public abstract TriBool EvalInPixel(in Interval x, in Interval y);
        protected abstract TriBool DoEval(in IntervalSet x, in IntervalSet y);
    }

    public class LessThan(Expr lhs, Expr rhs) : Relation(lhs, rhs)
    {
        protected override TriBool DoEval(in IntervalSet x, in IntervalSet y) => x.LessThan(y);

        public override TriBool EvalInPixel(in Interval x, in Interval y)
        {
            TriBool r = Eval(x, y);
            if (r != TriBool.Maybe)
            {
                return r;
            }

            return Detect(x, y) ? TriBool.True : TriBool.Maybe;
        }

        private bool Detect(in Interval xi, in Interval yi)
        {
            double[] points =
            [
                xi.Lo, yi.Lo,
                xi.Lo, yi.Hi,
                xi.Hi, yi.Lo,
                xi.Hi, yi.Hi
            ];

            for (int i = 0; i < points.Length; i += 2)
            {
                double v = expr.Eval(points[i], points[i + 1]);

                if (v < 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class LessEqualThan(Expr lhs, Expr rhs) : Relation(lhs, rhs)
    {
        protected override TriBool DoEval(in IntervalSet x, in IntervalSet y) => x.LessEqualThan(y);

        public override TriBool EvalInPixel(in Interval x, in Interval y)
        {
            TriBool r = Eval(x, y);
            if (r != TriBool.Maybe)
            {
                return r;
            }

            return Detect(x, y) ? TriBool.True : TriBool.Maybe;
        }

        private bool Detect(in Interval xi, in Interval yi)
        {
            double[] points =
            [
                xi.Lo, yi.Lo,
                xi.Lo, yi.Hi,
                xi.Hi, yi.Lo,
                xi.Hi, yi.Hi
            ];

            for (int i = 0; i < points.Length; i += 2)
            {
                double v = expr.Eval(points[i], points[i + 1]);

                if (v <= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class GreaterThan(Expr lhs, Expr rhs) : Relation(lhs, rhs)
    {
        protected override TriBool DoEval(in IntervalSet x, in IntervalSet y) => x.GreaterThan(y);

        public override TriBool EvalInPixel(in Interval x, in Interval y)
        {
            TriBool r = Eval(x, y);
            if (r != TriBool.Maybe)
            {
                return r;
            }

            return Detect(x, y) ? TriBool.True : TriBool.Maybe;
        }

        private bool Detect(in Interval xi, in Interval yi)
        {
            double[] points =
            [
                xi.Lo, yi.Lo,
                xi.Lo, yi.Hi,
                xi.Hi, yi.Lo,
                xi.Hi, yi.Hi
            ];

            for (int i = 0; i < points.Length; i += 2)
            {
                double v = expr.Eval(points[i], points[i + 1]);

                if (v > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class GreaterEqualThan(Expr lhs, Expr rhs) : Relation(lhs, rhs)
    {
        protected override TriBool DoEval(in IntervalSet x, in IntervalSet y) => x.GreaterEqualThan(y);

        public override TriBool EvalInPixel(in Interval x, in Interval y)
        {
            TriBool r = Eval(x, y);
            if (r != TriBool.Maybe)
            {
                return r;
            }

            return Detect(x, y) ? TriBool.True : TriBool.Maybe;
        }

        private bool Detect(in Interval xi, in Interval yi)
        {
            double[] points =
            [
                xi.Lo, yi.Lo,
                xi.Lo, yi.Hi,
                xi.Hi, yi.Lo,
                xi.Hi, yi.Hi
            ];

            for (int i = 0; i < points.Length; i += 2)
            {
                double v = expr.Eval(points[i], points[i + 1]);

                if (v >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class Equal(Expr lhs, Expr rhs) : Relation(lhs, rhs)
    {
        protected override TriBool DoEval(in IntervalSet x, in IntervalSet y) => x.Equal(y);

        public override TriBool EvalInPixel(in Interval x, in Interval y)
        {
            IntervalSet xs = new IntervalSet([x]);
            IntervalSet ys = new IntervalSet([y]);
            IntervalSet i1 = Lhs.Eval(xs, ys);
            IntervalSet i2 = Rhs.Eval(xs, ys);
            TriBool r = DoEval(i1, i2);
            if (r != TriBool.Maybe)
            {
                return r;
            }

            if (i1.DefAndCont && i2.DefAndCont && HasZeroPoint(x, y))
            {
                return TriBool.True;
            }

            return TriBool.Maybe;
        }

        private bool HasZeroPoint(in Interval xi, in Interval yi)
        {
            double[] points =
            [
                xi.Lo, yi.Lo,
                xi.Lo, yi.Hi,
                xi.Hi, yi.Lo,
                xi.Hi, yi.Hi
            ];

            bool hasPositive = false;
            bool hasNegative = false;

            for (int i = 0; i < points.Length; i += 2)
            {
                double v = expr.Eval(points[i], points[i + 1]);

                if (v == 0)
                {
                    return true;
                }
                if (v > 0)
                {
                    hasPositive = true;
                }
                if (v < 0)
                {
                    hasNegative = true;
                }

                if (hasPositive && hasNegative)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
