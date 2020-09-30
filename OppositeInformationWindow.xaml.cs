using LogAnalyzerV2.Services;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for OppositeInformationWindow.xaml
    /// </summary>
    public partial class OppositeInformationWindow : Window
    {
        private BGWorker bgWorker;
        MainWindow MainWindow;
        public OppositeInformationWindow()
        {
            InitializeComponent();
            bgWorker = new BGWorker(MainWindow, this);
        }

        private void btnCheckOpposite_Click(object sender, RoutedEventArgs e)
        {
            bgWorker.NEList = new List<string>();
            bgWorker.RmonData = new List<string>();

            int OppProgressBarItemCount = 0;
           
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV | *.csv;", // file types, that will be allowed to upload
                Multiselect = true // allow / deny user to upload more than one file at a time
            };

            // Display OpenFileDialog by calling ShowDialog method
            bool? result = dialog.ShowDialog();

            if (result == true) // if user clicked OK
            {
                int count = 0;
                // Read the files
                foreach (string path in dialog.FileNames)
                {
                    var list = File.ReadAllLines(path);
                    count += list.Count();
                    if (dialog.FileName.Contains("NEList"))
                    {
                        bgWorker.NEList.AddRange(list.ToList());
                    }
                    else if (dialog.FileName.Contains("rmon"))
                    {
                        bgWorker.RmonData.AddRange(list.ToList());
                    }
                }
                OppProgressBarItemCount = count;
            }

            bgWorker.worker.RunWorkerAsync(OppProgressBarItemCount);
        }
    }
}
