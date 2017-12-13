using FieldManager;
using Implementor.Item;
using LoggerManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Implementor
{
    public class EPFFactory : ItemFactory
    {
        public EPFFactory(Logger log) : base(log)
        {
        }

        public override bool CheckDataContent(string lineString)
        {
            bool result = false;
            if (lineString.StartsWith("DEBUG"))
            {
                string pattern = @"Metadata \w* Information: (<Field[^>])([\s\S]*?)(</Field>)";
                result = Regex.IsMatch(lineString, pattern);
            }
            return result;
        }

        public override BaseItem CreateItem(string content)
        {
            string logTime = string.Empty;
            BaseItem item;
            try
            {
                logTime = AnalysisLogTime(content);
                string xml = ExtractXML(content);
                XmlDocument document = new XmlDocument();
                XmlNode rootNode;
                document.LoadXml(xml);
                rootNode = document.FirstChild;
                ItemType type = AnalysisItemType(rootNode);
                item = CreateItem(type, document, logTime);
            }
            catch (Exception ex)
            {
                log.Error("Create item failed.Reason: {0}.", ex.ToString());
                item = new Unknown(logTime);
            }
            return item;
        }

        protected ItemType AnalysisItemType(XmlNode rootNode)
        {

            ItemType type = ItemType.Unknown;
            XmlAttribute nameAttribute = rootNode.Attributes["name"];
            if (nameAttribute == null)
            {
                return ItemType.Unknown;
            }
            string name = nameAttribute.Value;
            switch (name)
            {
                case "AttachmentData":
                    type = ItemType.Unknown;
                    break;
                case "ListItemInfo":
                    type = ItemType.ListItem;
                    break;
                case "DocProperty":
                    type = ItemType.Folder;
                    break;
                case "ListBasicInfo":
                    type = ItemType.List;
                    break;
                case "WebBasicInfo":
                    type = ItemType.Web;
                    break;
                case "SiteBasicInfo":
                    type = ItemType.Others;
                    break;
                case "DocData":
                    type = ItemType.Others;
                    break;
                case "ItemTableInfo":
                    type = ItemType.Others;
                    break;
                case "Report":
                    type = ItemType.Others;
                    break;
                default:
                    type = ItemType.Unknown;
                    break;
            }
            return type;
        }

        protected BaseItem CreateItem(ItemType type, XmlDocument document, string logTime)
        {
            BaseItem item = null;
            switch (type)
            {
                case ItemType.Unknown:
                    item = new Unknown(document, logTime);
                    break;
                case ItemType.ListItem:
                    item = new ListItem(document, logTime);
                    break;
                case ItemType.Folder:
                    item = new Folder(document, logTime);
                    break;
                case ItemType.List:
                    item = new List(document, logTime);
                    break;
                case ItemType.Web:
                    item = new Web(document, logTime);
                    break;
                case ItemType.Others:
                    item = new Others(document, logTime);
                    break;
                default:
                    item = null;
                    break;
            }
            return item;
        }
    }
}
