using System;
using System.IO;
using System.Xml;
using System.Globalization;

namespace TimeEye
{
    public class Configuration
    {
        private string configFile = "TimeEye.config";
        public string ConfigFile
        {
            get { return configFile; }
            set { configFile = value; }
        }

        public string Pattern { get; set; }
        public string Format { get; set; }

        private TimeSpan period = new TimeSpan(0, 5, 0);
        public TimeSpan Period
        {
            get { return period; }
            set { period = value; }
        }

        private string outputFile = string.Format("Period-{0}.log", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        public string OutputFile
        {
            get { return outputFile; }
            set { outputFile = value; }
        }

        public AdvanceConfiguration Advance { get; set; }

        public Configuration()
        {
            this.Advance = new AdvanceConfiguration();
        }

        public bool LoadConfigFile(string filePath = null)
        {
            filePath = filePath ?? this.configFile;
            if (!File.Exists(filePath))
            {
                return false;
            }
            this.configFile = filePath;
            XmlDocument document = new XmlDocument();
            document.Load(this.configFile);
            foreach (XmlElement node in document.DocumentElement.ChildNodes)
            {
                switch (node.Name.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "BASE":
                        this.Pattern = GetAttributeValue(node, "pattern", string.Empty);
                        this.Format = GetAttributeValue(node, "format", string.Empty);
                        break;
                    case "PERIOD":
                        this.Period = GetPeriod(node);
                        break;
                    case "ADVANCE":
                        this.Advance.GetAdvanceConfig(node);
                        break;
                    default:
                        break;
                }
            }
            return true;
        }

        private string GetAttributeValue(XmlElement element, string attribute, string defaultValue)
        {
            return element.GetAttribute(attribute) ?? defaultValue;
        }

        private TimeSpan GetPeriod(XmlElement element)
        {
            int[] time = new int[5];
            time[0] = GetAttributeValue(element, "day", -1);
            time[1] = GetAttributeValue(element, "hour", -1);
            time[2] = GetAttributeValue(element, "minute", -1);
            time[3] = GetAttributeValue(element, "second", -1);
            time[4] = GetAttributeValue(element, "millisecond", -1);

            bool isInvalid = false;
            foreach (int t in time)
            {
                if (t < 0)
                {
                    isInvalid = true;
                    break;
                }
            }

            if (isInvalid)
            {
                return new TimeSpan(0, 5, 0);
            }
            else
            {
                return new TimeSpan(time[0], time[1], time[2], time[3], time[4]);
            }
        }

        private int GetAttributeValue(XmlElement element, string attribute, int defaultValue)
        {
            string strValue = element.GetAttribute(attribute);
            if (string.IsNullOrEmpty(strValue))
            {
                return 0;
            }
            else
            {
                int value;
                if (!int.TryParse(strValue, out value))
                {
                    return defaultValue;
                }
                return value;
            }
        }
    }

    public class AdvanceConfiguration
    {
        public bool Depth { get; set; }
        public AdvanceConfiguration()
        {
            this.Depth = false;
        }

        public void GetAdvanceConfig(XmlElement rootElement)
        {
            foreach (XmlElement element in rootElement.ChildNodes)
            {
                switch (element.Name.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "DEPTHSCAN":
                        bool depth;
                        if (bool.TryParse(element.InnerText, out depth))
                        {
                            this.Depth = depth;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
