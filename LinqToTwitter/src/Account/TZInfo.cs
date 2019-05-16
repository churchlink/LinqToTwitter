﻿using LinqToTwitter.Common;

using LitJson;
using System.Xml.Serialization;

namespace LinqToTwitter
{
    /// <summary>
    /// Time zone informtion
    /// </summary>
    [XmlType(Namespace = "LinqToTwitter")]
    public class TZInfo
    {
        public TZInfo() {}
        internal TZInfo(JsonData timeZone)
        {
            Name = timeZone.GetValue<string>("name");
            TzInfoName = timeZone.GetValue<string>("tzinfo_name");
            UtcOffset = timeZone.GetValue<int>("utc_offset");
        }

        /// <summary>
        /// Human-readable timezone name
        /// </summary>
        /// <example>Pacific Time (US &amp; Canada)</example>
        public string Name { get; set; }

        /// <summary>
        /// Rails/unix TZINFO name
        /// </summary>
        /// <example>America/Los_Angeles</example>
        public string TzInfoName { get; set; }

        /// <summary>
        /// Seconds to subtract from UTC time
        /// </summary>
        /// <example>-28800</example>
        public int? UtcOffset { get; set; }
    }
}
