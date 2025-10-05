namespace ImplicitPlotter
{
    public class MathUtils
    {
        public static double FastPow(double num, int exp)
        {
            double result = 1.0;
            while (exp > 0)
            {
                if ((exp & 1) == 1)
                    result *= num;
                exp >>= 1;
                num *= num;
            }

            return result;
        }

        public static bool IsInteger(double value)
        {
            return Math.Floor(value) == Math.Ceiling(value);
        }

        public static double Root(double x, int n)
        {
            if ((n & 1) == 1)
            {
                return x < 0 ? -Math.Pow(-x, 1.0 / n) : Math.Pow(x, 1.0 / n);
            }
            else
            {
                return Math.Pow(x, 1.0 / n);
            }
        }

        public static double Angle(double x, double y)
        {
            if (x == 0 && y == 0)
            {
                return 0;
            }

            if (y >= 0)
            {
                return Math.Atan2(y, x);
            }
            else
            {
                return Math.Atan2(y, x) + 2 * Math.PI;
            }
        }
    }
}