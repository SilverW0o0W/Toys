using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class DocumentSet : BaseItem
    {
        public DocumentSet(XmlDocument document, string logTime) : base(ItemType.DocumentSet, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = null, version = null, virtualDocumentName = null;
            bool isFirst = false;
            int[] condition = { 0, 0 };
            foreach (XmlNode node in document.FirstChild.ChildNodes)
            {
                if (condition[0] > 0 && condition[1] > 0)
                {
                    break;
                }
                XmlAttribute nameAttribute = node.Attributes["name"];
                if (nameAttribute == null)
                {
                    continue;
                }
                string name = nameAttribute.Value;
                switch (name)
                {
                    case "ParentFolderPath":
                        path = node.InnerText;
                        condition[0]++;
                        break;
                    case "ItemFields":
                        virtualDocumentName = GetOriginalName(node);
                        version = GetItemFieldName(node, "Version");
                        isFirst = string.Empty == version;
                        condition[1]++;
                        break;
                    default:
                        break;
                }
            }
            if (path == null || version == null || virtualDocumentName == null)
            {
                log.Debug("The required info is incomplete. Path: {0},Version: {1},FileName: {2}.", path, version, virtualDocumentName);
                return string.Empty;
            }

            if (isFirst)
            {
                path = string.Format("{0}/{1}", path, virtualDocumentName);
                virtualDocumentName = string.Format("VirtualDocument_{0}", virtualDocumentName);
            }
            else
            {
                virtualDocumentName = string.Format("VirtualDocumentVersion_{0}", virtualDocumentName);
            }

            relativeName = ConcatRelativeName(virtualDocumentName, path, version);
            return relativeName;
        }
    }
}
