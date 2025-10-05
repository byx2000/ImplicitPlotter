namespace ImplicitPlotter
{
    public readonly struct Range(int left, int top, int right, int bottom, Interval xi, Interval yi)
    {
        public int Left => left;
        public int Top => top;
        public int Right => right;
        public int Bottom => bottom;
        public Interval Xi => xi;
        public Interval Yi => yi;
        public int Width => Right - Left + 1;
        public int Height => Bottom - Top + 1;
        public int PixelCount => (Right - Left + 1) * (Bottom - Top + 1);
        public bool IsMinimum => Left == Right && Top == Bottom;

        public Range[] Split()
        {
            double dx = (Xi.Hi - Xi.Lo) / (Right - Left + 1);
            double dy = (Yi.Hi - Yi.Lo) / (Bottom - Top + 1);
            int h = (Top + Bottom) / 2;
            int w = (Left + Right) / 2;
            double xMin = Xi.Lo;
            double xMid = Xi.Lo + (w - Left + 1) * dx;
            double xMax = Xi.Hi;
            double yMin = Yi.Lo;
            double yMid = Yi.Lo + (Bottom - h) * dy;
            double yMax = Yi.Hi;

            if (IsMinimum)
            {
                return [];
            }
            else if (Left == Right)
            {
                return [
                    new Range(Left, Top, Right, h, Xi, new Interval(yMid, yMax)),
                    new Range(Left, h + 1, Right, Bottom, Xi, new Interval(yMin, yMid))
                ];
            }
            else if (Top == Bottom)
            {
                return [
                    new Range(Left, Top, w, Bottom, new Interval(xMin, xMid), Yi),
                    new Range(w + 1, Top, Right, Bottom, new Interval(xMid, xMax), Yi)
                ];
            }
            else
            {
                return [
                    new Range(Left, Top, w, h, new Interval(xMin, xMid), new Interval(yMid, yMax)),
                    new Range(w + 1, Top, Right, h, new Interval(xMid, xMax), new Interval(yMid, yMax)),
                    new Range(Left, h + 1, w, Bottom, new Interval(xMin, xMid), new Interval(yMin, yMid)),
                    new Range(w + 1, h + 1, Right, Bottom, new Interval(xMid, xMax), new Interval(yMin, yMid))
                ];
            }
        }
    }
}
