﻿using System.Collections.Generic;
using System.Linq;

using LinqToTwitter.Common;
using LitJson;
using System.Xml.Serialization;

namespace LinqToTwitter
{
    /// <summary>
    /// Geographical area
    /// </summary>
    [XmlType(Namespace = "LinqToTwitter")]
    public class Geometry
    {
        public Geometry() {}
        internal Geometry(JsonData geometry)
        {
            if (geometry == null) return;

            Type = geometry.GetValue<string>("type");

            var coordinates = geometry.GetValue<JsonData>("coordinates");
            Coordinates =
                (from JsonData outer in coordinates
                 from JsonData coord in outer
                 select new Coordinate(coord))
                .ToList();
        }

        /// <summary>
        /// Type of bouding box
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Coordinates for bounding box
        /// </summary>
        public List<Coordinate> Coordinates { get; set; }
    }
}
