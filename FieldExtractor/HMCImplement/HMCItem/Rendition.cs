﻿using FieldManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Implementor.Item
{
    public class Rendition : BaseItem
    {
        public Rendition(XmlDocument document, string logTime) : base(ItemType.Rendition, document, logTime)
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
                log.Debug("The required info is incomplete. Path: {0},Version: {1},FileName: {2}.", path, fileName);
                return string.Empty;
            }

            relativeName = ConcatRelativeName(fileName, path);
            return relativeName;
        }
    }
}
