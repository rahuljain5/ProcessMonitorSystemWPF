using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace PMSAppWPF
{
    /// <summary>
    /// Interaction logic for GraphWindow.xaml
    /// </summary>
    public partial class GraphWindow : Window
    {
        private Dictionary<DateTime, List<Process>> sampledData;
        private List<Process> processes;

        public GraphWindow(Dictionary<DateTime, List<Process>> sampledData, List<Process> processes)
        {
            this.sampledData = sampledData;
            this.processes = processes;
            InitializeComponent();
            DataContext = new MainViewModel(sampledData, processes, "PagedSystemMemorySize64");
        }


        public PlotModel MyModel { get; private set; }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var x = dropDown.SelectedItem as ComboBoxItem;
            DataContext = new MainViewModel(sampledData, processes, x.Content.ToString());
        }
    }
    public class MainViewModel
    {
        Dictionary<DateTime, List<Process>> sampledData;
        List<Process> processes;
        private readonly string prop;

        public MainViewModel(Dictionary<DateTime, List<Process>> sampledData, List<Process> processes, string prop)
        {
            this.sampledData = sampledData;
            this.processes = processes;
            this.prop = prop;
            Plot();
        }

        private void Plot()
        {
            
            var model = new PlotModel() { LegendSymbolLength = 24 };
            Random rnd = new Random();
            //OxyColor.FromRgb((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
            foreach (Process proc in processes)
            {
                double initVal = GetPropValue(proc, prop);
                var line = new LineSeries()
                {
                    Title = proc.ProcessName,
                    //LabelFormatString = "{1}",
                    // LabelFormatString = "{0} -> {1}",
                    Color = OxyColors.SkyBlue,
                    MarkerType = MarkerType.Diamond,
                    MarkerSize = 4,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColor.FromRgb((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256)),
                    MarkerStrokeThickness = 1.5
                };
                int i = 0;
                foreach (var item in sampledData)
                {
                    Process p = item.Value.Find(a => a.Id == proc.Id);
                    if (p != null)
                    {
                        
                        line.Points.Add(new DataPoint(i,(GetPropValue(p, prop) - initVal)));
                        
                        i++;
                    }
                }
                model.Series.Add(line);
            }
        
            model.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom, MinimumPadding = 0.1, MaximumPadding = 0.1, Title = "Sample Interval" });
            model.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MinimumPadding = 0.1, MaximumPadding = 0.1, Title = prop });
            MyModel = model;
        }
        public static double GetPropValue(object src, string propName)
        {
            return double.Parse(src.GetType().GetProperty(propName).GetValue(src, null).ToString());
        }
        public PlotModel MyModel { get; private set; }
    }
}

