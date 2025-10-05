namespace ImplicitPlotter
{
    public enum TriBool
    {
        True, False, Maybe
    }

    public static class TriBoolExtensions
    {
        public static TriBool And(this TriBool lhs, TriBool rhs)
        {
            if (lhs == TriBool.True && rhs == TriBool.True)
            {
                return TriBool.True;
            }
            else if (lhs == TriBool.False && rhs == TriBool.False)
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
