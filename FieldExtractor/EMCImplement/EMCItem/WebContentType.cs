using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class WebContentType : BaseItem
    {
        public WebContentType(XmlDocument document, string logTime) : base(ItemType.WebContentType, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = "/WebContentType";
            string fileName = "web content type";
            relativeName = ConcatRelativeName(fileName, path);
            return relativeName;
        }
    }
}
