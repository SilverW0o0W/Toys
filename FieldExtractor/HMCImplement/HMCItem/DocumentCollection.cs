using Implementor;
using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class DocumentCollection : BaseItem
    {
        public DocumentCollection(XmlDocument document, string logTime) : base(ItemType.DocumentCollection, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            if (document.FirstChild.ChildNodes.Count > 0)
            {
                return GenerateRelativeNameByFirstChild(document.FirstChild.ChildNodes[0]);
            }
            else
            {
                string path = "/UnKnown";
                string fileName = "empty document collection";
                return ConcatRelativeName(fileName, path);
            }
        }

        private string GenerateRelativeNameByFirstChild(XmlNode documentNode)
        {
            string relativeName = string.Empty;
            string path = null, fileName = null;
            int[] condition = { 0, 0 };

            foreach (XmlNode node in documentNode.ChildNodes)
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
                        fileName = GetOriginalName(node);
                        condition[1]++;
                        break;
                    default:
                        break;
                }
            }
            if (path == null || fileName == null)
            {
                log.Debug("The required info is incomplete. Path: {0},FileName: {1}.", path, fileName);
                return string.Empty;
            }

            relativeName = ConcatRelativeName(fileName, path);
            return relativeName;
        }
    }
}
