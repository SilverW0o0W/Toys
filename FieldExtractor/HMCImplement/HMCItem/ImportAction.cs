using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class ImportAction : BaseItem
    {
        public ImportAction(XmlDocument document, string logTime) : base(ItemType.ImportAction, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = "/HSMJob";
            string fileName = "ImportAction";
            relativeName = ConcatRelativeName(fileName, path);
            return relativeName;
        }
    }
}
