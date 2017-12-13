using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class List : BaseItem
    {
        public List(XmlDocument document, string logTime) : base(ItemType.List, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = null, listName = null;
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
                    case "<ListInfo>k__BackingField":
                        listName = GetItemFieldName(node, "Title");
                        condition[1]++;
                        break;
                    default:
                        break;
                }
            }
            if (path == null || listName == null)
            {
                log.Debug("The required info is incomplete. Path: {0},ListName: {1}.", path, listName);
                return string.Empty;
            }

            listName = string.Format("list_{0}", listName);
            relativeName = ConcatRelativeName(listName, path);
            return relativeName;
        }
    }
}
