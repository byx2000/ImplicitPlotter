using System.Drawing;

namespace ImplicitPlotter
{
    public class Plotter
    {
        public static void Plot(PlotConfig config)
        {
            Relation relation = RelationParser.Parse(config.Relation);
            Color drawColor = ColorTranslator.FromHtml(config.DrawColor);
            Color backgroundColor = ColorTranslator.FromHtml(config.BackgroundColor);
            Brush drawBrush = new SolidBrush(drawColor);
            Brush backgroundBrush = new SolidBrush(backgroundColor);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(config.Timeout);
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            using Bitmap bitmap = new Bitmap(config.Width, config.Height);
            Graphics g = Graphics.FromImage(bitmap);
            g.FillRectangle(backgroundBrush, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

            Queue<Range> ranges = new Queue<Range>();
            ranges.Enqueue(new Range(0, 0, config.Width - 1, config.Height - 1, new Interval(config.XMin, config.XMax), new Interval(config.YMin, config.YMax)));
            Queue<PixelTask> pixelTasks = new Queue<PixelTask>();

            while (ranges.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                Range range = ranges.Dequeue();
                if (range.IsMinimum)
                {
                    switch (relation.EvalInPixel(range.Xi, range.Yi))
                    {
                        case TriBool.True:
                            g.FillRectangle(drawBrush, new Rectangle(range.Left, range.Top, 1, 1));
                            break;
                        case TriBool.Maybe:
                            pixelTasks.Enqueue(new PixelTask(range));
                            break;
                    }
                }
                else
                {
                    switch (relation.Eval(range.Xi, range.Yi))
                    {
                        case TriBool.True:
                            g.FillRectangle(drawBrush, new Rectangle(range.Left, range.Top, range.Width, range.Height));
                            break;
                        case TriBool.Maybe:
                            foreach (Range r in range.Split())
                            {
                                ranges.Enqueue(r);
                            }
                            break;
                    }
                }
            }

            while (pixelTasks.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                PixelTask task = pixelTasks.Dequeue();
                switch (RefinePixel(relation, task))
                {
                    case TriBool.True:
                        g.FillRectangle(drawBrush, new Rectangle(task.Left, task.Top, 1, 1));
                        break;
                    case TriBool.Maybe:
                        pixelTasks.Enqueue(task);
                        break;
                }
            }

            bitmap.Save(config.Output);
        }

        private static TriBool RefinePixel(Relation relation, PixelTask task)
        {
            int blockSize = task.Blocks.Count;
            for (int i = 0; i < blockSize; i++)
            {
                foreach ((Interval xi, Interval yi) in SplitInterval(task.Blocks.Dequeue()))
                {
                    switch (relation.EvalInPixel(xi, yi))
                    {
                        case TriBool.True:
                            return TriBool.True;
                        case TriBool.Maybe:
                            task.Blocks.Enqueue((xi, yi));
                            break;
                    }
                }
            }

            if (task.Blocks.Count == 0 || task.Split >= 10)
            {
                return TriBool.False;
            }
            else
            {
                task.Split++;
                return TriBool.Maybe;
            }
        }

        private static List<(Interval, Interval)> SplitInterval((Interval, Interval) block)
        {
            (Interval xi, Interval yi) = block;
            double xMid = xi.Mid;
            double yMid = yi.Mid;
            return [
                (new Interval(xi.Lo, xMid), new Interval(yi.Lo, yMid)),
                (new Interval(xi.Lo, xMid), new Interval(yMid, yi.Hi)),
                (new Interval(xMid, xi.Hi), new Interval(yi.Lo, yMid)),
                (new Interval(xMid, xi.Hi), new Interval(yMid, yi.Hi)),
            ];
        }
    }

    public class PlotConfig
    {
        public required string Relation { get; set; }
        public required string Output { get; set; }
        public int Width { get; set; } = 500;
        public int Height { get; set; } = 500;
        public double XMin { get; set; } = -10;
        public double XMax { get; set; } = 10;
        public double YMin { get; set; } = -10;
        public double YMax { get; set; } = 10;
        public string DrawColor { get; set; } = "#C80078D7";
        public string BackgroundColor { get; set; } = "#FFFFFFFF";
        public int Timeout { get; set; } = 10000;
    }

    internal class PixelTask(Range range)
    {
        public int Left { get; } = range.Left;
        public int Top { get; } = range.Top;
        public Queue<(Interval, Interval)> Blocks { get; } = new Queue<(Interval, Interval)>([(range.Xi, range.Yi)]);
        public int Split { get; set; } = 1;
    }
}
