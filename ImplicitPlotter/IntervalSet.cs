namespace ImplicitPlotter
{
    public readonly struct IntervalSet(List<Interval> intervals)
    {
        public static readonly IntervalSet EMPTY = new IntervalSet([]);
        public static readonly IntervalSet ONE = new IntervalSet([Interval.ONE]);

        public List<Interval> Intervals => intervals;

        public bool DefAndCont
        {
            get
            {
                if (Intervals.Count != 1)
                {
                    return false;
                }
                return Intervals[0].Def && Intervals[0].Cont;
            }
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", Intervals.Select(i => i.ToString()).ToList()) + "]";
        }

        public bool Includes(double val)
        {
            foreach (Interval i in Intervals)
            {
                if (i.Includes(val))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsEmpty() => Intervals.Count == 0;

        public TriBool GreaterThan(in IntervalSet rhs) => CompareOp(rhs, (a, b) => a.GreaterThan(b));

        public TriBool GreaterEqualThan(in IntervalSet rhs) => CompareOp(rhs, (a, b) => a.GreaterEqualThan(b));

        public TriBool LessThan(in IntervalSet rhs) => CompareOp(rhs, (a, b) => a.LessThan(b));

        public TriBool LessEqualThan(in IntervalSet rhs) => CompareOp(rhs, (a, b) => a.LessEqualThan(b));

        public TriBool Equal(in IntervalSet rhs) => CompareOp(rhs, (a, b) => a.Equal(b));

        public IntervalSet Add(in IntervalSet rhs) => BinaryOp(rhs, (a, b) => a.Add(b));

        public IntervalSet Sub(in IntervalSet rhs) => BinaryOp(rhs, (a, b) => a.Sub(b));

        public IntervalSet Mul(in IntervalSet rhs) => BinaryOp(rhs, (a, b) => a.Mul(b));

        public IntervalSet Div(in IntervalSet rhs)
        {
            if (IsEmpty() || rhs.IsEmpty())
            {
                return EMPTY;
            }

            List<Interval> intervals = new List<Interval>(Intervals.Count * rhs.Intervals.Count);
            foreach (Interval i1 in Intervals)
            {
                if (i1.IsEmpty())
                {
                    continue;
                }
                foreach (Interval i2 in rhs.Intervals)
                {
                    if (i2.IsEmpty())
                    {
                        continue;
                    }

                    if (i2.Lo > 0 || i2.Hi < 0)
                    {
                        double a = i1.Lo / i2.Lo;
                        double b = i1.Lo / i2.Hi;
                        double c = i1.Hi / i2.Lo;
                        double d = i1.Hi / i2.Hi;
                        intervals.Add(new Interval(Math.Min(Math.Min(a, b), Math.Min(c, d)), Math.Max(Math.Max(a, b), Math.Max(c, d)), i1.Def && i2.Def, i1.Cont && i2.Cont));
                    }
                    else if (i2.Lo == 0)
                    {
                        if (i2.Hi == 0)
                        {
                            continue;
                        }
                        else if (i1.Lo > 0)
                        {
                            intervals.Add(new Interval(i1.Lo / i2.Hi, double.PositiveInfinity, false, i1.Cont && i2.Cont));
                        }
                        else if (i1.Hi < 0)
                        {
                            intervals.Add(new Interval(double.NegativeInfinity, i1.Hi / i2.Hi, false, i1.Cont && i2.Cont));
                        }
                        else if (i1.Lo == 0)
                        {
                            intervals.Add(new Interval(0, double.PositiveInfinity, false, i1.Cont && i2.Cont));
                        }
                        else if (i1.Hi == 0)
                        {
                            intervals.Add(new Interval(double.NegativeInfinity, 0, false, i1.Cont && i2.Cont));
                        }
                        else
                        {
                            return new IntervalSet([new Interval(double.NegativeInfinity, double.PositiveInfinity, false, i1.Cont && i2.Cont)]);
                        }
                    }
                    else if (i2.Hi == 0)
                    {
                        if (i1.Lo > 0)
                        {
                            intervals.Add(new Interval(double.NegativeInfinity, i1.Lo / i2.Lo, false, i1.Cont && i2.Cont));
                        }
                        else if (i1.Hi < 0)
                        {
                            intervals.Add(new Interval(i1.Hi / i2.Lo, double.PositiveInfinity, false, i1.Cont && i2.Cont));
                        }
                        else if (i1.Lo == 0)
                        {
                            intervals.Add(new Interval(double.NegativeInfinity, 0, false, i1.Cont && i2.Cont));
                        }
                        else if (i1.Hi == 0)
                        {
                            intervals.Add(new Interval(0, double.PositiveInfinity, false, i1.Cont && i2.Cont));
                        }
                        else
                        {
                            return new IntervalSet([new Interval(double.NegativeInfinity, double.PositiveInfinity, false, i1.Cont && i2.Cont)]);
                        }
                    }
                    else
                    {
                        if (i1.Lo > 0)
                        {
                            intervals.Add(new Interval(double.NegativeInfinity, i1.Lo / i2.Lo, false, i1.Cont && i2.Cont));
                            intervals.Add(new Interval(i1.Lo / i2.Hi, double.PositiveInfinity, false, i1.Cont && i2.Cont));
                        }
                        else if (i1.Hi < 0)
                        {
                            intervals.Add(new Interval(double.NegativeInfinity, i1.Hi / i2.Hi, false, i1.Cont && i2.Cont));
                            intervals.Add(new Interval(i1.Hi / i2.Lo, double.PositiveInfinity, false, i1.Cont && i2.Cont));
                        }
                        else
                        {
                            return new IntervalSet([new Interval(double.NegativeInfinity, double.PositiveInfinity, false, i1.Cont && i2.Cont)]);
                        }
                    }
                }
            }
            return new IntervalSet(intervals);
        }

        public IntervalSet Pow(in IntervalSet rhs) => Exp(rhs.Mul(Ln(this)));

        public IntervalSet Pow(int n) => UnaryOp(x => x.Pow(n));

        public IntervalSet Root(in IntervalSet rhs) => Pow(ONE.Div(rhs));

        public IntervalSet Root(int n) => UnaryOp(x => x.Root(n));

        public static IntervalSet Exp(in IntervalSet i) => i.UnaryOp(x => Interval.Exp(x));

        public static IntervalSet Cos(in IntervalSet i) => i.UnaryOp(x => Interval.Cos(x));

        public static IntervalSet Sin(in IntervalSet i) => i.UnaryOp(x => Interval.Sin(x));

        public static IntervalSet Abs(in IntervalSet i) => i.UnaryOp(x => Interval.Abs(x));

        public static IntervalSet Sqrt(in IntervalSet i) => i.UnaryOp(x => Interval.Sqrt(x));

        public static IntervalSet Ln(in IntervalSet i) => i.UnaryOp(x => Interval.Ln(x));

        public static IntervalSet Arcsin(in IntervalSet i) => i.UnaryOp(x => Interval.Arcsin(x));

        public static IntervalSet Arccos(in IntervalSet i) => i.UnaryOp(x => Interval.Arccos(x));

        public static IntervalSet Arctan(in IntervalSet i) => i.UnaryOp(x => Interval.Arctan(x));

        public static IntervalSet Sinh(in IntervalSet i) => i.UnaryOp(x => Interval.Sinh(x));

        public static IntervalSet Cosh(in IntervalSet i) => i.UnaryOp(x => Interval.Cosh(x));

        public static IntervalSet Tanh(in IntervalSet i) => i.UnaryOp(x => Interval.Tanh(x));

        public static IntervalSet Max(in IntervalSet lhs, in IntervalSet rhs) => lhs.BinaryOp(rhs, (a, b) => Interval.Max(a, b));

        public static IntervalSet Min(in IntervalSet lhs, in IntervalSet rhs) => lhs.BinaryOp(rhs, (a, b) => Interval.Min(a, b));

        public static IntervalSet Angle(in IntervalSet lhs, in IntervalSet rhs)
        {
            if (lhs.IsEmpty() || rhs.IsEmpty())
            {
                return EMPTY;
            }

            List<Interval> intervals = new List<Interval>(lhs.Intervals.Count * rhs.Intervals.Count);
            foreach (Interval a in lhs.Intervals)
            {
                if (a.IsEmpty())
                {
                    continue;
                }
                foreach (Interval b in rhs.Intervals)
                {
                    if (b.IsEmpty())
                    {
                        continue;
                    }

                    if (a.Hi > 0 && b.Lo < 0 && b.Hi >= 0)
                    {
                        if (b.Hi == 0)
                        {
                            if (a.Lo < 0)
                            {
                                intervals.Add(new Interval(Math.PI, 2 * Math.PI, a.Def && b.Def, a.Cont && b.Cont));
                            }
                            else // a.Lo >= 0
                            {
                                intervals.Add(new Interval(MathUtils.Angle(a.Lo, b.Lo), 2 * Math.PI, a.Def && b.Def, a.Cont && b.Cont));
                            }
                        }
                        else // b.Hi > 0
                        {
                            if (a.Lo < 0)
                            {
                                intervals.Add(new Interval(0, 2 * Math.PI, a.Def && b.Def, false));
                            }
                            else // a.Lo >= 0
                            {
                                intervals.Add(new Interval(0, MathUtils.Angle(a.Lo, b.Hi), a.Def && b.Def, a.Cont && b.Cont));
                                intervals.Add(new Interval(MathUtils.Angle(a.Lo, b.Lo), 2 * Math.PI, a.Def && b.Def, a.Cont && b.Cont));
                            }
                        }
                    }
                    else
                    {
                        double m = MathUtils.Angle(a.Lo, b.Lo);
                        double n = MathUtils.Angle(a.Lo, b.Hi);
                        double p = MathUtils.Angle(a.Hi, b.Lo);
                        double q = MathUtils.Angle(a.Hi, b.Hi);

                        intervals.Add(new Interval(
                            Math.Min(Math.Min(m, n), Math.Min(p, q)),
                            Math.Max(Math.Max(m, n), Math.Max(p, q)),
                            a.Def && b.Def,
                            a.Cont && b.Cont
                        ));
                    }
                }
            }
            return new IntervalSet(intervals);
        }

        public static IntervalSet Floor(in IntervalSet i)
        {
            if (i.IsEmpty())
            {
                return EMPTY;
            }

            List<Interval> intervals = new List<Interval>(i.Intervals.Count);
            foreach (Interval ii in i.Intervals)
            {
                if (ii.IsEmpty())
                {
                    continue;
                }
                double a = Math.Floor(ii.Lo);
                double b = Math.Floor(ii.Hi);
                if (a == b)
                {
                    intervals.Add(new Interval(a, b, ii.Def, ii.Cont));
                }
                else if (a + 1 == b)
                {
                    intervals.Add(new Interval(a, a, ii.Def, false));
                    intervals.Add(new Interval(b, b, ii.Def, false));
                }
                else
                {
                    intervals.Add(new Interval(a, b, ii.Def, false));
                }
            }
            return new IntervalSet(intervals);
        }

        public static IntervalSet Ceil(in IntervalSet i)
        {
            if (i.IsEmpty())
            {
                return EMPTY;
            }

            List<Interval> intervals = new List<Interval>(i.Intervals.Count);
            foreach (Interval ii in i.Intervals)
            {
                if (ii.IsEmpty())
                {
                    continue;
                }
                double a = Math.Ceiling(ii.Lo);
                double b = Math.Ceiling(ii.Hi);
                if (a == b)
                {
                    intervals.Add(new Interval(a, b, ii.Def, ii.Cont));
                }
                else if (a + 1 == b)
                {
                    intervals.Add(new Interval(a, a, ii.Def, false));
                    intervals.Add(new Interval(b, b, ii.Def, false));
                }
                else
                {
                    intervals.Add(new Interval(a, b, ii.Def, false));
                }
            }
            return new IntervalSet(intervals);
        }

        private IntervalSet BinaryOp(in IntervalSet rhs, Func<Interval, Interval, Interval> opFunc)
        {
            if (IsEmpty() || rhs.IsEmpty())
            {
                return EMPTY;
            }

            List<Interval> intervals = new List<Interval>(Intervals.Count * rhs.Intervals.Count);
            foreach (Interval i1 in Intervals)
            {
                foreach (Interval i2 in rhs.Intervals)
                {
                    Interval i = opFunc(i1, i2);
                    if (!i.IsEmpty())
                    {
                        intervals.Add(i);
                    }
                }
            }
            return new IntervalSet(intervals);
        }

        private IntervalSet UnaryOp(Func<Interval, Interval> opFunc)
        {
            if (IsEmpty())
            {
                return EMPTY;
            }

            List<Interval> intervals = new List<Interval>(Intervals.Count);
            foreach (Interval i in Intervals)
            {
                Interval i2 = opFunc(i);
                if (!i2.IsEmpty())
                {
                    intervals.Add(i2);
                }
            }
            return new IntervalSet(intervals);
        }

        private TriBool CompareOp(in IntervalSet rhs, Func<Interval, Interval, TriBool> opFunc)
        {
            if (IsEmpty() || rhs.IsEmpty())
            {
                return TriBool.False;
            }

            bool allTrue = true;
            bool allFalse = true;

            foreach (Interval a in Intervals)
            {
                foreach (Interval b in rhs.Intervals)
                {
                    switch (opFunc(a, b))
                    {
                        case TriBool.True:
                            allFalse = false;
                            break;
                        case TriBool.False:
                            allTrue = false;
                            break;
                        case TriBool.Maybe:
                            return TriBool.Maybe;
                    }

                    if (!allTrue && !allFalse)
                    {
                        return TriBool.Maybe;
                    }
                }
            }

            if (allTrue)
            {
                return TriBool.True;
            }
            else if (allFalse)
            {
                return TriBool.False;
            }
            else
            {
                return TriBool.Maybe;
            }
        }
    }
}
