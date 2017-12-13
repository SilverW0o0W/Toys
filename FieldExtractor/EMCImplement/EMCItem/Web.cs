using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class Web : BaseItem
    {
        public Web(XmlDocument document, string logTime) : base(ItemType.Web, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = null, webName = null;
            int condition = 0;
            foreach (XmlNode node in document.FirstChild.ChildNodes)
            {
                if (condition > 0)
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
                    case "SourcePath":
                        path = node.InnerText;
                        condition++;
                        break;
                    default:
                        break;
                }
            }
            webName = GetNameFromPath(path);
            if (path == null || webName == null)
            {
                log.Debug("The required info is incomplete. Path: {0},WebName: {1}.", path, webName);
                return string.Empty;
            }

            webName = string.Format("web_{0}", webName);
            relativeName = ConcatRelativeName(webName, path);
            return relativeName;
        }
    }
}
