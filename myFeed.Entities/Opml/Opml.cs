﻿using System.Collections.Generic;
using System.Xml.Serialization;

namespace myFeed.Entities.Opml
{
    [XmlType("opml")]
    public class Opml
    {
        [XmlElement("head")]
        public Head Head { get; set; }

        [XmlArray("body")]
        public List<Outline> Body { get; set; }
    }
}