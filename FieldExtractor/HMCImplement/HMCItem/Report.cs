using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class Report : BaseItem
    {
        public Report(XmlDocument document, string logTime) : base(ItemType.Report, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = "/HSMJob";
            string fileName = "Report";
            relativeName = ConcatRelativeName(fileName, path);
            return relativeName;
        }
    }
}
