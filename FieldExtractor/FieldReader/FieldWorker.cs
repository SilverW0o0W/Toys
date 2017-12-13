using LoggerManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FieldManager
{
    public class FieldWorker
    {
        public const long MB = 1024 * 1024;


        private static Logger workerLog;
        public static Logger WorkerLog { get { return workerLog; } }
        internal static string CurrentPath { get; private set; }

        private Logger jobLog;
        private Configuration config;
        private ItemFactory factory;

        private string cacheLogLine = string.Empty;

        private string currentJobPath;
        private XmlWriter recordWriter;

        public string JobId { get; private set; }

        static FieldWorker()
        {
            FileInfo info = new FileInfo(Assembly.GetExecutingAssembly().Location);
            CurrentPath = info.DirectoryName;
        }

        public FieldWorker()
        {
        }

        public bool Initialize()
        {
            bool result = false;
            workerLog = new Logger("FieldWorker.log", CurrentPath);
            config = new Configuration();
            workerLog.Info("Initializing worker.");
            result = config.Load(CurrentPath);
            if (!result)
            {
                workerLog.Error("Load config file failed. File name: {0}.", config.ConfigFullName);
                return false;
            }

            return result;
        }

        public int[] Run(string[] filePaths)
        {
            this.JobId = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            workerLog.Info("A job start running. Job id: {0}.", this.JobId);
            currentJobPath = CurrentPath + Path.DirectorySeparatorChar + JobId;
            jobLog = new Logger("ExportJob.log", currentJobPath);

            if (!config.UseAutoMapping)
            {
                try
                {
                    workerLog.Debug("Load implement dll. DLL file name: {0}.", config.ImplementorFullName);
                    factory = LoadImplementDll();
                }
                catch (Exception ex)
                {
                    workerLog.Error("An exception occurred in load implement dll. DLL file name: {0}. Reason: {1}.", config.ImplementorFullName, ex.ToString());
                    throw;
                }
            }

            List<FileInfo> fileInfos;
            int[] totalReport;
            if (filePaths == null)
            {
                jobLog.Error("File path is null.");
                throw new HandlerException("File path is null.");
            }
            fileInfos = CheckFilePath(filePaths);
            jobLog.Info("The job is start.Job id: {0}.", JobId);
            totalReport = ExportFiles(fileInfos);
            jobLog.Info("The job is finished. Job id: {0}.", JobId);
            workerLog.Info("The job is finished. Job id: {0}.", JobId);
            return totalReport;
        }

        private List<FileInfo> CheckFilePath(string[] filePaths)
        {
            List<FileInfo> fileInfos = new List<FileInfo>();
            try
            {
                foreach (string filePath in filePaths)
                {
                    string file = filePath ?? string.Empty;
                    file = file.Trim('\"');
                    if (File.Exists(file))
                    {
                        FileInfo info = new FileInfo(file);
                        fileInfos.Add(info);
                    }
                    else
                    {
                        jobLog.Warning("The file is not existed.File Name: {0}.", file);
                    }
                }
            }
            catch (Exception ex)
            {
                jobLog.Warning("Get file info failed.Reason: {0}", ex.ToString());
                fileInfos = new List<FileInfo>();
            }
            jobLog.Debug("File checking is over. Available file count: {0}.", fileInfos.Count);
            return fileInfos;
        }

        private int[] ExportFiles(List<FileInfo> fileInfos)
        {
            int[] totalReport = { 0, 0, 0, 0 };

            using (recordWriter = XmlWriter.Create(string.Format("{0}{1}{2}", currentJobPath, Path.DirectorySeparatorChar, "Record.xml")))
            {
                int totalCount = 0;
                recordWriter.WriteStartDocument();
                recordWriter.WriteStartElement("Items");
                foreach (FileInfo fileInfo in fileInfos)
                {
                    jobLog.Info("Log File Name: {0}.", fileInfo.FullName);
                    Console.WriteLine("Current file name:");
                    Console.WriteLine(fileInfo.FullName);
                    if (config.UseAutoMapping)
                    {
                        foreach (string fileName in config.ImplementorMapping.Keys)
                        {
                            if (fileInfo.Name.StartsWith(fileName))
                            {
                                Implementor implementor = config.ImplementorMapping[fileName];
                                if (string.IsNullOrEmpty(config.ImplementorFullName) || implementor.FullName != config.ImplementorFullName)
                                {
                                    config.CurrentImplementor = implementor;
                                    try
                                    {
                                        workerLog.Debug("Reload implement dll. DLL file name: {0}.", config.ImplementorFullName);
                                        factory = LoadImplementDll();
                                    }
                                    catch (Exception ex)
                                    {
                                        workerLog.Error("An exception occurred in load implement dll. DLL file name: {0}. Reason: {1}.", config.ImplementorFullName, ex.ToString());
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                    int[] fileReport = { 0, 0, 0 };
                    int fileCount = 0;
                    using (StreamReader reader = new StreamReader(fileInfo.FullName, Encoding.UTF8))
                    {
                        while (true)
                        {
                            string logLine;
                            logLine = ReadLine(reader);
                            if (logLine == null)
                            {
                                break;
                            }
                            else if (string.IsNullOrWhiteSpace(logLine))
                            {
                                continue;
                            }
                            if (factory.CheckDataContent(logLine))
                            {
                                totalCount++;
                                fileCount++;
                                try
                                {
                                    jobLog.Debug("Start to save xml.");
                                    BaseItem item = factory.CreateItem(logLine);
                                    if (!(Result.None == item.ExportResult))
                                    {
                                        switch (item.ExportResult)
                                        {
                                            case Result.Fail:
                                                fileReport[1]++;
                                                break;
                                            case Result.Skip:
                                                fileReport[2]++;
                                                break;
                                            default:
                                                break;
                                        }
                                        RecordXml(item, recordWriter);
                                        continue;
                                    }
                                    item.Export(currentJobPath);
                                    RecordResult(item, fileReport, recordWriter);
                                }
                                catch (Exception ex)
                                {
                                    jobLog.Warning("Save xml failed.Reason: {0}", ex.ToString());
                                }
                                jobLog.Debug("End to save xml.");
                            }
                        }
                    }
                    jobLog.Debug("File Report.File Path: {0}.", fileInfo.FullName);
                    jobLog.Debug("File Count: {0}. Success: {1}. Failed: {2}. Skipped: {3}.", fileCount, fileReport[0], fileReport[1], fileReport[2]);
                    totalReport[0] += fileReport[0];
                    totalReport[1] += fileReport[1];
                    totalReport[2] += fileReport[2];
                }
                //recordWriter.WriteAttributeString("Count", totalCount.ToString());
                //recordWriter.WriteAttributeString("Success", totalReport[0].ToString());
                //recordWriter.WriteAttributeString("Fail", totalReport[1].ToString());
                //recordWriter.WriteAttributeString("Skip", totalReport[2].ToString());
                recordWriter.WriteEndElement();
                recordWriter.WriteEndDocument();
                recordWriter.Close();
                jobLog.Info("Total Count: {0}. Success: {1}. Failed: {2}. Skipped: {3}.", totalCount, totalReport[0], totalReport[1], totalReport[2]);
                totalReport[3] = totalCount;
            }
            return totalReport;
        }

        private string ReadLine(StreamReader reader)
        {
            return config.ReadMultiLine ? ReadMultiLine(reader) : reader.ReadLine();
        }

        private string ReadMultiLine(StreamReader reader)
        {
            string logLine = string.Empty;
            if (!string.IsNullOrEmpty(cacheLogLine))
            {
                logLine = cacheLogLine;
                cacheLogLine = string.Empty;
            }
            else
            {
                logLine = reader.ReadLine();
            }
            if (logLine != null)
            {
                if (factory.CheckDataContent(logLine))
                {
                    string tempLogLine = logLine;
                    while (!factory.CheckLogLineEnd(tempLogLine))
                    {
                        string tempLine = reader.ReadLine();
                        if (factory.CheckLogLineHeader(tempLine))
                        {
                            cacheLogLine = tempLine;
                            break;
                        }
                        else
                        {
                            tempLogLine += tempLine;
                        }
                    }
                    logLine = tempLogLine;
                }
            }
            return logLine;
        }

        private void RecordResult(BaseItem item, int[] result, XmlWriter recordWriter)
        {
            switch (item.ExportResult)
            {
                case Result.Fail:
                    result[1]++;
                    break;
                case Result.Skip:
                    result[2]++;
                    break;
                case Result.Success:
                    result[0]++;
                    break;
                default:
                    result[1]++;
                    break;
            }
            RecordXml(item, recordWriter);
        }

        private void RecordXml(BaseItem item, XmlWriter recordWriter)
        {
            recordWriter.WriteStartElement("Item");
            recordWriter.WriteAttributeString("Name", ConvertString(item.FileName));
            recordWriter.WriteAttributeString("Type", ConvertString(item.ItemType.ToString()));
            recordWriter.WriteAttributeString("Result", ConvertString(item.ExportResult.ToString()));
            recordWriter.WriteAttributeString("LogTime", ConvertString(item.LogTime));
            recordWriter.WriteAttributeString("ExportFullName", ConvertString(item.FullName));
            recordWriter.WriteAttributeString("RelativeFullName", ConvertString(item.RelativeFullName));
            recordWriter.WriteEndElement();
        }

        private string ConvertString(object value)
        {
            string result;
            if (value != null)
            {
                result = value.ToString();
                result = result ?? string.Empty;
            }
            else
            {
                result = string.Empty;
            }
            return result;
        }

        private ItemFactory LoadImplementDll()
        {
            Assembly assembly = Assembly.LoadFile(config.CurrentImplementor.FullName);
            Type type = assembly.GetType(config.CurrentImplementor.ClassName);
            object instance = assembly.CreateInstance(config.CurrentImplementor.ClassName, true, BindingFlags.Default, null, new object[] { jobLog }, null, null);
            ItemFactory newFactory = instance as ItemFactory;
            return newFactory;
        }
    }

    public class HandlerException : Exception
    {
        public HandlerException(string message) : base(message)
        {

        }
    }
}
