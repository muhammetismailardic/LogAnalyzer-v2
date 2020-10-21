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
        // Insert Transfer files......
        public List<TransferItems> transferItems;
        public bool IsFileTransferEnabled = false;

        // Insert all the logs based on type into one big list.
        public List<string> logList;
        public List<string> NEList;
        public List<string> RmonData;
        public List<ServerAgentCollection> ServerAgentTable;
        public List<CollectionItem> scheduledJobsList;
        public List<MissingOpposite> missingOpposites = new List<MissingOpposite>();
        List<RmonItems> RmonItems = new List<RmonItems>();

        private string[] properLine;
        private string collectionType, items;

        protected void ReadCSVFile(object sender, DoWorkEventArgs e, BackgroundWorker bw)
        {
            int max = (int)e.Argument;
            int result = 0;
            int counter = 0;

            if (logList != null && logList.Count != 0)
            {
                AnalyzeLog(max, result, counter, e, bw);
            }
            else if (NEList != null && NEList.Count != 0) //&& RmonData != null && RmonData.Count != 0)
            {
                AnalyzeOppositeInfo(max, result, counter, e, bw);
            }
        }

        private void AnalyzeOppositeInfo(int max, int result, int counter, DoWorkEventArgs e, BackgroundWorker bw)
        {
            missingOpposites = new List<MissingOpposite>();

            AnalyzeNEList(max, result, counter, e, bw);
            AnalyzeNEListForOpposite();



            AnalyzeOppositeInfoRmon(max, result, counter, e, bw);
            UpdatingTheListWithRmonFile();
            
        }

        private void AnalyzeNEListForOpposite()
        {
            List<string> IpWithOppIp = new List<string>();

            

            foreach (var item in missingOpposites)
            {
                //var test = missingOpposites.Where(x => x.IP == item.IP).Select(y=> y.Port)

                var FindIp = item.IP;
                var findPort = item.Port;

            }
            
            
            
        }

        private void UpdatingTheListWithRmonFile()
        {
            // TODO 15 dakikalıklar için bakılacak
            foreach (var item in missingOpposites)
            {
                var selectedOppPort = RmonItems.Where(x => x.IP == item.IP && x.Port == item.Port).Select(a => new { a.OppIP, a.OppPort, a.GroupMember, a.Time }).ToList();

                if (selectedOppPort.Count == 1)
                {
                    var theSelected = selectedOppPort.SingleOrDefault();

                    //Update the selected list if exist.
                    if (selectedOppPort != null)
                    {
                        if (theSelected.OppIP == item.OppIP)
                        {
                            item.IsMatch = true;
                        }
                        item.OppIP = theSelected.OppIP;
                        item.Time = theSelected.Time;
                        item.OppPort = theSelected.OppPort;
                        item.HasRmon = true;
                        item.GroupMember = theSelected.GroupMember;

                        if (item.IsMatch == true && item.HasRmon == true)
                        {
                            item.Status = "Ok";
                        }
                    }
                }

                else
                {
                    foreach (var slctPort in selectedOppPort)
                    {
                        if (slctPort != null)
                        {
                            if (slctPort.OppIP == item.OppIP)
                            {
                                item.IsMatch = true;
                            }
                            item.OppIP = slctPort.OppIP;
                            item.Time = slctPort.Time;
                            item.OppPort = slctPort.OppPort;
                            item.HasRmon = true;
                            item.GroupMember = slctPort.GroupMember;

                            if (item.IsMatch == true && item.HasRmon == true)
                            {
                                item.Status = "Ok";
                            }
                        }
                    }
                }
            }
        }
        private void AnalyzeOppositeInfoRmon(int max, int result, int counter, DoWorkEventArgs e, BackgroundWorker bw)
        {
            List<string> RmonIndexLoc = new List<string>();
            List<string> RmonOppIps = new List<string>();


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

                    // Find Index
                    var items = line.Split(',');

                    if (RmonData.IndexOf(line) == 0)
                    {
                        int loc = 0;
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (items[i].Equals("IP Address"))
                            {
                                //RmonIndexLoc.Add("IP Address," + loc);
                                RmonOppIps.Add("IP Address," + loc);
                            }

                            if (items[i].Equals("Port"))
                            {
                                //RmonIndexLoc.Add("Port," + loc);
                                RmonOppIps.Add("Port," + loc);
                            }

                            if (items[i].Equals("Opposite NE IP Address"))
                            {
                                //RmonIndexLoc.Add("Opposite NE IP Address," + loc);
                                RmonOppIps.Add("Opposite NE IP Address," + loc);
                            }

                            if (items[i].Equals("Opposite Port"))
                            {
                                //RmonIndexLoc.Add("Opposite Port," + loc);
                                RmonOppIps.Add("Opposite Port," + loc);
                            }

                            if (items[i].Equals("Group Member"))
                            {
                                //RmonIndexLoc.Add("Group Member," + loc);
                                RmonOppIps.Add("Group Member," + loc);
                            }
                            loc++;
                        }
                    }

                    // Find Opp IPs using Index
                    if (RmonData.IndexOf(line) != 0)
                    {
                        // Define Location of Items
                        //foreach (var index in RmonIndexLoc)
                        //{
                        //    var itemName = index.Split(',')[0];

                        //    if (!String.IsNullOrEmpty(itemName))
                        //    {
                        //        if (itemName == "IP Address")
                        //        {
                        //            RmonOppIps.Add("IP Address," + items[(int.Parse(index.Split(',')[1]))]);
                        //        }

                        //        else if (itemName == "Port")
                        //        {
                        //            RmonOppIps.Add("Port," + items[(int.Parse(index.Split(',')[1]))]);
                        //        }

                        //        else if (itemName == "Opposite NE IP Address")
                        //        {
                        //            RmonOppIps.Add("Opposite NE IP Address," + items[(int.Parse(index.Split(',')[1]))]);
                        //        }

                        //        else if (itemName == "Opposite Port")
                        //        {
                        //            RmonOppIps.Add("Opposite Port," + items[(int.Parse(index.Split(',')[1]))]);
                        //        }

                        //        else if (itemName == "Group Member")
                        //        {
                        //            RmonOppIps.Add("Group Member," + items[(int.Parse(index.Split(',')[1]))]);
                        //        }
                        //    }
                        //}

                        // Insert to List
                        var rmonItems = new RmonItems();
                        foreach (var item in RmonOppIps)
                        {
                            if (item.Split(',')[0] == "IP Address")
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

                        //Adding items to list.
                        RmonItems.Add(rmonItems);
                    }
                }
               //RmonOppIps.Clear();
            }
        }
        private void AnalyzeNEList(int max, int result, int counter, DoWorkEventArgs e, BackgroundWorker bw)
        {
            List<string> NeListOppIps = new List<string>();
            string primaryAdd = "";
            List<int> NeListOppLoc = new List<int>();
            int primaryAddLoc = 3;

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

                    // TODO // Define opp Ip Loc
                    var items = line.Split(',');
                    if (NEList.IndexOf(line) == 1)
                    {
                        int loc = 0;
                        for (int i = 0; i < items.Length; i++)
                        {
                            //Console.WriteLine("Index: " + loc++ + " Item Name: " + items[i].ToString() + "\n");
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
                        if (primaryAddLoc != -1)
                            primaryAdd = items[primaryAddLoc].ToString();

                        foreach (var index in NeListOppLoc)
                        {
                            if (!String.IsNullOrEmpty(items[index].ToString()))
                            {
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
                                Status = "No Data",
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
                NeListOppIps.Clear();
            }
        }
        private void AnalyzeLog(int max, int result, int counter, DoWorkEventArgs e, BackgroundWorker bw)
        {
            foreach (string line in logList)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    counter++;
                    int progressPercentage = Convert.ToInt32((counter * 100) / max);

                    if (counter % 42 == 0)
                    {
                        result++;
                        bw.ReportProgress(progressPercentage);
                    }

                    if (line.Contains("UNMS VAF Module Service"))
                    {
                        VAFServerCollection(line);
                    }

                    else if (line.Contains("UNMS VAF Module Agent"))
                    {
                        VAFAgentCollection(line);
                    }
                    e.Result = result;
                }
            }
        }
        private void VAFServerCollection(string line)
        {
            if (!line.Contains("UNMS VAF Module Service,An agent service stopped and started again"))
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

        #region tools
        private string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
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
        #endregion
    }
}
