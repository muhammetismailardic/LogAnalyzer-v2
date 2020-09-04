using LogAnalyzerV2.Models;
using LogAnalyzerV2.Models.HelperClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.Linq;
using System.Text;
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
        public List<ServerAgentCollection> ServerAgentTable;
        public List<CollectionItem> scheduledJobsList;

        private string[] properLine;
        private string collectionType, items;

        protected void ReadCSVFile(object sender, DoWorkEventArgs e, BackgroundWorker bw)
        {
            int max = (int)e.Argument;
            int result = 0;
            int counter = 0;

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
            if (!line.Contains("An agent service started to work during the job is in progress."))
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
                        colType = "Historical Data Collection";
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
                        colType = "Historical Data Collection";
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
        #endregion
    }
}
