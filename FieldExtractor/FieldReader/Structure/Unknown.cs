using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FieldManager
{
    public class Unknown : BaseItem
    {
        public Unknown(string logTime) : base(ItemType.Unknown, null, logTime)
        {
            this.ExportResult = Result.Fail;
        }

        public Unknown(XmlDocument document, string logTime) : base(ItemType.Unknown, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = "/UnKnown";
            string fileName = "unknown data";
            relativeName = ConcatRelativeName(fileName, path);
            return relativeName;
        }
    }
}
