﻿using LinqToTwitter.Common;
using LitJson;
using System.Xml.Serialization;

namespace LinqToTwitter
{
    [XmlType(Namespace = "LinqToTwitter")]
    public class Contributor
    {
        public Contributor() { }
        public Contributor(JsonData contributorJson)
        {
            ID = contributorJson.GetValue<string>("id_str");
            ScreenName = contributorJson.GetValue<string>("screen_name");
        }

        public string ID { get; set; }

        public string ScreenName { get; set; }
    }
}
