using System;
using System.Collections.Generic;
using System.Text;

namespace spc_desktop_notifications
{
    internal class Point
    {
        public float lat;
        public float lon;

        public Point(float latitude, float longitude)
        {
            lat = latitude;
            lon = longitude;
        }
    }
}
