using LogAnalyzerV2.Models;
using LogAnalyzerV2.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LogAnalyzerV2
{
    /// <summary>
    /// Interaction logic for NEFtpDuration.xaml
    /// </summary>
    public partial class NEFtpDuration
    {
        private BGWorker bgWorker;
        public NEFtpDuration()
        {
            InitializeComponent();
            bgWorker = new BGWorker(null, null, this);
        }
        private void btnCheckFTPDuration_Click(object sender, RoutedEventArgs e)
        {
            bgWorker.timeTableStringList = new List<string>();
            bgWorker.timeTable = new List<TimeTable>();
            grdNEFtpDuration.ItemsSource = null;

            int count = 0;
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV | *.csv;", // file types, that will be allowed to upload
                Multiselect = true // allow / deny user to upload more than one file at a time
            };

            // Display OpenFileDialog by calling ShowDialog method
            bool? result = dialog.ShowDialog();

            if (FileNameCheck(dialog.FileNames))
            {
                if (result == true) // if user clicked OK
                {
                    // Read the files
                    foreach (string path in dialog.FileNames)
                    {
                        var fi = new FileInfo(path);
                        if (bgWorker.IsFileLocked(fi) == false)
                        {
                            count += File.ReadLines(path).Count();
                            foreach (var item in File.ReadLines(path))
                            {
                                bgWorker.timeTableStringList.Add(item);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Can not access the one of files! \n It is used by an other proceess.");
                            bgWorker.timeTableStringList = null;
                            break;
                        }
                    }
                }
                bgWorker.worker.RunWorkerAsync(count);
            }
        }
        private bool FileNameCheck(string[] InsertedFileNames)
        {
            string desiredName = "TRC";

            if (InsertedFileNames.Count() != 0)
            {
                // Check if required file names are ok!
                for (int i = 0; i < InsertedFileNames.Length; i++)
                {
                    if (!InsertedFileNames[i].Contains(desiredName))
                    {
                        MessageBox.Show("Required File Names are not match!");
                        return false;
                    }
                }
                return true;
            }
            else { return false; }

        }
        private void btnExpFtpDurationList_Click(object sender, RoutedEventArgs e)
        {
            if (bgWorker.timeTable.Count != 0)
            {
                var loc = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\NEFTPDuration " + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx";
                expGrdNEFtpDuration.ExportToXlsx(loc);
                MessageBox.Show("The list has been generated to Desktop.",
                    "Log Analyzer", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Nothing to Export",
                    "Log Analyzer", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        private void requiredData_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Please Insert Trace Logs from VAF Agent to get FTP Durations!", "Log Analyzer Informational",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
