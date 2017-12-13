using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class Folder : BaseItem
    {
        public Folder(XmlDocument document, string logTime) : base(ItemType.Folder, document, logTime)
        {
        }

        protected override string GenerateRelativeName()
        {
            string relativeName = string.Empty;
            string path = null, folderName = null;
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
                    case "<SourceFullPath>k__BackingField":
                        path = node.InnerText;
                        condition++;
                        break;
                    default:
                        break;
                }
            }
            folderName = GetNameFromPath(path);
            if (path == null || folderName == null)
            {
                log.Debug("The required info is incomplete. Path: {0},FolderName: {1}.", path, folderName);
                return string.Empty;
            }

            folderName = string.Format("folder_{0}", folderName);
            relativeName = ConcatRelativeName(folderName, path);
            return relativeName;
        }
    }
}
