using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;

using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Spreadsheet;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace DataBase_Medical.Windows
{
    /// <summary>
    /// Логика взаимодействия для Window_Plot.xaml
    /// </summary>
    public partial class Window_Plot : Window
    {
        System.Data.DataTable dt;
        String plotTitle;
        public Window_Plot(System.Data.DataTable dt, String plotTitle)
        {
            InitializeComponent();
            this.dt = dt;
            this.plotTitle = plotTitle;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var plotModel = new PlotModel { Title = plotTitle };
            plotModel.Axes.Add(new LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom });
            plotModel.Axes.Add(new OxyPlot.Axes.CategoryAxis { Position = OxyPlot.Axes.AxisPosition.Left });
            
            foreach (DataRow row in dt.Rows)
            {
                string xValue = row[dt.Columns[0].ColumnName].ToString();
                double yValue = Convert.ToDouble(row[dt.Columns[1].ColumnName]);
                plotModel.Series.Add(new OxyPlot.Series.BarSeries { Title = xValue, ItemsSource = new[] { new OxyPlot.Series.BarItem(yValue) } });
            }
            plotView.Model = plotModel;
        }
    }
}
