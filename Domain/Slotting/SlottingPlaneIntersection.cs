using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waffle.Domain.Slotting
{
    internal class SlottingPlaneIntersection
    {
        public Point3d Point { get; private set; }
        public int CurveIndex { get; private set; }

        public SlottingPlaneIntersection(Point3d point, int curveIndex)
        {
            Point = point;
            CurveIndex = curveIndex;
        }
    }
}
