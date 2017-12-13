using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FieldManager
{
    public class Others : BaseItem
    {
        public Others(XmlDocument document, string logTime) : base(ItemType.Others, document, logTime)
        {
            this.ExportResult = Result.Skip;
        }

        protected override string GenerateRelativeName()
        {
            return string.Empty;
        }
    }
}
