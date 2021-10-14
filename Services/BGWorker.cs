﻿using System;
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
        private NEFtpDuration _NEFtpDuration;

        public BGWorker(MainWindow mainWindow, OppositeInformationWindow oppositeInformationWindow, NEFtpDuration NEFtpDuration)
        {
            _mainWindow = mainWindow;
            _oppositeInformationWindow = oppositeInformationWindow;
            _NEFtpDuration = NEFtpDuration;

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        //public BGWorker(MainWindow mainWindow, NEFtpDuration NEFtpDuration)
        //{
        //    _mainWindow = mainWindow;
        //    _NEFtpDuration = NEFtpDuration;

        //    worker = new BackgroundWorker();
        //    worker.WorkerReportsProgress = true;
        //    worker.DoWork += worker_DoWork;
        //    worker.ProgressChanged += worker_ProgressChanged;
        //    worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        //}

        public void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (scheduledJobsList != null && scheduledJobsList.Count != 0)
            {
                _mainWindow.progressBar.Value = e.ProgressPercentage;
                _mainWindow.lblPercentage.Content = "%" + e.ProgressPercentage.ToString() + " Loading... Please wait!";
            }

            if (missingOpposites != null && missingOpposites.Count != 0)
            {
                _oppositeInformationWindow.MissingOppProgressBar.Value = e.ProgressPercentage;
                _oppositeInformationWindow.lblMissingOppPercentage.Content = "%" + e.ProgressPercentage.ToString() + " Loading... Please wait!";
            }

            if (timeTable != null && timeTable.Count != 0)
            {
                _NEFtpDuration.ftpDurationProgressBar.Value = e.ProgressPercentage;
                _NEFtpDuration.lblFtpDurationPercentage.Content = "%" + e.ProgressPercentage.ToString() + " Loading... Please wait!";
            }
        }

        public void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ReadCSVFile(sender, e, worker);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (scheduledJobsList != null && scheduledJobsList.Count != 0)
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
                    //If there server log the summary grid will be populated
                    if (ServerAgentTable.Any(x => x.Type == true))
                    {
                        _mainWindow.PopulateGrid(ServerAgentTable, colSumList);
                    }

                    // Insert Populated data to grid.
                    _mainWindow.grdDailyColReports.ItemsSource = PopulateDatas();
                }
            }

            if (missingOpposites != null && missingOpposites.Count != 0)
            {
                _oppositeInformationWindow.MissingOppProgressBar.Value = 0;
                _oppositeInformationWindow.lblMissingOppPercentage.Content = "Completed...";
                _oppositeInformationWindow.grdMissingOppList.ItemsSource = missingOpposites.ToList();
            }

            if (timeTable != null && timeTable.Count != 0)
            {
                _NEFtpDuration.ftpDurationProgressBar.Value = 0;
                _NEFtpDuration.lblFtpDurationPercentage.Content = "Completed...";
                _NEFtpDuration.grdNEFtpDuration.ItemsSource = timeTable.ToList();
            }
        }
    }
}
