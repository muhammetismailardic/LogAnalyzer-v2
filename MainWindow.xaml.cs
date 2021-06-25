using DevExpress.Xpf.Grid;
using LogAnalyzerV2.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogAnalyzerV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BGWorker bgWorker;
        public static string test;
        OppositeInformationWindow oppositeInformation;
        private List<string> cmbSessionList;
        private string serviceIp;

        public MainWindow()
        {
            InitializeComponent();
            bgWorker = new BGWorker(this, oppositeInformation);
        }
        private void btnUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            if (bgWorker.missingOpposites != null)
            {
                // Cleaning  the list before proceed.
                bgWorker.missingOpposites.Clear();
            }
            uploadFiles();
        }

        #region tools
        private void uploadFiles()
        {
            ClearComboBoxes();
            // Wipe out data
            grdDailyColReports.ItemsSource = null;
            // Wipe out bands
            grdDailyColReports.Bands.Clear();
            // Wipe out list
            grdAgentFileTransfer.ItemsSource = null;

            // Deletes previous result from memory
            bgWorker.scheduledJobsList = new List<CollectionItem>();
            bgWorker.ServerAgentTable = new List<ServerAgentCollection>();

            if (IsFileTransferEnabled.IsChecked == true)
            {
                bgWorker.transferItems = new List<TransferItems>();
                bgWorker.IsFileTransferEnabled = true;

            }

            else if (IsFileTransferEnabled.IsChecked == false)
            {
                bgWorker.IsFileTransferEnabled = false;
            }

            bgWorker.worker.RunWorkerAsync(bgWorker.SelectFileToProceed());
        }
        private void ClearComboBoxes()
        {
            cmbVSVAFAgents.ItemsSource = null;
            cmbVSVAFServers.ItemsSource = null;

            cmbCSVAFServers.ItemsSource = null;
            cmbCSVAFAgents.ItemsSource = null;
            cmbCSVAFSessions.ItemsSource = null;
        }
        public void MessageToUser(string msg)
        {
            MessageBox.Show(msg);
        }
        #endregion

        #region GridPopulation
        public void PopulateGrid(List<ServerAgentCollection> collections, List<CollectionItem> colSum)
        {
            try
            {
                if (colSum.Count != 0)
                {
                    if (collections != null || collections.Count != 0)
                    {
                        GridControlBand banners = null;
                        GridColumn items = null;

                        grdDailyColReports.BeginDataUpdate();

                        var serverIps = collections.Select(serverIp => serverIp.ServerId).Distinct().ToList();

                        int bannerCount = 0;
                        for (int i = 0; i < 1; i++)
                        {
                            bannerCount++;

                            // Generates only one time.
                            if (bannerCount == 1)
                            {
                                banners = new GridControlBand();
                                banners.Header = "#";

                                items = new GridColumn();
                                items.Header = "Type";
                                items.FieldName = "CollectionType";
                                banners.Columns.Add(items);

                                grdDailyColReports.Bands.Add(banners);
                            }

                            banners = new GridControlBand();
                            banners.Header = String.Format("VAF Server ({0})", serverIps[i]);

                            items = new GridColumn();
                            items.Header = "Started";
                            items.FieldName = "Started_" + i;
                            banners.Columns.Add(items);

                            items = new GridColumn();
                            items.Header = "Completed";
                            items.FieldName = "Completed_" + i;
                            banners.Columns.Add(items);

                            items = new GridColumn();
                            items.Header = "Duration";
                            items.FieldName = "Duration_" + i;
                            banners.Columns.Add(items);

                            grdDailyColReports.Bands.Add(banners);

                            var AgentIps = collections.Where(s => s.ServerId == serverIps.ElementAt(i).ToString()).Select(a => a.AgentId).OrderBy(x => x).ToList();

                            for (int j = 0; j < AgentIps.Count(); j++)
                            {
                                banners = new GridControlBand();
                                banners.Header = "VAF Agent" + "(" + AgentIps[j] + ")";

                                items = new GridColumn();
                                items.Header = "Started";
                                items.FieldName = "Started_" + (j + 1).ToString();
                                banners.Columns.Add(items);

                                items = new GridColumn();
                                items.Header = "Completed";
                                items.FieldName = "Completed_" + (j + 1).ToString();
                                banners.Columns.Add(items);

                                items = new GridColumn();
                                items.Header = "Duration";
                                items.FieldName = "Duration_" + (j + 1).ToString();
                                banners.Columns.Add(items);

                                grdDailyColReports.Bands.Add(banners);
                            }
                        }
                    }
                }
            }
            finally
            {
                grdDailyColReports.EndDataUpdate();
            }
        }

        #endregion

        #region combobox controls 
        private bool handleVsServers = true;
        private void cmbVSVAFServers_DropDownClosed(object sender, EventArgs e)
        {
            if (handleVsServers) HandleVsServers();
            handleVsServers = true;
        }
        private void CmbVSVAFServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            handleVsServers = cmb.IsDropDownOpen;
            //HandleVsServers();
        }

        private void HandleVsServers()
        {
            string selectedVAFServer;

            if (cmbVSVAFServers.SelectedIndex > -1)
            {
                selectedVAFServer = cmbVSVAFServers.SelectedItem.ToString();
            }

            else
            {
                selectedVAFServer = null;
            }

            if (selectedVAFServer != null)
            {
                if (bgWorker.ServerAgentTable.Where(s => s.ServerId == selectedVAFServer).Select(s => s.AgentId).Count() > 0)
                {
                    // It will display the related Agents
                    cmbVSVAFAgents.ItemsSource = bgWorker.ServerAgentTable.Where(s => s.ServerId == selectedVAFServer).Select(s => s.AgentId).OrderBy(x => x).ToList();
                }

                else
                {
                    MessageBox.Show(selectedVAFServer + " : Does not have any related agent !! ");
                }

                var serviceSessions = bgWorker.serviceSessions(selectedVAFServer, "VAF Service Session");

                if ((serviceSessions == null || serviceSessions.Count == 0))
                {
                    MessageBox.Show("Server: " + selectedVAFServer + " does not have any session to display.", "Log Analyzer VAF Service" + selectedVAFServer);
                    grdSessionReport.ItemsSource = null;
                }

                else
                {
                    grdSessionReport.ItemsSource = serviceSessions;
                }
            }
        }

        private bool handleVsAgents = true;
        private void cmbVSVAFAgents_DropDownClosed(object sender, EventArgs e)
        {
            if (handleVsAgents) HandleVsAgents();
            handleVsAgents = true;
        }
        private void CmbVSVAFAgents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            handleVsAgents = cmb.IsDropDownOpen;
            HandleVsAgents();
        }
        private void HandleVsAgents()
        {
            string selectedVAFAgent;
            List<ServiceSession> serviceSessions = new List<ServiceSession>();

            if (cmbVSVAFAgents.SelectedIndex > -1)
            {
                selectedVAFAgent = cmbVSVAFAgents.SelectedItem.ToString();
            }
            else
            {
                selectedVAFAgent = null;
            }

            if (selectedVAFAgent != null)
            {
                //serviceSessions = bgWorker.serviceSessions(selectedVAFAgent, "VAF Agent Session");

                //if ((serviceSessions == null || serviceSessions.Count == 0))
                //{
                //    MessageBox.Show("Server: " + selectedVAFAgent + " does not have any session to display.", "Log Analyzer VAF Agent" + selectedVAFAgent);
                //    grdSessionReport.ItemsSource = null;
                //}

                //else
                //{
                //    grdSessionReport.ItemsSource = serviceSessions;
                //}
            }

            else
            {
                grdSessionReport.ItemsSource = null;
            }
        }

        private bool handleCsServers = true;
        private void cmbCSVAFServers_DropDownClosed(object sender, EventArgs e)
        {
            if (handleCsServers) HandleCsServers();
            handleCsServers = true;
        }
        private void CmbCSVAFServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            handleCsServers = cmb.IsDropDownOpen;
            HandleCsServers();
        }
        private void HandleCsServers()
        {
            string selectedVAFServer;
            grdScheduledjobsReport.ItemsSource = null;

            if (cmbCSVAFServers.SelectedIndex > -1)
            {
                selectedVAFServer = cmbCSVAFServers.SelectedItem.ToString();
            }

            else
            {
                selectedVAFServer = null;
            }

            if (selectedVAFServer != null)
            {
                if (bgWorker.ServerAgentTable.Where(s => s.ServerId == selectedVAFServer).Select(s => s.AgentId).Count() > 0)
                {
                    // It will display the related Agents
                    cmbCSVAFAgents.ItemsSource = bgWorker.ServerAgentTable.Where(s => s.ServerId == selectedVAFServer).Select(s => s.AgentId).OrderBy(x => x).ToList();
                }

                else
                {
                    MessageBox.Show(selectedVAFServer + " : Does not have any related agent !! ");
                }

                var grdList = bgWorker.scheduledJobsList.Where(s => s.Ip == selectedVAFServer);

                if (grdList.Count() > 0)
                {
                    grdScheduledjobsReport.ItemsSource = grdList.Where(x => !x.CollectionType.Contains("VAF Service Session"));
                    cmbCSVAFServers.SelectedIndex = 0;

                    if (grdList.Any(x => x.CollectionType == "VAF Service Session"))
                    {
                        var serverSessions = grdList.Where(s => s.CollectionType == "VAF Service Session").ToList();

                        if (serverSessions.Count > 0)
                        {
                            cmbCSVAFSessions.ItemsSource = null;

                            cmbSessionList = new List<string>();
                            foreach (var item in serverSessions)
                            {
                                cmbSessionList.Add(item.Date.ToString("dd/MM/yyyy") +
                                                " : " + (item.Started == DateTime.MinValue ? "unknown" : item.Started.ToString("HH:mm:ss")) +
                                                " ~ " + (item.Completed == null ? "continues" : item.Completed.Value.ToString("HH:mm:ss")));
                            }

                            cmbCSVAFSessions.ItemsSource = cmbSessionList;
                        }
                    }
                }

                else
                {
                    grdScheduledjobsReport.ItemsSource = null;

                    if (selectedVAFServer != "0")
                    {
                        MessageBox.Show("VAF Server: " + selectedVAFServer + " does not have any collection to display.");
                    }
                }

                // assigned to find proper session.
                serviceIp = selectedVAFServer;
            }
        }

        private bool handleCsAgents = true;
        private void cmbCSVAFAgents_DropDownClosed(object sender, EventArgs e)
        {
            if (handleCsAgents) HandleCsAgents();
            handleCsAgents = true;
        }
        private void CmbCSVAFAgents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            handleCsAgents = cmb.IsDropDownOpen;
            HandleCsAgents();
        }
        private void HandleCsAgents()
        {
            string selectedVAFAgent;
            grdScheduledjobsReport.ItemsSource = null;
            cmbCSVAFSessions.ItemsSource = null;

            if (cmbCSVAFAgents.SelectedIndex > -1)
            {
                selectedVAFAgent = cmbCSVAFAgents.SelectedItem.ToString();
            }
            else
            {
                selectedVAFAgent = null;
            }

            if (selectedVAFAgent != null)
            {
                var grdList = bgWorker.scheduledJobsList.Where(s => s.Ip == selectedVAFAgent).ToList();

                if (grdList.Count() > 0)
                {
                    grdScheduledjobsReport.ItemsSource = grdList.Where(x => !x.CollectionType.Contains("VAF Agent Session"));

                    if (grdList.Any(x => x.CollectionType == "VAF Agent Session"))
                    {
                        var agentSessions = grdList.Where(s => s.CollectionType == "VAF Agent Session").ToList();

                        if (agentSessions.Count > 0)
                        {
                            cmbCSVAFSessions.ItemsSource = null;

                            cmbSessionList = new List<string>();
                            foreach (var item in agentSessions)
                            {
                                cmbSessionList.Add(item.Date.ToString("dd/MM/yyyy") +
                                                                                    " : " + (item.Started == DateTime.MinValue ? "unknown" : item.Started.ToString("HH:mm:ss")) +
                                                                                    " ~ " + (item.Completed == null ? "continues" : item.Completed.Value.ToString("HH:mm:ss")));
                            }

                            cmbCSVAFSessions.ItemsSource = cmbSessionList;
                        }
                    }
                }

                else
                {
                    grdScheduledjobsReport.ItemsSource = null;

                    if (selectedVAFAgent != "0")
                    {
                        MessageBox.Show("VAF Agent: " + selectedVAFAgent + " does not have any collection to display.");
                    }
                }

                // assigned to find proper session.
                serviceIp = selectedVAFAgent;
            }

            //else
            //{
            //    MessageBox.Show("Please select Agent first.");
            //}
        }

        private bool handleCsSession = true;
        private void cmbCSVAFSessions_DropDownClosed(object sender, EventArgs e)
        {
            if (handleCsSession) HandleCsSessions();
            handleCsSession = true;
        }
        private void CmbCSVAFSessions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            handleCsSession = cmb.IsDropDownOpen;
            HandleCsSessions();
        }
        private void HandleCsSessions()
        {
            if (serviceIp != null)
            {
                IEnumerable<CollectionItem> selectedDateTimeInterval;

                int selectedIndex = 0;
                string source = bgWorker.scheduledJobsList.Where(y => y.Ip == serviceIp).Select(t => t.Source).FirstOrDefault();
                string colType = " ";
                var selectedSession = new CollectionItem();

                if (cmbCSVAFSessions.SelectedIndex > -1)
                {
                    selectedIndex = cmbCSVAFSessions.SelectedIndex;
                }

                if (source != null)
                {
                    if (source.Contains("Agent"))
                    {
                        colType = "VAF Agent Session";
                    }

                    else if (source.Contains("Service"))
                    {
                        colType = "VAF Service Session";
                    }
                }

                try
                {
                    selectedSession = bgWorker.scheduledJobsList.Where(x => x.CollectionType == colType).ToList()[selectedIndex];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred görmeliyim: ", ex.Source);
                }

                if (selectedSession != null)
                {
                    if (selectedSession.Started != DateTime.MinValue && selectedSession.Completed != null)
                    {
                        // if both starttime and complete time is not null
                        selectedDateTimeInterval = from item in bgWorker.scheduledJobsList.
                                                   Where(s => s.CollectionType != colType).
                                                   Where(s => s.Source == source).
                                                   Where(x => x.Started >= selectedSession.Started).
                                                   Where(x => x.Started <= selectedSession.Completed)
                                                   select item;
                    }

                    else if (selectedSession.Started != DateTime.MinValue && selectedSession.Completed == null)
                    {
                        selectedDateTimeInterval = from item in bgWorker.scheduledJobsList.
                                                   Where(s => s.CollectionType != colType).
                                                   Where(s => s.Source == source).
                                                   Where(x => x.Started >= selectedSession.Started)
                                                   select item;
                    }

                    else if (selectedSession.Started == DateTime.MinValue && selectedSession.Completed != null)
                    {
                        selectedDateTimeInterval = from item in bgWorker.scheduledJobsList.
                                                   Where(s => s.CollectionType != colType).
                                                   Where(s => s.Source == source).
                                                   Where(x => x.Completed <= selectedSession.Completed)
                                                   select item;
                    }

                    else if (selectedSession.Started == DateTime.MinValue && selectedSession.Completed == null)
                    {
                        // There is no such condition.
                        selectedDateTimeInterval = null;
                    }

                    else
                    {
                        selectedDateTimeInterval = null;
                    }

                    if (selectedDateTimeInterval != null)
                    {
                        grdScheduledjobsReport.ItemsSource = selectedDateTimeInterval.Where(x => x.CollectionType != "VAF Agent Session" && x.CollectionType != "VAF Server Session").ToList();
                    }
                }

                else
                {
                    grdScheduledjobsReport.ItemsSource = null;
                }
            }
        }

        private bool handleFileTransfer = true;
        private void cmbAgentFileTransfer_DropDownClosed(object sender, EventArgs e)
        {
            if (handleFileTransfer) HandleFileTransfer();
            handleFileTransfer = true;
        }
        private void cmbAgentFileTransfer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            handleFileTransfer = cmb.IsDropDownOpen;
            HandleFileTransfer();
        }
        private void HandleFileTransfer()
        {
            string selectedVAFAgent = null;

            if (cmbAgentFileTransfer.SelectedIndex > -1)
            {
                selectedVAFAgent = cmbAgentFileTransfer.SelectedItem.ToString();
            }

            else
            {
                selectedVAFAgent = null;
            }

            if (selectedVAFAgent != null)
            {
                List<TransferItems> transfersByAgent;
                if (bgWorker.transferItems != null)
                {
                    transfersByAgent = bgWorker.transferItems.Where(ıp => ıp.Ip == selectedVAFAgent).ToList();
                }
                else { transfersByAgent = null; }

                grdAgentFileTransfer.ItemsSource = transfersByAgent;
            }
        }

        #endregion

        private void Onclick_OppositeInformation(object sender, RoutedEventArgs e)
        {
            OppositeInformationWindow oppWin = new OppositeInformationWindow();
            oppWin.ShowDialog();
        }

        private void btnExpCollectionResults_Click(object sender, RoutedEventArgs e)
        {
            if (bgWorker.scheduledJobsList != null && bgWorker.scheduledJobsList.Count != 0)
            {
                var loc = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CollectionResults " + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
                grdScheduledjobsReportTable.ExportToCsv(loc);

                MessageBox.Show("The list has been generated to Desktop.");
            }
            else
            {
                MessageBox.Show("Nothing to Export");
            }
        }
    }
}
