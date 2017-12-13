using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using LoggerManager;

namespace FieldManager
{
    internal class Configuration
    {
        private static Logger log = FieldWorker.WorkerLog;

        public string ConfigFullName { get; private set; }
        public string ImplementorFullName { get; set; }
        public string FactoryClass { get; set; }
        public Implementor CurrentImplementor { get; set; }
        public bool ReadMultiLine { get; private set; }
        public bool UseAutoMapping { get; private set; }
        public List<Implementor> Implementors { get; private set; }
        public Dictionary<string, Implementor> ImplementorMapping { get; private set; }

        private static string ModulePath = "module";

        public Configuration()
        {
        }

        public bool Load(string path)
        {
            bool result = false;
            this.ConfigFullName = string.Format("{0}{1}{2}", path, Path.DirectorySeparatorChar, "handler.config");
            if (!File.Exists(this.ConfigFullName))
            {
                log.Error("The config file is not found. File name: {0}.", this.ConfigFullName);
                return false;
            }
            XmlDocument document = new XmlDocument();
            document.Load(this.ConfigFullName);
            XmlElement rootNode = document.FirstChild as XmlElement;
            LoadMultiline(rootNode);
            if (!LoadImplementor(rootNode))
            {
                return false;
            }
            LoadAutoMapping(rootNode);
            result = this.UseAutoMapping ? true : SetNoMappingImplementor();
            return result;
        }

        private bool LoadImplementor(XmlElement rootNode)
        {
            XmlNodeList nodes = rootNode.GetElementsByTagName("Implementors");
            XmlNode implementorsNode;
            if (nodes.Count > 0)
            {
                implementorsNode = nodes[0];
            }
            else
            {
                log.Error("Implementors node not found.");
                return false;
            }
            this.Implementors = new List<Implementor>();
            foreach (XmlElement impNode in implementorsNode.ChildNodes)
            {
                Implementor implementor = new Implementor();
                bool available = false;
                string name = impNode.GetAttribute("Name");
                string fileName = impNode.GetAttribute("FileName");
                string className = impNode.GetAttribute("Class");
                string value = impNode.GetAttribute("Available");
                log.Info("Dll file: {0}. Class: {1}. Name: {2}", fileName, className, name);
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(name))
                {
                    log.Error("The dll config infomation is not found.");
                    continue;
                }
                string fullName = FieldWorker.CurrentPath + Path.DirectorySeparatorChar + Configuration.ModulePath + Path.DirectorySeparatorChar + fileName;
                implementor = new Implementor(name, fileName, className, fullName, available);
                this.Implementors.Add(implementor);
                log.Debug("Implementor {0} is matched.", implementor.FileName);
            }
            return true;
        }

        public bool SetNoMappingImplementor()
        {
            foreach (Implementor implementor in this.Implementors)
            {
                if (implementor.Available)
                {
                    this.CurrentImplementor = implementor;
                    return true;
                }
            }
            return false;
        }

        //private bool LoadImplementor(XmlElement rootNode)
        //{
        //    bool result = false;
        //    XmlNodeList nodes = rootNode.GetElementsByTagName("Implementors");
        //    XmlNode implementorsNode;
        //    if (nodes.Count > 0)
        //    {
        //        implementorsNode = nodes[0];
        //    }
        //    else
        //    {
        //        log.Error("Implementors node not found.");
        //        return false;
        //    }
        //    foreach (XmlElement impNode in implementorsNode.ChildNodes)
        //    {

        //        if (impNode.HasAttribute("Available"))
        //        {
        //            bool available = false;
        //            string value = impNode.GetAttribute("Available");
        //            if (bool.TryParse(value, out available) && available)
        //            {
        //                try
        //                {
        //                    string fileName = impNode.GetAttribute("FileName");
        //                    string className = impNode.GetAttribute("Class");
        //                    log.Info("Dll file: {0}. Class: {1}.", fileName, className);
        //                    if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(className))
        //                    {
        //                        log.Error("The dll file name or class name is not found.");
        //                        return false;
        //                    }
        //                    this.ImplementorFullName = FieldWorker.CurrentPath + Path.DirectorySeparatorChar + Configuration.ModulePath + Path.DirectorySeparatorChar + fileName;
        //                    this.FactoryClass = className;
        //                    result = true;
        //                }
        //                catch (Exception ex)
        //                {
        //                    log.Error("Get attribute value failed. Reason: {0}.", ex.ToString());
        //                    result = false;
        //                }
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        private void LoadMultiline(XmlElement rootNode)
        {
            try
            {
                XmlNodeList nodes = rootNode.GetElementsByTagName("MultiLineLog");
                XmlNode multilineNode;
                multilineNode = nodes[0];
                string flagValue = multilineNode.Attributes["Flag"].Value;
                this.ReadMultiLine = bool.Parse(flagValue);
            }
            catch (Exception ex)
            {
                log.Warning("Load MultiLineLog node failed. Use default setting: false. Reason: {0}.", ex.ToString());
                this.ReadMultiLine = false;
            }
        }


        private void LoadAutoMapping(XmlElement rootNode)
        {

            try
            {
                XmlNodeList nodes = rootNode.GetElementsByTagName("AutoMapping");
                XmlNode autoMappingNode;
                autoMappingNode = nodes[0];
                string value = autoMappingNode.Attributes["Available"].Value;
                bool available;
                bool.TryParse(value, out available);
                if (!available)
                {
                    this.UseAutoMapping = false;
                    return;
                }
                this.ImplementorMapping = new Dictionary<string, Implementor>();
                foreach (XmlElement node in autoMappingNode.ChildNodes)
                {
                    string file = node.GetAttribute("File");
                    string implementorName = node.GetAttribute("Implementor");
                    if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(implementorName))
                    {
                        log.Warning("Auto mapping config information is not found.");
                        continue;
                    }
                    Implementor implementor = this.Implementors.Find(implementorTemp => implementorTemp.Name == implementorName);
                    this.ImplementorMapping.Add(file, implementor);
                }
                this.UseAutoMapping = true;
            }
            catch (Exception ex)
            {
                log.Warning("Load MultiLineLog node failed. Use default setting: false. Reason: {0}.", ex.ToString());
                this.UseAutoMapping = false;
            }
        }
    }

    public class Implementor
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string ClassName { get; set; }
        public string FullName { get; set; }
        public bool Available { get; set; }

        public Implementor()
        {

        }

        public Implementor(string name, string fileName, string className, string fullName, bool available)
        {
            this.Name = name;
            this.FileName = fileName;
            this.ClassName = className;
            this.ClassName = className;
            this.FullName = fullName;
            this.Available = available;
        }
    }
}
