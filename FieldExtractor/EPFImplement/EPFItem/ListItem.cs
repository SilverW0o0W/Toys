using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class ListItem : BaseItem
    {
        public ListItem(XmlDocument document, string logTime) : base(ItemType.ListItem, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = null, fileName = null;
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
                    case "<SourceFullPath>k__BackingField":
                        path = node.InnerText;
                        condition[0]++;
                        break;
                    case "<DisplayName>k__BackingField":
                        fileName = node.InnerText;
                        condition[1]++;
                        break;
                    default:
                        break;
                }
            }
            if (path == null || fileName == null)
            {
                log.Debug("The required info is incomplete. Path: {0},FileName: {2}.", path, fileName);
                return string.Empty;
            }
            path = CutItemSelfName(path);
            fileName = string.Format("{0}", fileName);
            relativeName = ConcatRelativeName(fileName, path);
            return relativeName;
        }

        private string CutItemSelfName(string path)
        {
            int signal = path.LastIndexOf('/');
            return path.Substring(0, signal);
        }
    }
}
