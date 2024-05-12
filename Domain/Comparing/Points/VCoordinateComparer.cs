using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Waffle.Domain.Comparing.Points
{
    public class VCoordinateComparer : IComparer<Point3d>
    {
        private Plane plane;

        public VCoordinateComparer(Plane plane)
        {
            this.plane = plane;
        }

        int IComparer<Point3d>.Compare(Point3d pointA, Point3d pointB)
        {
            Point3d pointToCompareA = getPointRelativeToPlane(pointA);
            Point3d pointToCompareB = getPointRelativeToPlane(pointB);

            if (pointToCompareA.Y < pointToCompareB.Y)
                return -1;

            else if (pointToCompareA.Y > pointToCompareB.Y)
                return 1;

            else
                return 0;
        }

        private Point3d getPointRelativeToPlane(Point3d point)
        {
            Transform orientToWorldXY = Transform.ChangeBasis(plane, Plane.WorldXY);
            return orientToWorldXY * point;
        }
    }
}
