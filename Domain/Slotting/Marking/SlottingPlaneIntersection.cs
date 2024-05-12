using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waffle.Domain.Slotting.Marking
{
    internal class SlottingPlaneIntersection
    {
        public Point3d Point { get; private set; }
        public int CurveIndex { get; private set; }

        public SlottingPlaneIntersection(IntersectionEvent intersectionEvent, int curveIndex)
        {
            if (!intersectionEvent.IsPoint)
                throw new Exception("Intersection event is not a point");

            Point = intersectionEvent.PointA;
            CurveIndex = curveIndex;
        }
    }
}
