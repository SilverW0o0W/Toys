using LoggerManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FieldManager
{
    public enum ItemType
    {
        Unknown,
        Document,
        DocumentCollection,
        Rendition,
        Attachment,
        LinkItem,
        ListItem,
        Folder,
        DocumentSet,
        List,
        Web,
        WebContentType,
        Site,
        Others,
        Report,
        ImportAction,
        ActiveFeature,
        Custom1,
        Custom2,
        Custom3
    }

    public enum Result
    {
        None,
        Fail,
        Skip,
        Success
    }

    public abstract class BaseItem : IBaseItem
    {
        protected static Logger log;

        protected XmlDocument document;
        private string jobCurrentPath;
        protected string version = string.Empty;

        public ItemType ItemType { get; protected set; }
        public string FileName { get; protected set; }
        public string RelativePath { get; protected set; }
        public string RelativeFullName { get; protected set; }
        public string FullName { get; protected set; }
        public string LogTime { get; protected set; }
        public Result ExportResult { get; protected set; }

        public BaseItem(ItemType type, XmlDocument document, string logTime)
        {
            this.ItemType = type;
            this.document = document;
            this.LogTime = logTime;
            this.ExportResult = Result.None;
            if (document != null)
            {
                GenerateRelativeName();
            }
        }


        //for over length item
        public BaseItem(ItemType type, XmlDocument document, string fileName, string logTime)
        {
            this.ItemType = type;
            this.document = document;
            this.FileName = fileName;
            this.LogTime = logTime;
            this.ExportResult = Result.None;
            if (document != null)
            {
                GenerateRelativeName();
            }
        }

        protected abstract string GenerateRelativeName();

        protected string CreateFolderStructure(string currentPath, string path)
        {
            string realPath = string.Empty;
            try
            {
                realPath = string.Format("{0}{1}{2}", currentPath, Path.DirectorySeparatorChar, path);
                if (!Directory.Exists(realPath))
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(realPath);
                    realPath = directoryInfo.FullName;
                }
            }
            catch (Exception ex)
            {
                log.Error("Create folder structure failed.Reason: {0}", ex.ToString());
                realPath = string.Empty;
            }
            return realPath;
        }

        protected string ConcatRelativeName(string fileName, string relativePath, string version = null)
        {
            string relativeFullName = string.Empty;
            if (!string.IsNullOrEmpty(version))
            {
                fileName = string.Format("{0}_{1}", fileName, version);
            }
            relativeFullName = string.Format("{0}{1}{2}", relativePath, Path.DirectorySeparatorChar, fileName);
            this.RelativePath = relativePath;
            this.FileName = fileName;
            this.RelativeFullName = relativeFullName;
            return relativeFullName;
        }

        protected Result CreateXMLFile(XmlDocument document, string fileName, string realPath, string version = null)
        {
            Result result = Result.None;
            string fullName = string.Empty;
            try
            {
                string fileRealName = fileName;
                if (!string.IsNullOrEmpty(version))
                {
                    fileRealName = string.Format("{0}_{1}", fileName, version);
                }
                fullName = ConfirmFileName(realPath, fileRealName);
                document.Save(fullName);
                FileInfo info = new FileInfo(fullName);
                fullName = info.FullName;
                result = Result.Success;
            }
            catch (PathTooLongException)
            {
                if (this is OverLengthItem)
                {
                    log.Warning("File path is still over length. Export file failed. File name: {0}.", fullName);
                    throw;
                }
                log.Warning("File path is over length.Reset path. File name: {0}.", fullName);
                OverLengthItem item = new OverLengthItem(this.ItemType, document, this.FileName, LogTime);
                item.Export(jobCurrentPath);
                this.FullName = item.FullName;
                this.ExportResult = item.ExportResult;
                result = item.ExportResult;
            }
            catch (Exception ex)
            {
                log.Warning("Create xml file failed. Full name: {0}. Reason: {1}.", fullName, ex.ToString());
                result = Result.Fail;
            }
            this.FullName = fullName;
            this.ExportResult = result;
            return result;
        }

        private string ConfirmFileName(string path, string nameNoExtension)
        {
            string fullName;
            string extension = ".xml";
            nameNoExtension = ReplaceIllegalCharacter(nameNoExtension);
            nameNoExtension = path + Path.DirectorySeparatorChar + nameNoExtension;
            fullName = string.Format("{0}{1}", nameNoExtension, extension);
            int index = 1;
            while (File.Exists(fullName))
            {
                fullName = string.Format("{0}_{1}{2}", nameNoExtension, index, extension);
                index++;
            }
            return fullName;
        }

        private string ReplaceIllegalCharacter(string name)
        {
            string pattern = @"\?|\*|\<|\>|\/|\:|" + '\\' + '"';
            pattern = string.Format("({0})", pattern);
            if (Regex.IsMatch(name, pattern))
            {
                string newName = string.Empty;
                foreach (char c in name)
                {
                    newName += Regex.IsMatch(c.ToString(), pattern) ? '_' : c;
                }
                name = newName;
            }
            return name;
        }

        public Result Export(string currentPath)
        {
            log.Debug("Start to export file. File name: {0}.", this.FileName);
            Result result;
            if (ItemType.Others == this.ItemType)
            {
                log.Debug("The file is skipped.");
                return Result.Skip;
            }
            if (Result.Fail == this.ExportResult)
            {
                log.Debug("The file is failed.");
                return Result.Fail;
            }
            string realPath;
            this.jobCurrentPath = currentPath;
            realPath = CreateFolderStructure(currentPath, this.RelativePath);
            if (!string.IsNullOrEmpty(realPath))
            {
                result = CreateXMLFile(document, this.FileName, realPath, version);
            }
            else
            {
                log.Error("Folder path is wrong.File name: {0}.", this.FileName);
                result = Result.Fail;
            }
            log.Debug("End to export file. File name: {0}.", this.FileName);
            return result;
        }

        protected string GetOriginalName(XmlNode parentNode)
        {
            string originName = null;
            foreach (XmlNode node in parentNode.ChildNodes)
            {
                XmlAttribute nameAttribute = node.Attributes["name"];
                if (nameAttribute == null)
                {
                    continue;
                }
                string name = nameAttribute.InnerText;
                if ("OriginalName".Equals(name))
                {
                    originName = node.InnerText;
                    break;
                }
            }
            return originName;
        }

        protected string GetItemFieldName(XmlNode parentNode, string nodeName)
        {
            string value = null;
            foreach (XmlNode node in parentNode.ChildNodes)
            {
                XmlAttribute nameAttribute = node.Attributes["name"];
                if (nameAttribute == null)
                {
                    continue;
                }
                string name = nameAttribute.InnerText;
                if (nodeName.Equals(name))
                {
                    value = node.InnerText;
                    break;
                }
            }
            return value;
        }

        protected string GetNameFromPath(string path)
        {
            string name;
            int index = path.LastIndexOf("/");
            name = path.Substring(index + 1);
            return name;
        }

        public static void SetLog(Logger log)
        {
            BaseItem.log = log;
        }
    }
}
