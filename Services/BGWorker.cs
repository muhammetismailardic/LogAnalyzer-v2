using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzerV2.Services
{
    internal class BGWorker : CollectionAnalyzerBase
    {
        public BackgroundWorker worker;
        private MainWindow _mainWindow;
        private OppositeInformationWindow _oppositeInformationWindow;

        public BGWorker(MainWindow mainWindow, OppositeInformationWindow oppositeInformationWindow)
        {
            _mainWindow = mainWindow;
            _oppositeInformationWindow = oppositeInformationWindow;

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        public void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _mainWindow.progressBar.Value = e.ProgressPercentage;
            _mainWindow.lblPercentage.Content = "%" + e.ProgressPercentage.ToString() + "Loading... Please wait!";
        }

        public void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ReadCSVFile(sender, e, worker);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _mainWindow.progressBar.Value = 0;

            var VAFServices = scheduledJobsList.Where(s => s.Source.Contains("VAF Service")).Select(s => s.Ip).Distinct();
            var VAFAgents = scheduledJobsList.Where(s => s.Source.Contains("VAF Agent")).Select(s => s.Ip).Distinct();

            // Fill up the comboboxes
            _mainWindow.cmbVSVAFServers.ItemsSource = VAFServices.OrderBy(x => x).ToList();
            _mainWindow.cmbVSVAFAgents.ItemsSource = VAFAgents.OrderBy(x => x).ToList();
            _mainWindow.cmbCSVAFAgents.ItemsSource = VAFAgents.OrderBy(x => x).ToList();
            _mainWindow.cmbCSVAFServers.ItemsSource = VAFServices.OrderBy(x => x).ToList();
            _mainWindow.cmbAgentFileTransfer.ItemsSource = VAFAgents.OrderBy(x => x).ToList();
            _mainWindow.lblPercentage.Content = "Completed...";
            _mainWindow.IsFileTransferEnabled.IsChecked = false;

            if (ServerAgentTable.Count == 0)
            {
                _mainWindow.MessageToUser("No related VAF Server & Agent logs detected.");
            }

            else
            {
                // get data for summary collection tab
                var colSumList = scheduledJobsList.ToList();

                // Populate Banners and child columns on Summay Tab
                _mainWindow.PopulateGrid(ServerAgentTable, colSumList);

                // Insert Populated data to grid.
                _mainWindow.grdDailyColReports.ItemsSource = PopulateDatas();
            }
        }
    }
}
