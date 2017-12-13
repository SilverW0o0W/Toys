using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FieldManager
{
    public class OverLengthItem : BaseItem
    {
        public OverLengthItem(FieldManager.ItemType type, XmlDocument document, string fileName, string logTime) : base(type, document, fileName, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = "/OverLength";
            relativeName = ConcatRelativeName(this.FileName, path);
            return relativeName;
        }
    }
}
