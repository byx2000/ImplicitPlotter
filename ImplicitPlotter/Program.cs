using System.CommandLine;

namespace ImplicitPlotter
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Option<string> relationOption = new Option<string>("--relation")
            {
                Description = "Implicit function expression to be plotted.",
                Required = true,
            };
            Option<string> outputOption = new Option<string>("--output")
            {
                Description = "Save path of the output image.",
                Required = true,
            };
            Option<int> widthOption = new Option<int>("--width")
            {
                Description = "The width of the output image.",
                DefaultValueFactory = _ => 500
            };
            Option<int> heightOption = new Option<int>("--height")
            {
                Description = "The height of the output image.",
                DefaultValueFactory = _ => 500
            };
            Option<double> xMinOption = new Option<double>("--xmin")
            {
                Description = "The minimum value of x.",
                DefaultValueFactory = _ => -10
            };
            Option<double> xMaxOption = new Option<double>("--xmax")
            {
                Description = "The maximum value of x.",
                DefaultValueFactory = _ => 10
            };
            Option<double> yMinOption = new Option<double>("--ymin")
            {
                Description = "The minimum value of y.",
                DefaultValueFactory = _ => -10
            };
            Option<double> yMaxOption = new Option<double>("--ymax")
            {
                Description = "The maximum value of y.",
                DefaultValueFactory = _ => 10
            };
            Option<string> drawColorOption = new Option<string>("--drawColor")
            {
                Description = "Plotting color.",
                DefaultValueFactory = _ => "#C80078D7"
            };

            Option<string> backgroundColorOption = new Option<string>("--backgroundColor")
            {
                Description = "Image background color.",
                DefaultValueFactory = _ => "#FFFFFFFF"
            };
            Option<int> timeoutOption = new Option<int>("--timeout")
            {
                Description = "Draw timeout (milliseconds).",
                DefaultValueFactory = _ => 10000
            };

            RootCommand rootCommand = new RootCommand("ImplicitPlotter - Plot the graph of any binary implicit function equation or inequality.");
            rootCommand.Options.Add(relationOption);
            rootCommand.Options.Add(outputOption);
            rootCommand.Options.Add(widthOption);
            rootCommand.Options.Add(heightOption);
            rootCommand.Options.Add(xMinOption);
            rootCommand.Options.Add(xMaxOption);
            rootCommand.Options.Add(yMinOption);
            rootCommand.Options.Add(yMaxOption);
            rootCommand.Options.Add(drawColorOption);
            rootCommand.Options.Add(backgroundColorOption);
            rootCommand.Options.Add(timeoutOption);
            rootCommand.SetAction(parseResult =>
            {
                Plotter.Plot(new PlotConfig
                {
                    Relation = parseResult.GetValue(relationOption),
                    Output = parseResult.GetValue(outputOption),
                    Width = parseResult.GetValue(widthOption),
                    Height = parseResult.GetValue(heightOption),
                    XMin = parseResult.GetValue(xMinOption),
                    XMax = parseResult.GetValue(xMaxOption),
                    YMin = parseResult.GetValue(yMinOption),
                    YMax = parseResult.GetValue(yMaxOption),
                    DrawColor = parseResult.GetValue(drawColorOption),
                    BackgroundColor = parseResult.GetValue(backgroundColorOption),
                    Timeout = parseResult.GetValue(timeoutOption),
                });
                return 0;
            });

            return rootCommand.Parse(args).Invoke();
        }
    } 
}
