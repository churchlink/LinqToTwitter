﻿using System.Xml.Serialization;

namespace LinqToTwitter
{
    /// <summary>
    /// Allows working at a low level with Twitter API
    /// </summary>
    [XmlType(Namespace = "LinqToTwitter")]
    public class Raw
    {
        /// <summary>
        /// Query string with segments and parameters.  
        /// Do not include BaseUrl as it will be prefixed 
        /// to this value. You're resposible for ensuring
        /// the format of this part of the query is correct,
        /// including encoding parameters.
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// Raw result, returned directly from Twitter.
        /// </summary>
        public string Response { get; set; }
    }
}
