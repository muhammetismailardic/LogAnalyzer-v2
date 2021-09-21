using System;
using System.Collections.Generic;
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

namespace LogAnalyzerV2
{
    /// <summary>
    /// Interaction logic for CapacityUtilization.xaml
    /// </summary>
    public partial class CapacityUtilization : Window
    {
        List<string> dataTypes;
        List<string> portTypes;

        public CapacityUtilization()
        {
            InitializeComponent();

            dataTypes = new List<string>()
            {
                "--Select Value--", "15-minute data", "1-day data"
            };

            portTypes = new List<string>()
            {
                "--Select Value--", "MODEM Port", "Ether Port"
            };

            cmbDataType.ItemsSource = dataTypes;
            cmbPortType.ItemsSource = portTypes;
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            List<double> toBeConverted = new List<double>();
            toBeConverted.Add(double.Parse(txtOctet.Text));
            toBeConverted.Add(double.Parse(txtPacket.Text));

            ConvertBitToMegabit(toBeConverted);
            txtEstResult.Text = "%" + Math.Round(CalculateUtilization(toBeConverted, double.Parse(txtBandwidth.Text)), 4).ToString();
            txtEstResult.Background = Brushes.Red;
            txtEstResult.Foreground = Brushes.Black;
        }

        private double CalculateUtilization(List<double> toBeConverted, double bandwidthValue)
        {
            int value = 0;
            if (cmbPortType.SelectedItem.ToString() == "MODEM Port")
            {
                value = 3;
            }
            else if (cmbPortType.SelectedItem.ToString() == "Ether Port")
            {
                value = 20;
            }

            //=((B7+(B8*3))*100)/(B9*B10)
            if (cmbDataType.SelectedItem.ToString() == "15-minute data")
            {
                return ((toBeConverted[0] + (toBeConverted[1] * value)) * 100) / (bandwidthValue * 900);
            }
            else if (cmbDataType.SelectedItem.ToString() == "1-day data")
            {
                return ((toBeConverted[0] + (toBeConverted[1] * value)) * 100) / (bandwidthValue * 86400);
            }
            else { return 0; }
        }

        private List<double> ConvertBitToMegabit(List<double> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                values[i] = values[i] * 8 / 1024 / 1024;
            }
            return values;
        }



        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtBandwidth.Clear();
            txtEstResult.Clear();
            txtOctet.Clear();
            txtPacket.Clear();
        }
    }
}
