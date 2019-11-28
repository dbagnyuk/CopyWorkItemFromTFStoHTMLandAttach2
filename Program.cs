using System;
using System.Net;
using System.IO;
using System.Threading;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace CopyWorkItemFromTFStoHTMLandAttach2
{
    class Program
    {
        public const string Key = "pass"; // key word for Encrypt/Decrypt

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "CopyWorkItemFromTFStoHTMLandDownloadAttachment";

            string configFile = @"CopyWorkItemFromTFStoHTMLandAttach2.conf";
            int itemId;
            string[] config = null; // string array for read the config

            while (true)
            {
                // read the config file until success
                if (Config.callReadConfig(configFile, ref config))
                    break;
            }
            while (true)
            {
                Console.WriteLine("Enter TFS number or 'config' for modify [login/password/path to attach] in config file.\n");
                Console.Write("Input TFS id (or 'config'): ");
                //int itemId = 1936268;
                itemId = Input.processingInput();

                if (itemId == -1)
                {
                    Config.callEditConfig(configFile);
                    while (true)
                    {
                        // read the config file until success
                        if (Config.callReadConfig(configFile, ref config))
                            break;
                    }
                    continue;
                }
                break;
            }

            // save the configuration for operate with
            // domain and login save from config as is
            string DomainName = config[0];
            // decrypting password from config
            string Password = Cipher.Decrypt(config[1], Key);
            // add the last '\' to path if it's not entered by user in config
            string pathToTasks = (config[2].EndsWith("\\") ? config[2] : config[2] + "\\");

            // ask if user want to download the attachments or not
            Console.Clear();
            Console.Write("Download the Attachments? (y/n): ");
            bool confirm = Input.downloadConfirm();
            // current state of program progress
            Console.Clear();
            Console.Write("Creating the HTML file...");

            // create the connection to the TFS server
            NetworkCredential netCred = new NetworkCredential(DomainName, Password);
            Microsoft.VisualStudio.Services.Common.WindowsCredential winCred = new Microsoft.VisualStudio.Services.Common.WindowsCredential(netCred);
            VssCredentials vssCred = new VssCredentials(winCred);
            TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(new Uri("https://tfs.mtsit.com/STS/"), vssCred);

            // catch the authentication error
            try
            {
                tpc.Authenticate();
            }
            catch (Exception ex)
            {
                exExit(ex);
            }

            WorkItemStore workItemStore = tpc.GetService<WorkItemStore>();
            WorkItem workItem = null;

            // catch not existed TFS id
            try
            {
                workItem = workItemStore.GetWorkItem(itemId);
            }
            catch (Exception ex)
            {
                exExit(ex);
            }

            // create web link for tfs id
            string tfsLink = tpc.Uri + workItem.AreaPath.Remove(workItem.AreaPath.IndexOf((char)92)) + "/_workitems/edit/";
            // create path and name to html file
            string pathToHtml = pathToTasks + workItem.Type.Name + " " + workItem.Id + ".html";
            // create path and folder name for attachments
            string pathToAttach = pathToTasks + workItem.Id;

            FileStream fileStream = null;
            StreamWriter streamWriter = null;

            // create/open the html file for write in to it
            if (File.Exists(pathToHtml))
                fileStream = new FileStream(pathToHtml, FileMode.Truncate);
            else
                fileStream = new FileStream(pathToHtml, FileMode.CreateNew);
            streamWriter = new StreamWriter(fileStream);

            // fill in the html file
            // head, title, encoding
            streamWriter.WriteLine("{0}", "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
            streamWriter.WriteLine("{0}", "<html>");
            streamWriter.WriteLine("<head>{0}</head>", "<meta charset=\"UTF-8\">");
            streamWriter.WriteLine("<title>{0} {1}</title>", workItem.Type.Name, workItem.Id);
            streamWriter.WriteLine("{0}", "<body>");
            streamWriter.WriteLine("{0}", "");
            // short info block: 'name', 'id', 'title', 'state' and 'assigned to' info of work item
            streamWriter.WriteLine(@"<p><font style=""background-color:rgb(255, 255, 255); color:rgb(0, 0, 0); font-family:Segoe UI; font-size:12px;"">"
                                   + workItem.Type.Name + " " + workItem.Id + ": " + workItem.Title
                                   + @"</font></p>");
            streamWriter.WriteLine(@"<p style=""border: 1px solid; color: red; width: 50%;"">"
                                   + @"<font style=""background-color:rgb(255, 255, 255); color:rgb(0, 0, 0); font-family:Segoe UI; font-size:12px;"">"
                                   + workItem.Type.Name + " is <b>" + workItem.State
                                   + (workItem.State == "Closed" ? "</b>" : "</b> and Assigned To <b>" + workItem.Fields["Assigned To"].Value + "</b>")
                                   + @"</font></p>");
            // blok 'title'
            streamWriter.WriteLine(@"<div style=""border: 1px solid black; background-color:lightgray;"">TITLE:</div>");
            streamWriter.WriteLine("<p>{0}</p>", workItem.Title);
            // block 'description'
            streamWriter.WriteLine(@"<div style=""border: 1px solid black; background-color:lightgray;"">DESCRIPTION:</div>");
            if (workItem.Type.Name == "Bug" || workItem.Type.Name == "Issue")
                streamWriter.WriteLine(workItem.Fields["REPRO STEPS"].Value);
            else if (workItem.Type.Name == "Task")
                streamWriter.WriteLine(workItem.Fields["DESCRIPTION"].Value);
            // block 'history'
            streamWriter.WriteLine(@"<div style=""border: 1px solid black; background-color:lightgray;"">HISTORY:</div><br>");
            for (int i = workItem.Revisions.Count - 1; i >= 0; i--)
            {
                streamWriter.WriteLine(@"<font style=""background-color:rgb(255, 255, 255); color:rgb(0, 0, 0); font-family:Segoe UI; font-size:12px; font-weight:bold;"">"
                                       + workItem.Revisions[i].Fields["Changed By"].Value
                                       + @"</font><br>");
                if (workItem.Revisions[i].Fields["History"].Value.Equals(""))
                    streamWriter.WriteLine(workItem.Revisions[i].Fields["History"].Value);
                else
                    streamWriter.WriteLine(workItem.Revisions[i].Fields["History"].Value
                                           + "<br>");
                streamWriter.WriteLine(@"<font style=""background-color:rgb(255, 255, 255); color:rgb(128, 128, 128); font-family:Segoe UI; font-size:12px;"">"
                                       + "&nbsp;"
                                       + workItem.Revisions[i].Fields["State Change Date"].Value
                                       + @"</font><br><br>");
            }
            // block with linked work items. in table
            streamWriter.WriteLine(@"<div style=""border: 1px solid black; background-color:lightgray;"">ALL LINKS:</div>");
            streamWriter.WriteLine(@"<p><table style=""width:100%; font-family:Segoe UI; font-size:12px;"">");
            streamWriter.WriteLine(@"<tr><th align=""left"">Link Type</th>
                                         <th align=""left"">Work Item Type</th>
                                         <th align=""left"">ID</th>
                                         <th align=""left"">State</th>
                                         <th align=""left"">Title</th>
                                         <th align=""center"">Assigned To</th></tr>");
            foreach (WorkItemLink link in workItem.WorkItemLinks)
            {
                WorkItem wiDeliverable = workItemStore.GetWorkItem(link.TargetId);
                streamWriter.WriteLine(@"<tr><td>{0}</td>", link.LinkTypeEnd.Name);
                streamWriter.WriteLine(@"<td>{0}</td>", wiDeliverable.Type.Name);
                streamWriter.WriteLine(@"<td><a href=""{0}{1}"">{1}</a></td>", tfsLink, wiDeliverable.Id);
                streamWriter.WriteLine(@"<td>{0}</td>", wiDeliverable.State);
                streamWriter.WriteLine(@"<td>{0}</td>", wiDeliverable.Title);
                streamWriter.WriteLine(@"<td>{0}</td></tr>", wiDeliverable.Fields["Assigned To"].Value);
            }
            streamWriter.WriteLine(@"</table></p>");
            // block with the web link to the thin client on to work item
            streamWriter.WriteLine(@"<div style=""border: 1px solid black; background-color:lightgray;"">LINK:</div>");
            streamWriter.WriteLine(@"<p><a href=""{0}{1}"">{0}{1}</a></p>", tfsLink, workItem.Id);

            // current state of program progress
            Console.Clear();
            Console.Write("Search folder for attach...");
            Thread.Sleep(700);

            // create the path to directory for saving attachments and search if the dir alredy exist
            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(pathToTasks);
            FileSystemInfo[] filesAndDirs = hdDirectoryInWhichToSearch.GetFileSystemInfos("*" + workItem.Id + "*");

            // if folder for attach alredy exists change the the default name to it
            foreach (FileSystemInfo foundDir in filesAndDirs)
                if (foundDir.GetType() == typeof(DirectoryInfo))
                    pathToAttach = foundDir.FullName;

            // if folder exists, add the link to it
            if (Directory.Exists(pathToAttach) || confirm)
            {
                // block with link to folder with attacments
                streamWriter.WriteLine(@"<div style=""border: 1px solid black; background-color:lightgray;"">ATTACHMENTS:</div>");
                streamWriter.WriteLine(@"<p><a href=""{0}"">{0}</a></p>", pathToAttach);
            }

            streamWriter.WriteLine("{0}", "</body>");
            streamWriter.WriteLine("{0}", "</html>");

            streamWriter.Close();
            fileStream.Close();

            // current state of program progress
            Console.Clear();
            Console.Write("Saving the HTML file...");
            Thread.Sleep(700);

            // download the attachments from tfs item
            if (confirm)
            {
                // current state of program progress
                Console.Clear();
                Console.Write("Download Attachments...");

                // if folder is not exists, create it
                if (!Directory.Exists(pathToAttach))
                    Directory.CreateDirectory(pathToAttach);

                // Get a WebClient object to do the attachment download
                WebClient webClient = new WebClient()
                {
                    UseDefaultCredentials = true
                };

                // catch the error with download the attacments
                try
                {
                    // Loop through each attachment in the work item.
                    foreach (Attachment attachment in workItem.Attachments)
                    {
                        // Construct a filename for the attachment
                        string filename = string.Format("{0}\\{1}", pathToAttach, attachment.Name);
                        // Download the attachment.
                        webClient.DownloadFile(attachment.Uri, filename);
                    }
                }
                catch (Exception ex)
                {
                    exExit(ex);
                }
            }

            // current state of program progress
            Console.Clear();
            Console.Write("Opening the HTML file...");
            Thread.Sleep(700);

            // open the created html file, will be open by default app for html files
            System.Diagnostics.Process.Start(pathToHtml);

            // current state of program progress
            Console.Clear();
            Console.Write("Finish...");
            Thread.Sleep(700);
        }

        // for write the catched exception and exit
        public static void exExit(Exception ex)
        {
            Console.Clear();
            Console.WriteLine(ex.Message);
            Console.Write("\nPlease 'Enter' for exit...");
            Console.Read();
            System.Environment.Exit(1);
        }
    }
}