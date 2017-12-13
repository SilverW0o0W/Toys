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
    public class HMCFactory : ItemFactory
    {
        public HMCFactory(Logger log) : base(log)
        {
        }

        public override bool CheckDataContent(string lineString)
        {
            bool result = false;
            if (lineString.StartsWith("DEBUG"))
            {
                string pattern = @"Data content: (<Field[^>])([\s\S]*?)(</Field>)";
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
                case "DocumentTagging":
                case "DocumentCollection":
                    type = ItemType.DocumentCollection;
                    break;
                case "ListItemInfo":
                    type = ItemType.LinkItem;
                    break;
                case "DocProperty":
                    type = GetDocPropertyType(rootNode);
                    break;
                case "ListBasicInfo":
                    type = ItemType.List;
                    break;
                case "WebBasicInfo":
                    type = ItemType.Web;
                    break;
                case "WebContentType":
                    type = ItemType.WebContentType;
                    break;
                case "Report":
                    type = ItemType.Report;
                    break;
                case "ImportAction":
                    type = ItemType.ImportAction;
                    break;
                case "ActiveFeature":
                    type = ItemType.ActiveFeature;
                    break;
                case "SocialComment":
                    type = ItemType.Others;
                    break;
                default:
                    type = ItemType.Unknown;
                    break;
            }
            return type;
        }

        private ItemType GetDocPropertyType(XmlNode rootNode)
        {
            ItemType type = ItemType.Unknown;

            try
            {
                foreach (XmlNode node in rootNode.ChildNodes)
                {
                    XmlAttribute nameAttribute = node.Attributes["name"];
                    string name = nameAttribute.Value;
                    if ("SourceType" == name)
                    {
                        if ("Document Set" == node.InnerText)
                        {
                            type = ItemType.DocumentSet;
                        }
                        else
                        {
                            type = ItemType.Folder;
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warning("A exception has occurred in DocProperty.Reason: {0}", ex.ToString());
                type = ItemType.Unknown;
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
                case ItemType.DocumentCollection:
                    item = new DocumentCollection(document, logTime);
                    break;
                case ItemType.Rendition:
                    item = new Rendition(document, logTime);
                    break;
                case ItemType.LinkItem:
                    item = new LinkItem(document, logTime);
                    break;
                case ItemType.Folder:
                    item = new Folder(document, logTime);
                    break;
                case ItemType.DocumentSet:
                    item = new DocumentSet(document, logTime);
                    break;
                case ItemType.List:
                    item = new List(document, logTime);
                    break;
                case ItemType.Web:
                    item = new Web(document, logTime);
                    break;
                case ItemType.WebContentType:
                    item = new WebContentType(document, logTime);
                    break;
                case ItemType.Report:
                    item = new Report(document, logTime);
                    break;
                case ItemType.ImportAction:
                    item = new ImportAction(document, logTime);
                    break;
                case ItemType.ActiveFeature:
                    item = new ActiveFeature(document, logTime);
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
