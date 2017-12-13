using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class ActiveFeature : BaseItem
    {
        public ActiveFeature(XmlDocument document, string logTime) : base(ItemType.ActiveFeature, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = "/HSMJob";
            string fileName = "ActiveFeature";
            relativeName = ConcatRelativeName(fileName, path);
            return relativeName;
        }
    }
}
