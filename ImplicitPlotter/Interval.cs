namespace ImplicitPlotter
{
    public readonly struct Interval(double lo, double hi, bool def, bool cont)
    {
        public static readonly Interval EMPTY = new Interval(double.PositiveInfinity, double.NegativeInfinity, false, false);
        public static readonly Interval ONE = new Interval(1);

        private const double PI_TWICE = Math.PI * 2;
        private const double PI_HALF = Math.PI / 2;

        public double Lo => lo;
        public double Hi => hi;
        public bool Def => def;
        public bool Cont => cont;

        public Interval(double lo, double hi) : this(lo, hi, true, true) {}
        public Interval(double val) : this(val, val, true, true) {}

        public override string ToString() => $"[{Lo}, {Hi}, {Def}, {Cont}]";

        public bool IsEmpty() => Lo > Hi;

        public bool Includes(double val) => Lo <= val && val <= Hi;

        public double Mid => (Lo + Hi) / 2;

        public double Length => Hi - Lo;

        public TriBool GreaterThan(in Interval rhs)
        {
            if (IsEmpty() || rhs.IsEmpty())
            {
                return TriBool.False;
            }
            if (Lo > rhs.Hi)
            {
                return Def && rhs.Def && Cont && rhs.Cont ? TriBool.True : TriBool.Maybe;
            }
            else if (Hi < rhs.Lo)
            {
                return TriBool.False;
            }
            else
            {
                return TriBool.Maybe;
            }
        }

        public TriBool GreaterEqualThan(in Interval rhs)
        {
            if (IsEmpty() || rhs.IsEmpty())
            {
                return TriBool.False;
            }
            if (Lo >= rhs.Hi)
            {
                return Def && rhs.Def && Cont && rhs.Cont ? TriBool.True : TriBool.Maybe;
            }
            else if (Hi < rhs.Lo)
            {
                return TriBool.False;
            }
            else
            {
                return TriBool.Maybe;
            }
        }

        public TriBool LessThan(in Interval rhs)
        {
            if (IsEmpty() || rhs.IsEmpty())
            {
                return TriBool.False;
            }
            if (Hi < rhs.Lo)
            {
                return Def && rhs.Def && Cont && rhs.Cont ? TriBool.True : TriBool.Maybe;
            }
            else if (Lo > rhs.Hi)
            {
                return TriBool.False;
            }
            else
            {
                return TriBool.Maybe;
            }
        }

        public TriBool LessEqualThan(in Interval rhs)
        {
            if (IsEmpty() || rhs.IsEmpty())
            {
                return TriBool.False;
            }
            if (Hi <= rhs.Lo)
            {
                return Def && rhs.Def && Cont && rhs.Cont ? TriBool.True : TriBool.Maybe;
            }
            else if (Lo > rhs.Hi)
            {
                return TriBool.False;
            }
            else
            {
                return TriBool.Maybe;
            }
        }

        public TriBool Equal(in Interval rhs)
        {
            if (IsEmpty() || rhs.IsEmpty())
            {
                return TriBool.False;
            }
            if (Hi < rhs.Lo || Lo > rhs.Hi)
            {
                return TriBool.False;
            }
            else
            {
                return TriBool.Maybe;
            }
        }

        public Interval Add(in Interval rhs) => new Interval(Lo + rhs.Lo, Hi + rhs.Hi, Def && rhs.Def, Cont && rhs.Cont);

        public Interval Sub(in Interval rhs) => new Interval(Lo - rhs.Hi, Hi - rhs.Lo, Def && rhs.Def, Cont && rhs.Cont);

        public Interval Mul(in Interval rhs)
        {
            if (IsEmpty() || rhs.IsEmpty())
            {
                return EMPTY;
            }

            double a = Lo * rhs.Lo;
            double b = Lo * rhs.Hi;
            double c = Hi * rhs.Lo;
            double d = Hi * rhs.Hi;
            return new Interval(
                Math.Min(Math.Min(a, b), Math.Min(c, d)),
                Math.Max(Math.Max(a, b), Math.Max(c, d)),
                Def && rhs.Def,
                Cont && rhs.Cont
            );
        }

        // n >= 0
        public Interval Pow(int n)
        {
            if (IsEmpty())
            {
                return EMPTY;
            }

            if (n == 0)
            {
                if (Lo == 0 && Hi == 0)
                {
                    return EMPTY;
                }
                else
                {
                    return new Interval(1, 1, Def, Cont);
                }
            }
            else
            {
                if ((n & 1) == 1)
                {
                    return new Interval(MathUtils.FastPow(Lo, n), MathUtils.FastPow(Hi, n), Def, Cont);
                }
                else
                {
                    if (Lo >= 0 || Hi <= 0)
                    {
                        double a = MathUtils.FastPow(Lo, n);
                        double b = MathUtils.FastPow(Hi, n);
                        return new Interval(Math.Min(a, b), Math.Max(a, b), Def, Cont);
                    }
                    else
                    {
                        return new Interval(0, MathUtils.FastPow(Math.Max(-Lo, Hi), n), Def, Cont);
                    }
                }
            }
        }

        // n >= 0
        public Interval Root(int n)
        {
            if (IsEmpty() || n <= 0)
            {
                return EMPTY;
            }

            if ((n & 1) == 1)
            {
                return new Interval(MathUtils.Root(lo, n), MathUtils.Root(hi, n), Def, Cont);
            }
            else
            {
                if (hi < 0)
                {
                    return EMPTY;
                }
                else if (lo >= 0)
                {
                    return new Interval(Math.Pow(lo, 1.0 / n), Math.Pow(hi, 1.0 / n), Def, Cont);
                }
                else
                {
                    return new Interval(0, Math.Pow(hi, 1.0 / n), false, Cont);
                }
            }
        }

        public static Interval Exp(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }
            return new Interval(Math.Exp(x.Lo), Math.Exp(x.Hi), x.Def, x.Cont);
        }

        public static Interval Cos(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }

            if (x.Lo == double.NegativeInfinity || x.Hi == double.PositiveInfinity || x.Hi - x.Lo >= PI_TWICE)
            {
                return new Interval(-1, 1, x.Def, x.Cont);
            }

            double lo = x.Lo;
            double hi = x.Hi;

            if (x.Lo < 0)
            {
                double n = Math.Ceiling(-x.Lo / PI_TWICE);
                lo += PI_TWICE * n;
                hi += PI_TWICE * n;
            }
            else if (lo > 0)
            {
                double n = Math.Floor(x.Lo / PI_TWICE);
                lo -= PI_TWICE * n;
                hi -= PI_TWICE * n;
            }


            if (lo >= Math.PI)
            {
                Interval cosv = Cos(new Interval(lo - Math.PI, hi - Math.PI, x.Def, x.Cont));
                return new Interval(-cosv.Hi, -cosv.Lo, x.Def, x.Cont);
            }

            double rlo = Math.Cos(hi);
            double rhi = Math.Cos(lo);
            if (hi <= Math.PI)
            {
                return new Interval(rlo, rhi, x.Def, x.Cont);
            }
            else if (hi <= PI_TWICE)
            {
                return new Interval(-1, Math.Max(rlo, rhi), x.Def, x.Cont);
            }
            else
            {
                return new Interval(-1, 1, x.Def, x.Cont);
            }
        }

        public static Interval Sin(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }
            return Cos(new Interval(x.Lo - PI_HALF, x.Hi - PI_HALF, x.Def, x.Cont));
        }

        public static Interval Abs(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }

            if (x.Lo >= 0)
            {
                return new Interval(x.Lo, x.Hi, x.Def, x.Cont);
            }
            else if (x.Hi <= 0)
            {
                return new Interval(-x.Hi, -x.Lo, x.Def, x.Cont);
            }
            else
            {
                return new Interval(0, Math.Max(-x.Lo, x.Hi), x.Def, x.Cont);
            }
        }

        public static Interval Sqrt(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }

            if (x.Hi < 0)
            {
                return EMPTY;
            }

            if (x.Lo < 0)
            {
                return new Interval(0, Math.Sqrt(x.Hi), false, x.Cont);
            }
            else
            {
                return new Interval(Math.Sqrt(x.Lo), Math.Sqrt(x.Hi), x.Def, x.Cont);
            }
        }

        public static Interval Ln(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }

            if (x.Hi <= 0)
            {
                return EMPTY;
            }

            if (x.Lo <= 0)
            {
                return new Interval(double.NegativeInfinity, Math.Log(x.Hi), false, x.Cont);
            }
            else
            {
                return new Interval(Math.Log(x.Lo), Math.Log(x.Hi), x.Def, x.Cont);
            }
        }

        public static Interval Arcsin(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }

            if (x.Hi < -1 || x.Lo > 1)
            {
                return EMPTY;
            }

            double lo = Math.Max(-1, x.Lo);
            double hi = Math.Min(1, x.Hi);
            return new Interval(Math.Asin(lo), Math.Asin(hi), x.Def && x.Lo >= -1 && x.Hi <= 1, x.Cont);
        }

        public static Interval Arccos(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }

            if (x.Hi < -1 || x.Lo > 1)
            {
                return EMPTY;
            }

            double lo = Math.Max(-1, x.Lo);
            double hi = Math.Min(1, x.Hi);
            return new Interval(Math.Acos(hi), Math.Acos(lo), x.Def && x.Lo >= -1 && x.Hi <= 1, x.Cont);
        }

        public static Interval Arctan(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }
            return new Interval(Math.Atan(x.Lo), Math.Atan(x.Hi), x.Def, x.Cont);
        }

        public static Interval Sinh(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }
            return new Interval(Math.Sinh(x.Lo), Math.Sinh(x.Hi), x.Def, x.Cont);
        }

        public static Interval Cosh(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }

            if (x.Lo >= 0 || x.Hi <= 0)
            {
                double a = Math.Cosh(x.Lo);
                double b = Math.Cosh(x.Hi);
                return new Interval(Math.Min(a, b), Math.Max(a, b), x.Def, x.Cont);
            }
            else
            {
                return new Interval(1, Math.Cosh(Math.Max(-x.Lo, x.Hi)), x.Def, x.Cont);
            }
        }

        public static Interval Tanh(in Interval x)
        {
            if (x.IsEmpty())
            {
                return EMPTY;
            }
            return new Interval(Math.Tanh(x.Lo), Math.Tanh(x.Hi), x.Def, x.Cont);
        }

        public static Interval Max(in Interval a, in Interval b)
        {
            if (a.IsEmpty() || b.IsEmpty())
            {
                return EMPTY;
            }
            return new Interval(Math.Max(a.Lo, b.Lo), Math.Max(a.Hi, b.Hi), a.Def && b.Def, a.Cont && b.Cont);
        }

        public static Interval Min(in Interval a, in Interval b)
        {
            if (a.IsEmpty() || b.IsEmpty())
            {
                return EMPTY;
            }
            return new Interval(Math.Min(a.Lo, b.Lo), Math.Min(a.Hi, b.Hi), a.Def && b.Def, a.Cont && b.Cont);
        }
    }
}
