using DevExpress.Data.Filtering.Helpers;
using DevExpress.Utils.Filtering.Internal;
using LogAnalyzerV2.Models;
using LogAnalyzerV2.Models.HelperClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogAnalyzerV2.Services
{
    internal class CollectionAnalyzerBase
    {
        private Microsoft.Win32.OpenFileDialog dialog;

        // For log analyzer section
        public List<string> NEList;
        public List<string> RmonData;
        public List<string> RadioConfData;
        public List<ServerAgentCollection> ServerAgentTable;
        public List<CollectionItem> scheduledJobsList;
        // Insert Transfer files......
        public List<TransferItems> transferItems;
        public bool IsFileTransferEnabled = false;

        // For missing far end section
        public List<MissingOpposite> missingOpposites = new List<MissingOpposite>();
        List<RmonItems> RmonItems;
        List<string> RmonIndexLoc;
        List<RadioConfItems> RadioConfigurationItems;
        List<string> RadioConfigurationIndexLoc;
        List<RadioConfItems> newList = new List<RadioConfItems>();

        //Using NE List
        string tempIp = String.Empty;
        string tempTX = String.Empty;
        string tempPort = String.Empty;
        private int rmonLineCount = 0;
        private string[] properLine;
        private string collectionType, items;

        // For RadioConfiguration
        List<RadioConfItems> selectNearEnd = new List<RadioConfItems>();
        List<RadioConfItems> selectFarEnd = new List<RadioConfItems>();

        protected void ReadCSVFile(object sender, DoWorkEventArgs e, BackgroundWorker bw)
        {
            int max = (int)e.Argument;
            int result = 0;
            int counter = 0;

            if (dialog != null)
            {
                foreach (string path in dialog.FileNames)
                {
                    foreach (var line in File.ReadLines(path))
                    {
                        counter++;
                        if (!String.IsNullOrEmpty(line))
                        {
                            int progressPercentage = Convert.ToInt32((counter * 100) / max);
                            if (counter % 42 == 0)
                            {
                                result++;
                                bw.ReportProgress(progressPercentage);
                            }
                            e.Result = result;

                            AnalyzeLog(line);
                        }
                    }
                }
            }

            if (NEList != null && NEList.Count != 0 && RmonData != null && RmonData.Count != 0 && RadioConfData != null && RadioConfData.Count != 0)
            {
                AnalyzeOppositeInfo(max, result, counter, e, bw);
            }
        }

        // For Missing far end section
        private void AnalyzeOppositeInfo(int max, int result, int counter, DoWorkEventArgs e, BackgroundWorker bw)
        {
            missingOpposites = new List<MissingOpposite>();

            // Get the first list of NEs to proceed further
            AnalyzeNEList(max, result, counter, e, bw);
        }
        private void AnalyzeNEList(int max, int result, int counter, DoWorkEventArgs e, BackgroundWorker bw)
        {
            List<string> NeListOppIps = new List<string>();
            string primaryAdd = "";
            string NeType = "";

            List<int> NeListOppLoc = new List<int>();
            int primaryAddLoc = 3;
            int NeTypeLoc = 2;

            foreach (string line in NEList)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    counter++;
                    int progressPercentage = Convert.ToInt32((counter * 100) / max);

                    //TODO: Here will be checked!
                    result++;
                    bw.ReportProgress(progressPercentage);
                    e.Result = result;

                    // TODO Define opp Ip Loc
                    var items = line.Split(',');
                    if (NEList.IndexOf(line) == 1)
                    {
                        int loc = 0;
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (items[i].Contains("Opposite NE Primary Address-"))
                            {
                                NeListOppLoc.Add(loc);
                            }
                            loc++;
                        }
                    }

                    // Find Opp IPs
                    if (NEList.IndexOf(line) != 0 && NEList.IndexOf(line) != 1)
                    {
                        // Getting NE Type and IP address.
                        if (primaryAddLoc != -1 && NeTypeLoc != -1)
                        {
                            primaryAdd = items[primaryAddLoc].ToString();
                            NeType = items[NeTypeLoc].ToString();
                        }


                        foreach (var index in NeListOppLoc)
                        {
                            if (!String.IsNullOrEmpty(items[index].ToString()))
                            {
                                // Slot Number
                                var currrentIndexLoc = NeListOppLoc.IndexOf(index) + 1;

                                if (currrentIndexLoc == 9)
                                {
                                    currrentIndexLoc = 11;
                                }
                                else if (currrentIndexLoc == 10)
                                {
                                    currrentIndexLoc = 12;
                                }
                                else if (currrentIndexLoc == 11)
                                {
                                    currrentIndexLoc = 13;
                                }
                                else if (currrentIndexLoc == 12)
                                {
                                    currrentIndexLoc = 14;
                                }

                                NeListOppIps.Add((items[index] + "," + "MODEM (" + "Slot" + currrentIndexLoc.ToString("D2") + ")"));
                            }
                        }

                        // Here add the founded items to list
                        // This can be simplified
                        foreach (var item in NeListOppIps)
                        {
                            var missingOpp = new MissingOpposite()
                            {
                                NEType = NeType,
                                IP = Regex.Replace(primaryAdd, "0*([0-9]+)", "${1}"),
                                Port = item.Split(',')[1].ToString(),
                                OppIP = item.Split(',')[0].ToString(),
                                OppPort = "",
                            };
                            missingOpposites.Add(missingOpp);
                        }
                    }
                }
                primaryAdd = "";
                NeType = "";
                NeListOppIps.Clear();
            }
            //Adding counts to progress bar
            max += missingOpposites.Count();

            AnalyzeNEListForOpposite(max, result, counter, e, bw);
        }
        private void AnalyzeNEListForOpposite(int max, int result, int counter, DoWorkEventArgs e, BackgroundWorker bw)
        {
            var IpList = missingOpposites.Select(x => x.IP).Distinct();

            max += IpList.Count();
            foreach (var Ip in IpList)
            {
                counter++;
                int progressPercentage = Convert.ToInt32((counter * 100) / max);

                //TODO: Here will be checked!
                result++;
                bw.ReportProgress(progressPercentage);
                e.Result = result;

                if (Ip == "10.25.46.5")
                {
                    Console.WriteLine("Here");
                }

                //find details for defined Ip
                var findIp = missingOpposites.Where(x => x.IP == Ip).ToList();

                //Find all port information
                var findNearEndPorts = findIp.Select(x => x.Port).ToList();

                //Find all opp ports
                var findOpps = findIp.Select(y => y.OppIP).ToList().Distinct();
                foreach (var oppIp in findOpps)
                {
                    // Find reverse opposite ports for selected IP.
                    var oppPorts = missingOpposites.Where(x => x.IP == oppIp && x.OppIP == Ip).Select(y => y.Port).ToList();

                    // If Selected reverse opp ip exist at other end
                    if (oppPorts != null)
                    {
                        int i = 0;
                        foreach (var missingOpposite in missingOpposites.Where(ip => ip.IP == Ip && ip.OppIP == oppIp))
                        {
                            if (String.IsNullOrWhiteSpace(missingOpposite.OppPort))
                            {
                                while (i < oppPorts.Count())
                                {
                                    missingOpposite.OppPort = oppPorts[i];
                                    break;
                                }
                                i++;
                            }
                        }
                    }
                }
            }
            AnalyzeRadioConfigurationInfo(max, result, counter, e, bw);
        }
        private void AnalyzeRadioConfigurationInfo(int max, int result, int counter, DoWorkEventArgs e, BackgroundWorker bw)
        {
            RadioConfigurationIndexLoc = new List<string>();
            RadioConfigurationItems = new List<RadioConfItems>();

            // Analyze Radio Configuration data to find index and related data for further process
            // When this loop over RadioConfigurationItems list will be filled.

            foreach (string line in RadioConfData)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    counter++;
                    int progressPercentage = Convert.ToInt32((counter * 100) / max);

                    result++;
                    bw.ReportProgress(progressPercentage);
                    e.Result = result;

                    // Finding Indexies
                    var items = line.Split(',');
                    if (RadioConfData.IndexOf(line) == 0)
                    {
                        ////Define the RadioConfData line count
                        //rmonLineCount = items.Length;

                        int loc = 0;
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (items[i].Equals("Near End IP Address") || items[i].Equals("IP Address"))
                            {
                                RadioConfigurationIndexLoc.Add("IP Address," + loc);
                            }

                            if (items[i].Equals("Near End Slot") || items[i].Equals("Slot"))
                            {
                                RadioConfigurationIndexLoc.Add("Port," + loc);
                            }

                            //if (items[i].Equals("Far End IP Address"))
                            //{
                            //    RadioConfigurationIndexLoc.Add("Port," + loc);
                            //}

                            if (items[i].Equals("TX RF Frequency"))
                            {
                                RadioConfigurationIndexLoc.Add("TX Frequency," + loc);
                            }

                            if (items[i].Equals("RX RF Frequency"))
                            {
                                RadioConfigurationIndexLoc.Add("RX Frequency," + loc);
                            }

                            loc++;
                        }
                    }

                    // Find Opp IPs using Index
                    if (RadioConfData.IndexOf(line) != 0)
                    {
                        // Insert headers to list
                        var radioConfItems = new RadioConfItems();
                        foreach (var item in RadioConfigurationIndexLoc)
                        {
                            //Check if the array lenght is ok
                            bool IsOk = inBounds((int.Parse(item.Split(',')[1])), items);

                            if (IsOk)
                            {
                                if (item.Split(',')[0] == "IP Address")
                                {
                                    radioConfItems.IpAddress = Regex.Replace(items[(int.Parse(item.Split(',')[1]))], "0*([0-9]+)", "${1}");
                                }

                                else if (item.Split(',')[0] == "Port")
                                {
                                    radioConfItems.Port = "MODEM (" + "Slot" + int.Parse(items[(int.Parse(item.Split(',')[1]))]).ToString("D2") + ")";
                                }

                                else if (item.Split(',')[0] == "TX Frequency")
                                {
                                    radioConfItems.TXRFFrequency = items[(int.Parse(item.Split(',')[1]))];
                                }

                                else if (item.Split(',')[0] == "RX Frequency")
                                {
                                    radioConfItems.RXRFFrequency = items[(int.Parse(item.Split(',')[1]))];
                                }
                            }
                        }

                        //Adding items to Rmon list.
                        RadioConfigurationItems.Add(radioConfItems);
                    }
                }
            }
            AnalyzeOppositeInfoRmon(max, result, counter, e, bw);
        }
        private void AnalyzeOppositeInfoRmon(int max, int result, int counter, DoWorkEventArgs e, BackgroundWorker bw)
        {
            RmonIndexLoc = new List<string>();
            RmonItems = new List<RmonItems>();

            // Analyze Rmon data to find index and related data for further process
            // When this loop over RmonItems list will be filled.
            foreach (string line in RmonData)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    counter++;
                    int progressPercentage = Convert.ToInt32((counter * 100) / max);

                    result++;
                    bw.ReportProgress(progressPercentage);
                    e.Result = result;

                    // Finding Indexies
                    var items = line.Split(',');
                    if (RmonData.IndexOf(line) == 0)
                    {
                        //Define the Rmon line count
                        rmonLineCount = items.Length;

                        int loc = 0;
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (items[i].Equals("Date"))
                            {
                                RmonIndexLoc.Add("Date," + loc);
                            }

                            if (items[i].Equals("IP Address"))
                            {
                                RmonIndexLoc.Add("IP Address," + loc);
                            }

                            if (items[i].Equals("Port"))
                            {
                                RmonIndexLoc.Add("Port," + loc);
                            }

                            if (items[i].Equals("Opposite NE IP Address"))
                            {
                                RmonIndexLoc.Add("Opposite NE IP Address," + loc);
                            }

                            if (items[i].Equals("Opposite Port"))
                            {
                                RmonIndexLoc.Add("Opposite Port," + loc);
                            }

                            if (items[i].Equals("Group Member"))
                            {
                                RmonIndexLoc.Add("Group Member," + loc);
                            }
                            loc++;
                        }
                    }

                    // Find Opp IPs using Index
                    if (RmonData.IndexOf(line) != 0)
                    {
                        // Insert headers to list
                        var rmonItems = new RmonItems();
                        foreach (var item in RmonIndexLoc)
                        {
                            //Check if the array lenght is ok
                            bool IsOk = inBounds((int.Parse(item.Split(',')[1])), items);

                            if (IsOk)
                            {
                                if (item.Split(',')[0] == "Date")
                                {
                                    rmonItems.Date = items[(int.Parse(item.Split(',')[1]))];
                                }

                                else if (item.Split(',')[0] == "IP Address")
                                {
                                    rmonItems.IP = items[(int.Parse(item.Split(',')[1]))];
                                }

                                else if (item.Split(',')[0] == "Port")
                                {
                                    rmonItems.Port = items[(int.Parse(item.Split(',')[1]))];
                                }

                                else if (item.Split(',')[0] == "Opposite NE IP Address")
                                {
                                    rmonItems.OppIP = items[(int.Parse(item.Split(',')[1]))];
                                }

                                else if (item.Split(',')[0] == "Opposite Port")
                                {
                                    rmonItems.OppPort = items[(int.Parse(item.Split(',')[1]))];
                                }

                                else if (item.Split(',')[0] == "Group Member")
                                {
                                    rmonItems.GroupMember = items[(int.Parse(item.Split(',')[1]))];
                                }
                            }
                        }

                        //Adding items to Rmon list.
                        RmonItems.Add(rmonItems);
                    }
                }
            }
            UpdatingTheListWithRmonFile(max, result, counter, e, bw);
        }
        private void UpdatingTheListWithRmonFile(int max, int result, int counter, DoWorkEventArgs e, BackgroundWorker bw)
        {
            foreach (var preparedNEListItems in missingOpposites)
            {
                bool isLast = false;
                counter++;
                int progressPercentage = Convert.ToInt32((counter * 100) / max);

                //TODO: Here will be checked!
                result++;
                bw.ReportProgress(progressPercentage);
                e.Result = result;

                if (preparedNEListItems == missingOpposites.Last())
                {
                    isLast = true;
                }
                InsertRadioConfDataToNEList(preparedNEListItems, isLast);

                //Updating Missing Opposite with Rmon using Rmon data
                var rmonSelectedOppPort = RmonItems.Where(x => x.IP == preparedNEListItems.IP && x.Port == preparedNEListItems.Port).Select(a => new { a.OppIP, a.OppPort, a.GroupMember, a.Date }).ToList();
                if (rmonSelectedOppPort.Count == 1)
                {
                    var rmonTheSelected = rmonSelectedOppPort.SingleOrDefault();

                    //Update the selected list if exist.
                    if (rmonSelectedOppPort != null)
                    {
                        //Check if Group Member for any potential match
                        //var neListOppPort = preparedNEListItems.OppPort.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                        var findOppositeIpDetails = RmonItems.Find(x => x.IP == rmonTheSelected.OppIP && x.Port == preparedNEListItems.OppPort);

                        preparedNEListItems.HasRmon = true;
                        preparedNEListItems.Date = rmonTheSelected.Date;
                        preparedNEListItems.GroupMember = (String.IsNullOrWhiteSpace(rmonTheSelected.GroupMember) ? "-" : rmonTheSelected.GroupMember);
                        preparedNEListItems.OppIpInRmon = rmonTheSelected.OppIP;
                        preparedNEListItems.OppPortInRmon = rmonTheSelected.OppPort;

                        if (findOppositeIpDetails != null)
                            preparedNEListItems.OppGroupMember = (String.IsNullOrWhiteSpace(findOppositeIpDetails.GroupMember) ? "-" : findOppositeIpDetails.GroupMember);

                        // Linking both Near & Far End NE type
                        string oppPortNeType = missingOpposites.Where(x => x.IP == preparedNEListItems.OppIP && x.Port == preparedNEListItems.OppPort).Select(t => t.NEType).FirstOrDefault();
                        preparedNEListItems.NEType = preparedNEListItems.NEType + "--" + oppPortNeType;

                        //If NE list opposite match with Rmon line.
                        if (rmonTheSelected.OppIP == preparedNEListItems.OppIP)
                        {
                            // If NE list and Rmon Opposite match!
                            if (rmonTheSelected.OppPort == preparedNEListItems.OppPort)
                            {
                                preparedNEListItems.IsMatch = true;
                                preparedNEListItems.GroupMember = rmonTheSelected.GroupMember;
                            }
                            else if (!String.IsNullOrWhiteSpace(preparedNEListItems.OppPort) && !String.IsNullOrWhiteSpace(rmonTheSelected.OppPort))
                            {
                                if (findOppositeIpDetails != null)
                                {
                                    if (findOppositeIpDetails.GroupMember.Contains(preparedNEListItems.OppPort))
                                        preparedNEListItems.IsMatch = true;
                                }
                            }
                            else if (!String.IsNullOrWhiteSpace(preparedNEListItems.OppPort) && String.IsNullOrWhiteSpace(rmonTheSelected.OppPort))
                            {
                                preparedNEListItems.Status = "Issue, (Only NE list has opposite Info)";
                            }
                            else if (String.IsNullOrWhiteSpace(preparedNEListItems.OppPort) && !String.IsNullOrWhiteSpace(rmonTheSelected.OppPort))
                            {
                                preparedNEListItems.Status = "Issue, (Only Rmon has Opposite info)";
                            }
                        }
                        else if (String.IsNullOrWhiteSpace(rmonTheSelected.OppIP))
                        {
                            preparedNEListItems.Status = "Issue, (Rmon has No Opposite IP)";
                        }

                        if (preparedNEListItems.IsMatch == true && preparedNEListItems.HasRmon == true)
                        {
                            preparedNEListItems.Status = "Ok";
                        }
                    }
                }
                else
                {
                    if (preparedNEListItems.HasRmon == false && String.IsNullOrWhiteSpace(preparedNEListItems.OppPort))
                    {
                        preparedNEListItems.Status = "Issue, (Does'nt have Rmon and Opposite Link)";
                    }

                    else if (preparedNEListItems.HasRmon == false)
                        preparedNEListItems.Status = "Issue, (Does'nt have Rmon)";
                }
            }
            PrepareListForUniqueMatchInOpposite();
        }

        // For Log Analyzer
        private void AnalyzeLog(string readLogFile)
        {
            if (!string.IsNullOrWhiteSpace(readLogFile))
            {
                if (readLogFile.Contains("UNMS VAF Module Service"))
                {
                    VAFServerCollection(readLogFile);
                }
                else if (readLogFile.Contains("UNMS VAF Module Agent"))
                {
                    VAFAgentCollection(readLogFile);
                }
                
            }
        }
        private void VAFServerCollection(string line)
        {
            if (!line.Contains("UNMS VAF Module Service,An agent service stopped and started again")
                && !line.Contains("UNMS VAF Module Service,An agent service started to work during the job is in progress."))
            {
                if (line.Contains("config file"))
                {
                    var serverIp = line.Split(',')[2];
                    var agentIp = line.Split(',')[4];
                    agentIp = agentIp.Split('(', ')')[1];

                    if (!ServerAgentTable.Any(a => a.AgentId == agentIp))
                    {
                        ServerAgentTable.Add(new ServerAgentCollection { ServerId = serverIp, AgentId = agentIp });
                    }
                }

                else if (line.Contains("Historical PMON/RMON Data Collection") == true ||
                         line.Contains("Actual PMON/RMON Data Collection") == true ||
                         line.Contains("Historical PMON/RMON Data Transfer") == true ||
                         line.Contains("Actual PMON/RMON Data Transfer") == true)
                {
                    if (!line.Contains("Data Collection could not be perfermed on some Agents"))
                    {
                        properLine = MakeLineProper(line);
                        properLine = properLine[4].Split(' ');

                        items = properLine[0] + ' ' + properLine[1] + ' ' + properLine[2] + ' ' + properLine[3];

                        collectionType = items;

                        ExecuteForServer(line, collectionType);
                    }
                }

                else if (line.Contains("Inventory Data Collection") == true || line.Contains("Inventory Data Mergining") == true ||
                         line.Contains("Provisioning Data Collection") == true || line.Contains("Provisioning Data Mergining") == true ||
                         line.Contains("Metering Data Collection") == true)
                {
                    properLine = MakeLineProper(line);
                    properLine = properLine[4].Split(' ');

                    items = properLine[0] + ' ' + properLine[2];

                    collectionType = items;

                    ExecuteForServer(line, collectionType);
                }

                else if (line.Contains("History Backup") == true || line.Contains("Network Health") == true)
                {
                    properLine = MakeLineProper(line);
                    properLine = properLine[4].Split(' ');

                    items = properLine[0] + ' ' + properLine[1];

                    collectionType = items;

                    ExecuteForServer(line, collectionType);
                }

                else if (line.Contains("UNMS VAF Module service started successfully") == true || line.Contains("VAF service started successfully") == true
                         || line.Contains("VAF service stopped") == true || line.Contains("UNMS VAF Module service stopped") == true)
                {
                    ExecuteForServer(line, "VAF Service Session");
                }
            }
        }
        private void VAFAgentCollection(string line)
        {
            if (line.Contains("Inventory Collection Process Starting"))
            {
                properLine = MakeLineProper(line);
                properLine = properLine[4].Split(' ');
                collectionType = properLine[0] + ' ' + properLine[1];

                ExecuteForAgent(line, collectionType);
            }

            else if (line.Contains("Inventory data collection process is completed"))
            {
                properLine = MakeLineProper(line);
                properLine = properLine[4].Split(' ');
                string convertedString = FirstLetterToUpper(properLine[2]);
                collectionType = properLine[0] + ' ' + convertedString;

                ExecuteForAgent(line, collectionType);
            }

            else if (line.Contains("Provisioning Data Collection Process Starting")
                || line.Contains("Provisioning data collection process is completed")
                || line.Contains("Metering Data Collecting Process Starting as a Scheduled Job")
                || line.Contains("Metering data collection process is completed successfully"))
            {
                properLine = MakeLineProper(line);
                properLine = properLine[4].Split(' ');
                string convertedString = FirstLetterToUpper(properLine[1]);

                items = properLine[0] + ' ' + convertedString;

                string colType;

                switch (items)
                {
                    case "Metering Data":
                        colType = "Metering Collection";
                        break;

                    case "Provisioning Data":
                        colType = "Provisioning Collection";
                        break;

                    default:
                        colType = items;
                        break;
                }

                collectionType = colType;

                ExecuteForAgent(line, collectionType);
            }

            else if (line.Contains("Historical PMON/RMON Data Collect"))
            {
                properLine = MakeLineProper(line);
                properLine = properLine[4].Split(' ');
                items = properLine[4] + ' ' + properLine[5];

                string colType;
                switch (items)
                {
                    case "Historical PMON/RMON":
                        colType = "Historical PMON/RMON Data Collection";
                        break;

                    default:
                        colType = items;
                        break;
                }

                collectionType = colType;

                ExecuteForAgent(line, collectionType);
            }

            else if (line.Contains("Historical PMON/RMON data collecting process is completed successfully"))
            {
                properLine = MakeLineProper(line);
                properLine = properLine[4].Split(' ');
                items = properLine[0] + ' ' + properLine[1];

                string colType;
                switch (items)
                {
                    case "Historical PMON/RMON":
                        colType = "Historical PMON/RMON Data Collection";
                        break;

                    default:
                        colType = items;
                        break;
                }

                collectionType = colType;

                ExecuteForAgent(line, collectionType);
            }

            else if (line.Split(' ').Last().Contains("starting") == true || line.Contains("Logged out") == true || line.Contains("Agent: Logging in") == true)
            {
                ExecuteForAgent(line, "VAF Agent Session");
            }

            if (IsFileTransferEnabled == true) { FileTransferService(line); }
        }
        private void FileTransferService(string line)
        {
            // For file transfer 

            if (line.Contains("A file is started to send to UNMS VAF Module server. File Name:"))
            {
                properLine = MakeLineProper(line);

                int index = 0;
                var findType = "";
                var findSendItem = "";


                if (properLine[4].Contains("Prv"))
                {
                    findType = "Provisioning";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                else if (properLine[4].Contains("Inv"))
                {
                    findType = "Inventory";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                else if (properLine[4].Contains("Mtr-"))
                {
                    findType = "Metering";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                else if (properLine[4].Contains("pmon_1day_"))
                {
                    findType = "Pmon 1 day";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                else if (properLine[4].Contains("pmon_15min_"))
                {
                    findType = "Pmon 15 min";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                else if (properLine[4].Contains("rmon_1day_"))
                {
                    findType = "Rmon 1 day";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                else if (properLine[4].Contains("rmon_15min_"))
                {
                    findType = "Rmon 15 min";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                else if (properLine[4].Contains("EQP"))
                {
                    findType = "History Backup EQP";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }
                else if (properLine[4].Contains("NW"))
                {
                    findType = "History Backup NW";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }
                else if (properLine[4].Contains("USR"))
                {
                    findType = "History Backup USR";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                else if (properLine[4].Contains("Ping-"))
                {
                    findType = "Ping Test";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                else if (properLine[4].Contains("HBU-"))
                {
                    findType = "HisBU Summary Log";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                else if (properLine[4].Contains("vlan_1day_"))
                {
                    findType = "VLAN 1day";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = "VLAN 1day";
                }

                else if (properLine[4].Contains("vlan_15min_"))
                {
                    findType = "VLAN 15min";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = "VLAN 15min";
                }

                else if (properLine[4].Contains("grp_1day_"))
                {
                    findType = "GRP 1day";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = "GRP 1day";
                }

                else if (properLine[4].Contains("grp_15min_"))
                {
                    findType = "GRP 15min";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = "GRP 15min";
                }

                else if (properLine[4].Contains("lsp_1day_"))
                {
                    findType = "LSP 1day";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = "LSP 1day";
                }

                else if (properLine[4].Contains("lsp_15min_"))
                {
                    findType = "LSP 15min";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = "LSP 15min";
                }

                else
                {
                    findType = "Unknown";
                    index = line.LastIndexOf("\\") + 1;
                    findSendItem = line.Substring(index, line.Length - index);
                }

                TransferItems transferItem = new TransferItems
                {
                    Ip = properLine[2],
                    TransferType = findType,
                    FileName = properLine[4].Substring(properLine[4].LastIndexOf(':') + 1),
                    Date = properLine[0],
                    Started = DateTime.Parse(properLine[0]),
                    Completed = "Not Available",
                    SendItem = findSendItem
                };

                transferItems.Add(transferItem);
            }

            else if (line.Contains("A file is successfully sent to UNMS VAF Module server. File Name:"))
            {
                var fileName = "";
                var getlastWord = "";
                var agentIp = "";
                properLine = MakeLineProper(line);

                if (properLine[4].Contains("Inv") || properLine[4].Contains("Prv") ||
                   properLine[4].Contains("EQP") || properLine[4].Contains("NW") ||
                   properLine[4].Contains("USR"))
                {

                    fileName = properLine[4].Substring(properLine[4].LastIndexOf(':') + 1);
                    getlastWord = properLine[5].Split('\\').Last();
                    agentIp = properLine[2];

                    var value = transferItems.FirstOrDefault(x => x.Ip == agentIp && x.FileName == fileName &&
                                                             x.SendItem == getlastWord && x.Completed == "Not Available");
                    if (value != null)
                    {
                        value.Completed = DateTime.Parse(properLine[0]).ToString();
                        value.Duration = DateTime.Parse(value.Completed).Subtract(value.Started);
                    }
                }

                else if (properLine[4].Contains("HBU-") || properLine[4].Contains("vlan_1day_") ||
                         properLine[4].Contains("vlan_15min_") || properLine[4].Contains("grp_1day_") ||
                         properLine[4].Contains("grp_15min_") || properLine[4].Contains("lsp_1day_") ||
                         properLine[4].Contains("lsp_15min_") || properLine[4].Contains("pmon_1day_") ||
                         properLine[4].Contains("pmon_15min_") || properLine[4].Contains("rmon_1day_") ||
                         properLine[4].Contains("rmon_15min_") || properLine[4].Contains("Mtr-") ||
                         properLine[4].Contains("Ping-"))
                {
                    fileName = properLine[4].Substring(properLine[4].LastIndexOf(':') + 1);
                    getlastWord = properLine[5].Split('\\').Last();
                    agentIp = properLine[2];

                    var value = transferItems.FirstOrDefault(x => x.Ip == agentIp && x.FileName == fileName &&
                                                             x.Completed == "Not Available");
                    if (value != null)
                    {
                        value.Completed = DateTime.Parse(properLine[0]).ToString();
                        value.Duration = DateTime.Parse(value.Completed).Subtract(value.Started);
                    }
                }
            }
        }
        private void ExecuteForServer(string line, string colType)
        {
            if (line.Contains("started") == true
                || line.Contains("UNMS VAF Module service started successfully") == true)
            {
                CreateCollection(colType, line);
            }
            if (line.Contains("completed") == true
                || line.Contains("UNMS VAF Module service stopped") == true)
            {
                UpdateCollection(colType, line);
            }
        }
        private void ExecuteForAgent(string line, string colType)
        {
            if (line.Contains("Starting") == true || line.Contains("Historical PMON/RMON Data Collect") == true
                                                  || line.Contains("Agent: Logging in") == true)
            {
                CreateCollection(colType, line);
            }
            if (line.Contains("completed") == true || line.Contains("Logged out") == true)
            {
                UpdateCollection(colType, line);
            }
        }
        private void CreateCollection(string collectionType, string line)
        {
            string logType;

            if (line.Contains("Agent") || line.Contains("Agent: Logging in"))
            {
                logType = "VAF Agent";
            }
            else
            {
                logType = "VAF Service";
            }

            // Convert string to list for further process.
            properLine = MakeLineProper(line);

            if (properLine != null && logType != null)
            {
                // Add
                CollectionItem list = new CollectionItem()
                {
                    Ip = properLine[2],
                    Source = logType + "(" + properLine[2] + ")",
                    Date = DateTime.Parse(properLine[0]),
                    CollectionType = collectionType,
                    Started = DateTime.Parse(properLine[0]),
                    Completed = null
                };

                // Adding item list
                scheduledJobsList.Add(list);
            }

            else
            {
                Console.WriteLine("Opps!, \n Someting went wrong!");
            }
        }
        private void UpdateCollection(string collectionType, string line)
        {
            // Initialize local variables..
            string logType;
            var duration = new TimeSpan();
            var updateLine = new CollectionItem();

            // Make line clear for further processing.
            properLine = MakeLineProper(line);

            var currentIp = properLine[2];

            if (line.Contains("Agent"))
            {
                logType = "VAF Agent";
            }
            else
            {
                logType = "VAF Service";
            }

            // If collection starts and finish in regular time interval.
            if (scheduledJobsList.Any(x => x.CollectionType == collectionType && x.Ip == currentIp && x.Completed == null))
            {
                try
                {
                    // Geting candidate started collection for further processs
                    updateLine = scheduledJobsList.Where(c => c.CollectionType == collectionType && c.Completed == null).LastOrDefault();

                    if (updateLine != null)
                    {
                        // Adding completed time to related collection type.
                        duration = DateTime.Parse(properLine[0]).Subtract(updateLine.Started);
                    }
                    else
                    {
                        Console.WriteLine("Update line is null!!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An exception occured while updating the item: {0}", ex.Message.ToString());
                }
            }

            // If the collection does not have start time.
            else
            {
                updateLine.Ip = properLine[3];
                updateLine.CollectionType = collectionType;
                updateLine.Started = DateTime.MinValue;
                duration = TimeSpan.Zero;
            }

            //Adding prepared items to list.
            CollectionItem list = new CollectionItem()
            {
                Ip = properLine[2],
                Source = logType + "(" + properLine[2] + ")",
                Date = DateTime.Parse(properLine[0]),
                CollectionType = updateLine.CollectionType,
                Started = updateLine.Started,
                Completed = DateTime.Parse(properLine[0]),
                Duration = duration.ToString()
            };

            // Remove pre-created collection and updating with completed time 
            scheduledJobsList.Remove(updateLine);
            scheduledJobsList.Add(list);
        }
        public int SelectFileToProceed()
        {
            int count = 0;
            dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "LOG | *.log;", // file types, that will be allowed to upload
                Multiselect = true // allow / deny user to upload more than one file at a time
            };
            // Display OpenFileDialog by calling ShowDialog method
            bool? result = dialog.ShowDialog();

            if (result == true) // if user clicked OK
            {
                // Read the files
                foreach (string path in dialog.FileNames)
                {
                    count +=File.ReadLines(path).Count();
                }
            }
            return count;
        }

        #region tools
        private bool inBounds(int index, string[] array)
        {
            return (index >= 0) && (index < array.Length);
        }
        private string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
        private string[] MakeLineProper(string line)
        {
            var WordsArray = string.Join(" ", line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                                                                       .Select(i => i.Trim())).Split(',');
            return WordsArray;
        }
        public virtual bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
        private List<int> GetIntListFromStringList(List<string> stringValue)
        {
            var numbers = new List<int>();
            foreach (var item in stringValue)
            {
                Regex num = new Regex(@"\d+");
                if (item == null)
                {
                    Match portNum = num.Match("100");
                    numbers.Add(int.Parse(portNum.Value));
                }
                else if (item == "Not Available")
                {
                    Match portNum = num.Match("100");
                    numbers.Add(int.Parse(portNum.Value));
                }
                else if (String.IsNullOrEmpty(item))
                {
                    Match portNum = num.Match("100");
                    numbers.Add(int.Parse(portNum.Value));
                }
                else
                {
                    Match portNum = num.Match(item);
                    numbers.Add(int.Parse(portNum.Value));
                }
            }
            return numbers;
        }
        private int getintFromString(string str)
        {
            Regex num = new Regex(@"\d+");
            Match portNum = num.Match(str);

            int a;
            if (int.TryParse(portNum.Value, out a))
            {
                return int.Parse(portNum.Value);
            }
            else { return -1; }
        }

        #endregion

        #region LogAnalyzer Section
        protected dynamic PopulateDatas()
        {
            // Prepare list of summary data
            List<ExpandoObject> datas = new List<ExpandoObject>();

            var serverIps = new List<string>();
            var serverDataList = new List<CollectionItem>();
            var agentIps = new List<string>();

            serverIps = ServerAgentTable.Select(serverIp => serverIp.ServerId).Distinct().ToList();
            agentIps = ServerAgentTable.Where(s => s.ServerId == serverIps.ElementAt(0).ToString()).Select(a => a.AgentId).OrderBy(x => x).ToList();

            if (serverIps.Count != 0 && agentIps.Count != 0)
            {
                serverDataList = SummaryList().Where(s => s.Ip == serverIps.ElementAt(0).ToString()).OrderBy(x => x.CollectionType).ToList();

                if (serverDataList.Count != 0)
                {
                    for (int i = 0; i < serverDataList.Count; i++)
                    {
                        Dictionary<string, object> oneLine = new Dictionary<string, object>();
                        var oneLineObj = new ExpandoObject();

                        try
                        {
                            oneLine["CollectionType"] = serverDataList[i].CollectionType.ToString();
                            oneLine["Started_0"] = serverDataList[i].Started.ToString();
                            oneLine["Completed_0"] = serverDataList[i].Completed.ToString();
                            oneLine["Duration_0"] = serverDataList[i].Duration;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("An error occured while generating service items", ex);
                        }

                        for (int j = 0; j < agentIps.Count; j++)
                        {
                            if (!string.IsNullOrEmpty(serverDataList[i].Completed.ToString()))
                            {
                                var agentIP = agentIps[j];
                                var agentDataList = SummaryList().Where(s => s.Ip == agentIP).OrderBy(x => x.CollectionType).ToList();

                                // if collection doubles it will take last collection.
                                var oneAgentData = agentDataList.Where(c => c.CollectionType == serverDataList[i].CollectionType).ToList();

                                if (oneAgentData.Count != 0 && oneAgentData != null)
                                {
                                    var serviceIpStartTime = serverDataList[i].Started;
                                    var serviceIpCompleteTime = serverDataList[i].Completed;
                                    var serviceIpColType = serverDataList[i].CollectionType;

                                    List<CollectionItem> temp = new List<CollectionItem>();
                                    for (int k = 0; k < oneAgentData.Count; k++)
                                    {
                                        if (serviceIpColType != null && serviceIpStartTime != DateTime.MinValue && serviceIpCompleteTime != null)
                                        {
                                            if (oneAgentData[k].CollectionType == serviceIpColType && (oneAgentData[k].Started.AddMinutes(2) > serviceIpStartTime && oneAgentData[k].Completed < serviceIpCompleteTime))
                                            {
                                                temp.Add(oneAgentData[k]);
                                            }
                                        }

                                        else if (serviceIpColType != null && serviceIpCompleteTime != null && serviceIpStartTime == DateTime.MinValue)
                                        {
                                            if (oneAgentData[k].CollectionType == serviceIpColType && oneAgentData[k].Completed < serviceIpCompleteTime)
                                            {
                                                temp.Add(oneAgentData[k]);
                                            }
                                        }
                                    }

                                    if (temp.Count > 1)
                                    {
                                        var getLastItem = temp.LastOrDefault();
                                        temp.Clear();
                                        temp.Add(getLastItem);
                                        oneAgentData = temp;
                                    }

                                    else
                                    {
                                        oneAgentData = temp;
                                    }

                                    try
                                    {
                                        if (oneAgentData.Count() != 0 && oneAgentData != null)
                                        {
                                            oneLine["Started_" + (j + 1)] = oneAgentData.FirstOrDefault().Started.ToString();
                                            oneLine["Completed_" + (j + 1)] = oneAgentData.FirstOrDefault().Completed.ToString();
                                            oneLine["Duration_" + (j + 1)] = oneAgentData.FirstOrDefault().Duration;
                                        }

                                        else
                                        {
                                            oneLine["Started_" + (j + 1)] = "*Not Available";
                                            oneLine["Completed_" + (j + 1)] = "*Not Available";
                                            oneLine["Duration_" + (j + 1)] = "*Not Available";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("An error occured while summary table being generated.", ex);
                                    }
                                }

                                else
                                {
                                    oneLine["Started_" + (j + 1)] = "Not Available";
                                    oneLine["Completed_" + (j + 1)] = "Not Available";
                                    oneLine["Duration_" + (j + 1)] = "Not Available";
                                }
                            }

                            else
                            {
                                oneLine["Completed_0"] = "Not Available";
                                oneLine["Duration_0"] = "Not Available";
                                oneLine["Started_" + (j + 1)] = "Not Available";
                                oneLine["Completed_" + (j + 1)] = "Not Available";
                                oneLine["Duration_" + (j + 1)] = "Not Available";
                            }
                        }

                        oneLineObj = HelperExpend.ToExpando(oneLine);

                        if (oneLineObj != null)
                        {
                            datas.Add(oneLineObj);
                        }
                    }
                }
            }

            return datas;
        }
        private List<CollectionItem> SummaryList()
        {
            // get data for summary collection tab
            var colSumList = scheduledJobsList.Where(x => x.CollectionType != "Metering Collection"
                                                            && x.CollectionType != "Actual Data Collection"
                                                            && x.CollectionType != "Actual Data Transfer"
                                                            && x.CollectionType != "History Backup"
                                                            && x.CollectionType != "Inventory Mergining"
                                                            && x.CollectionType != "Provisioning Mergining"
                                                            && x.CollectionType != "Historical Data Transfer"
                                                            && x.CollectionType != "VAF Agent Session"
                                                            && x.CollectionType != "VAF Service Session")
                                                            .ToList();
            return colSumList;
        }
        // This method filters VAF Sessions from whole collection.
        public List<ServiceSession> serviceSessions(string serverIp, string colType)
        {
            if (scheduledJobsList.Any(ip => ip.Ip == serverIp))
            {
                var serviceSession = new List<ServiceSession>();

                int sessionId = 1;
                foreach (var item in scheduledJobsList.Where(c => c.CollectionType == colType && c.Ip == serverIp).ToList())
                {
                    var srvcSession = new ServiceSession();

                    if (item.Duration == "00:00:00" || item.Duration == null)
                    {
                        srvcSession.Duration = "Not Available";
                    }

                    else
                    {
                        srvcSession.Duration = item.Duration;
                    }

                    if (item.Started == DateTime.MinValue || item.Started == null)
                    {
                        srvcSession.StartTime = "Unknown";
                    }

                    else
                    {
                        srvcSession.StartTime = item.Started.ToString();
                    }

                    if (item.Completed == DateTime.MinValue || item.Completed == null)
                    {
                        srvcSession.StopTime = "Unknown";
                    }

                    else
                    {
                        srvcSession.StopTime = item.Completed.ToString();
                    }

                    srvcSession.Source = item.Source;
                    srvcSession.Date = item.Date;
                    srvcSession.SessionNo = sessionId++;
                    serviceSession.Add(srvcSession);
                }

                return serviceSession.ToList();
            }

            else
            {
                return null;
            }
        }
        #endregion

        #region Missing FarEnd Section
        private void InsertRadioConfDataToNEList(MissingOpposite preparedNEListItems, bool islast)
        {
            //Updating Prepared Nelist with Radio Configuration data.
            try
            {
                var nearEndTXFrequency = RadioConfigurationItems.Where(x => x.IpAddress == preparedNEListItems.IP && x.Port == preparedNEListItems.Port)
                                       .Select(o => new { o.Port, o.TXRFFrequency }).FirstOrDefault();

                // If Temp Ip is not null and TX frequency changed means port or Ip changed
                if (!String.IsNullOrEmpty(tempIp) && (preparedNEListItems.IP != tempIp
                    || (preparedNEListItems.Port != tempPort && nearEndTXFrequency.TXRFFrequency != tempTX)))
                //|| nearEndTXFrequency.TXRFFrequency != tempTX))
                {
                    //Define SWGroups and opposite radio conf ports
                    DefineOppPorts();
                }
                try
                {
                    if (nearEndTXFrequency != null)
                    {
                        var oppositePortsAndRXFrequencies = RadioConfigurationItems.Where(x => x.IpAddress == preparedNEListItems.OppIP && x.RXRFFrequency == nearEndTXFrequency.TXRFFrequency)
                                                .Select(o => new { o.IpAddress, o.Port, o.RXRFFrequency }).ToList();

                        if (nearEndTXFrequency != null && oppositePortsAndRXFrequencies.Count != 0)
                        {
                            Regex nPort = new Regex(@"\d+");
                            Match radioConfPort = nPort.Match(nearEndTXFrequency.Port);

                            var nearEndIp = new RadioConfItems();
                            nearEndIp.IpAddress = preparedNEListItems.IP;
                            nearEndIp.Port = radioConfPort.Value;
                            nearEndIp.TXRFFrequency = nearEndTXFrequency.TXRFFrequency;
                            nearEndIp.OppIpAddress = preparedNEListItems.OppIP;
                            newList.Add(nearEndIp);

                            if ((preparedNEListItems.IP != tempIp && nearEndTXFrequency.TXRFFrequency != tempTX)
                                || (preparedNEListItems.Port != tempPort && nearEndTXFrequency.TXRFFrequency != tempTX))
                            {
                                foreach (var item in oppositePortsAndRXFrequencies)
                                {
                                    var getOppIp = missingOpposites.Where(x => x.IP == item.IpAddress && x.Port == item.Port).Select(y => y.OppIP).FirstOrDefault();

                                    Regex rOppPort = new Regex(@"\d+");
                                    Match radioConfOppPort = rOppPort.Match(item.Port);
                                    var farEndIp = new RadioConfItems
                                    {
                                        Port = radioConfOppPort.Value,
                                        RXRFFrequency = item.RXRFFrequency,
                                        IpAddress = item.IpAddress,
                                        OppIpAddress = getOppIp != null ? getOppIp : ""
                                    };

                                    newList.Add(farEndIp);
                                }
                            }

                            tempIp = preparedNEListItems.IP;
                            tempTX = nearEndTXFrequency.TXRFFrequency;
                            tempPort = preparedNEListItems.Port;

                            if (islast)
                            {
                                DefineOppPorts();
                            }
                        }

                        else { preparedNEListItems.OppPortInRadio = "Not Available"; }
                    }
                    else { preparedNEListItems.OppPortInRadio = "Not Available"; }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Opposite adding from radio conf.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        private void DefineOppPorts()
        {
            DefineSwGroupsByPort();
            // this for finding opp port of Near End ports.
            SepareteNearEndFarEndPorts();
            try
            {
                // Defining Opposite SW Groups and ports for near end modems.
                if (selectFarEnd.Count != 0)
                {
                    for (int i = 0; i < selectNearEnd.Count; i++)
                    {

                        // TODO check if there is issue
                        var slctFarEnd = selectFarEnd.Where(x => x.OppIpAddress == selectNearEnd[i].IpAddress).ToList();

                        if (selectNearEnd.Count != slctFarEnd.Count)
                        {
                            Console.WriteLine("ddd");
                        }

                        if (selectNearEnd.Count != slctFarEnd.Count)
                        {
                            var dif = selectNearEnd.Count - slctFarEnd.Count;

                            if (dif > 0)
                            {
                                var newInstance = new RadioConfItems();
                                for (int k = 0; k < dif; k++)
                                {
                                    slctFarEnd.Add(newInstance);
                                }
                            }
                        }

                        if (slctFarEnd.Count != 0)
                        {

                            if (i != 0 && selectNearEnd.ElementAtOrDefault(i - 1) != null && selectNearEnd[i].SWGroup == selectNearEnd[i - 1].SWGroup)
                            {
                                selectNearEnd[i].OppSWGroup = selectNearEnd[i - 1].OppSWGroup;
                                selectNearEnd[i].OppPort = getOppositePort(selectNearEnd[i].OppSWGroup);
                            }
                            else
                            {
                                selectNearEnd[i].OppSWGroup = !String.IsNullOrEmpty(slctFarEnd[i].SWGroup) ? slctFarEnd[i].SWGroup : "";
                                selectNearEnd[i].OppPort = getOppositePort(selectNearEnd[i].OppSWGroup);
                            }

                            //// get only lists that match 
                            //if (selectNearEnd.Count == slctFarEnd.Count)
                            //{
                            //    selectNearEnd[i].OppSWGroup = slctFarEnd[i].SWGroup;
                            //    selectNearEnd[i].OppPort = getOppositePort(selectNearEnd[i].OppSWGroup);
                            //}
                            //else
                            //{
                            //    // if item is not the first one Check if previous element exist on the list / if yes continue..
                            //    if (i != 0 && selectNearEnd.ElementAtOrDefault(i - 1) != null && selectNearEnd[i].SWGroup == selectNearEnd[i - 1].SWGroup)
                            //    {
                            //        selectNearEnd[i].OppSWGroup = selectNearEnd[i - 1].OppSWGroup;
                            //        selectNearEnd[i].OppPort = getOppositePort(selectNearEnd[i].OppSWGroup);
                            //    }

                            //    else
                            //    {
                            //        selectNearEnd[i].OppSWGroup = slctFarEnd[i].SWGroup;
                            //        selectNearEnd[i].OppPort = getOppositePort(selectNearEnd[i].OppSWGroup);
                            //    }

                            //    Console.WriteLine("Ip: ", selectNearEnd[i].IpAddress, "Opp Ip: ", slctFarEnd[i].IpAddress);
                            //}




                            //if (i == 0)
                            //{
                            //    selectNearEnd[i].OppSWGroup = slctFarEnd[i].SWGroup;
                            //    selectNearEnd[i].OppPort = getOppositePort(selectNearEnd[i].OppSWGroup);
                            //}
                            //else if (selectNearEnd[i].SWGroup == selectNearEnd[i - 1].SWGroup)
                            //{
                            //    selectNearEnd[i].OppSWGroup = selectNearEnd[i - 1].SWGroup;
                            //    selectNearEnd[i].OppPort = getOppositePort(selectNearEnd[i].OppSWGroup);
                            //}

                            //if (slctFarEnd.Count < i)
                            //{
                            //    if (selectNearEnd[i - 1] != null && (selectNearEnd[i - 1].OppSWGroup != slctFarEnd[i].SWGroup))
                            //    {
                            //        selectNearEnd[i].OppSWGroup = slctFarEnd[i].SWGroup;
                            //        selectNearEnd[i].OppPort = getOppositePort(selectNearEnd[i].OppSWGroup);
                            //    }

                            //    if (slctFarEnd.Count < (i + 1))
                            //    {
                            //        if (selectNearEnd[i - 1] != null && (selectNearEnd[i - 1].OppSWGroup != slctFarEnd[i + 1].SWGroup))
                            //        {
                            //            selectNearEnd[i].OppSWGroup = slctFarEnd[i + 1].SWGroup;
                            //            selectNearEnd[i].OppPort = getOppositePort(selectNearEnd[i].OppSWGroup);
                            //        }
                            //    }
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "The code blow ups here!");
                throw;
            }

            updateRadioConfOnNEList(selectNearEnd);
            newList = new List<RadioConfItems>();
        }
        private void SepareteNearEndFarEndPorts()
        {
            var selectAdressFromNewlist = newList.Select(y => y.IpAddress).ToList();
            try
            {
                for (int i = 0; i < selectAdressFromNewlist.Distinct().Count(); i++)
                {
                    if (i == 0)
                    {
                        selectNearEnd = newList.Where(x => x.IpAddress == selectAdressFromNewlist[i]).ToList();
                    }
                    else if (i == 1)
                    {
                        selectFarEnd = newList.Where(x => x.IpAddress == selectAdressFromNewlist[i]).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The code blows up here!");
                throw;
            }
        }
        private void updateRadioConfOnNEList(List<RadioConfItems> selectNearEnd)
        {
            try
            {
                if (selectNearEnd.Count != 0)
                {
                    foreach (var nearEnditem in selectNearEnd)
                    {
                        if (nearEnditem.OppPort == null)
                        {
                            nearEnditem.OppPort = "Not Available";
                        }
                        foreach (var item in missingOpposites.Where(x => x.IP == nearEnditem.IpAddress).ToList())
                        {
                            if (nearEnditem.IpAddress == item.IP && "MODEM (" + "Slot" + nearEnditem.Port.ToString() + ")" == item.Port)
                            {
                                if (getintFromString(nearEnditem.OppPort) == getintFromString(item.OppPort) || getintFromString(item.OppPort) == (getintFromString(nearEnditem.OppPort) + 1))
                                {
                                    item.OppPortInRadio = item.OppPort;
                                }
                                else { item.OppPortInRadio = nearEnditem.OppPort; }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("updateRadioConfOnNEList");
                throw;
            }
        }
        private string getOppositePort(string oppPortNum)
        {
            if (!String.IsNullOrEmpty(oppPortNum))
            {
                Regex oppTmp = new Regex(@"\d+");
                Match oppEndSWValue = oppTmp.Match(oppPortNum);

                switch (int.Parse(oppEndSWValue.Value))
                {
                    case 01:
                        return "MODEM (Slot01)";
                    case 02:
                        return "MODEM (Slot03)";
                    case 03:
                        return "MODEM (Slot05)";
                    case 04:
                        return "MODEM (Slot07)";
                    case 05:
                        return "MODEM (Slot09)";
                    case 06:
                        return "MODEM (Slot11)";
                    case 07:
                        return "MODEM (Slot13)";
                    default:
                        return "Not Found!";
                }
            }
            else { return ""; }
        }
        private void PrepareListForUniqueMatchInOpposite()
        {
            var uniqueIds = missingOpposites.Select(x => x.IP).Distinct();

            foreach (var ıds in uniqueIds)
            {
                // Marking Cross Connected Ports
                var getIpDetails = missingOpposites.Where(x => x.IP == ıds).Select(a => new { a.OppIP, a.Port, a.OppPortInRadio }).ToList();

                string temp = "";
                var getUniqueDetails = new List<MissingOpposite>();
                foreach (var item in getIpDetails)
                {
                    if (String.IsNullOrEmpty(temp))
                    {
                        temp = item.OppIP;

                        var missingOpp = new MissingOpposite()
                        {
                            Port = item.Port,
                            OppPort = item.OppPortInRadio != null ? item.OppPortInRadio : ""
                        };
                        getUniqueDetails.Add(missingOpp);

                        // if there is only one connection
                        if (getIpDetails.Count == 1)
                        {
                            MarkCrossConnectedPorts(getUniqueDetails, ıds);
                        }
                    }

                    else if (!String.IsNullOrEmpty(temp) && item.OppIP == temp)
                    {
                        var missingOpp = new MissingOpposite()
                        {
                            Port = item.Port,
                            OppPort = item.OppPortInRadio != null ? item.OppPortInRadio : ""
                        };
                        getUniqueDetails.Add(missingOpp);
                    }

                    else if (!String.IsNullOrEmpty(temp) && item.OppIP != temp)
                    {
                        // Bunun anlamı near ıp farklı bir opp Ip ye geçti.
                        // hazırlanan listeyi kullan crosslara bak!
                        MarkCrossConnectedPorts(getUniqueDetails, ıds);

                        // listeyi sıfırla
                        getUniqueDetails = new List<MissingOpposite>();

                        // yeni gelen port ile yeni opposite leri eşleştirmeye başla
                        var missingOpp = new MissingOpposite()
                        {
                            Port = item.Port,
                            OppPort = item.OppPortInRadio
                        };
                        getUniqueDetails.Add(missingOpp);
                        temp = item.OppIP;
                    }
                }
            }
        }
        private void DefineSwGroupsByPort()
        {
            try
            {
                //Defining SW Groups into separete list.
                foreach (var item in newList)
                {
                    switch (int.Parse(item.Port))
                    {
                        case 01:
                            item.SWGroup = "SW1";
                            break;
                        case 02:
                            item.SWGroup = "SW1";
                            break;
                        case 03:
                            item.SWGroup = "SW2";
                            break;
                        case 04:
                            item.SWGroup = "SW2";
                            break;
                        case 05:
                            item.SWGroup = "SW3";
                            break;
                        case 06:
                            item.SWGroup = "SW3";
                            break;
                        case 07:
                            item.SWGroup = "SW4";
                            break;
                        case 08:
                            item.SWGroup = "SW4";
                            break;
                        case 09:
                            item.SWGroup = "SW5";
                            break;
                        case 10:
                            item.SWGroup = "SW5";
                            break;
                        case 11:
                            item.SWGroup = "SW6";
                            break;
                        case 12:
                            item.SWGroup = "SW6";
                            break;
                        case 13:
                            item.SWGroup = "SW7";
                            break;
                        case 14:
                            item.SWGroup = "SW7";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        private void MarkCrossConnectedPorts(List<MissingOpposite> getUniqueDetails, string ıds)
        {
            int tempValue = 0;
            int tempOfTemp = 0;
            int count = 0;

            foreach (var port in getUniqueDetails)
            {
                if (tempValue != 0 && getintFromString(port.Port) >= tempValue)
                {
                    if (getintFromString(port.OppPort) != 100)
                        tempOfTemp = getintFromString(port.OppPort);
                }

                if (count == 0)
                {
                    if (getintFromString(port.OppPort) != 100)
                    {
                        tempValue = getintFromString(port.OppPort);
                    }
                }
                else if (count >= 1)
                {
                    if (getintFromString(port.OppPort) != 100 && getintFromString(port.OppPort) < tempValue)
                    {
                        //var status = missingOpposites.Where(x => x.IP == ıds && x.Port == "MODEM (" + "Slot" + port.NearPort.ToString("D2") + ")").SingleOrDefault();
                        var status = missingOpposites.Where(x => x.IP == ıds && x.Port == port.Port).SingleOrDefault();
                        status.Status = "Cross Connection detected";
                    }
                }
                if (getintFromString(port.OppPort) != 100)
                    count++;

                // This will work when the near and slot become bigger than previous far end slot
                if (tempOfTemp != 0)
                {
                    tempValue = tempOfTemp;
                    tempOfTemp = 0;
                }
            }
        }
        #endregion
    }
}
