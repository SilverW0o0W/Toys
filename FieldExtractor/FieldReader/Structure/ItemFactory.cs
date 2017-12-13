using LoggerManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FieldManager
{
    public abstract class ItemFactory : IFieldHandler
    {
        protected Logger log;

        public ItemFactory(Logger log)
        {
            this.log = log;
            BaseItem.SetLog(log);
        }

        public abstract bool CheckDataContent(string lineString);
        public abstract BaseItem CreateItem(string content);

        public virtual bool CheckLogLineHeader(string tempLogLine)
        {
            string pattern = @"^\b(DEBUG|INFO|WARN|ERROR)\b";
            return Regex.IsMatch(tempLogLine, pattern);
        }

        //V:6.9.0.4002
        public virtual bool CheckLogLineEnd(string tempLogLine)
        {
            string pattern = @"(</Field>)+. *V:(\d+\.){3}\d+";
            return Regex.IsMatch(tempLogLine, pattern);
        }

        protected virtual string ExtractXML(string content)
        {
            string xml = string.Empty;
            string head = "<Field";
            string foot = "</Field>";
            int start = content.IndexOf(head);
            int end = content.LastIndexOf(foot);
            xml = content.Substring(start, end - start + 1 + head.Length + 1);
            return xml;
        }

        protected virtual string AnalysisLogTime(string content)
        {
            //DEBUG 04-27 13:42:21,115 1 RuntimeConstructorInfo 0 - Data content:
            string logTime = string.Empty;
            string pattern = @"\d{2}-\d{2} (\d{2}:){2}\d{2},\d{3}";
            Regex regex = new Regex(pattern);
            if (regex.IsMatch(content))
            {
                Match match = regex.Match(content);
                logTime = match.Value;
            }
            return logTime;
        }
    }
}
