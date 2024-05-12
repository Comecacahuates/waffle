using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Waffle.Domain.Math;

namespace Waffle.Domain.Slicing
{
    public class LinearSlicingPlanes : IEnumerable<Plane>
    {
        private Plane[] planes;
        public int Count => planes.Length;
        public Plane this[int key] { get => planes[key]; }

        public IEnumerator<Plane> GetEnumerator()
        {
            foreach (Plane plane in planes) yield return plane;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return planes.GetEnumerator();
        }

        public LinearSlicingPlanes(Plane basePlane, Brep brep, double distanceBetweenSlices)
        {
            BoundingBox boundingBox = brep.GetBoundingBox(basePlane);

            double minimumW = getMinimumW(boundingBox, basePlane),
                firstPlaneW = Rounder.RoundUp(minimumW, distanceBetweenSlices);

            double maximumW = getMaximumW(boundingBox, basePlane),
                lastPlaneW = Rounder.RoundDown(maximumW, distanceBetweenSlices);

            Interval wInterval = new Interval(firstPlaneW, lastPlaneW);

            int divisionCount = (int)(wInterval.Length / distanceBetweenSlices);

            double[] wOfPlanes = IntervalDivider.DivideIntervalInNumbers(wInterval, divisionCount);

            planes = buildPlanes(basePlane, wOfPlanes);

        }

        private double getMinimumW(BoundingBox box, Plane plane)
        {
            Transform orientToWorldXY = Transform.ChangeBasis(plane, Plane.WorldXY);
            Point3d lowestPointRelativeToPlane = orientToWorldXY * box.Min;

            return lowestPointRelativeToPlane.Z;
        }

        private double getMaximumW(BoundingBox box, Plane plane)
        {
            Transform orientToWorldXY = Transform.ChangeBasis(plane, Plane.WorldXY);
            Point3d highestPointRelativeToPlane = orientToWorldXY * box.Max;

            return highestPointRelativeToPlane.Z;
        }

        private Plane[] buildPlanes(Plane basePlane, double[] wOfPlanes)
        {
            int planeCount = wOfPlanes.Length;
            Plane[] planes = new Plane[planeCount];

            for (int i = 0; i < planeCount; i++)
            {
                double eachW = wOfPlanes[i];
                Point3d origin = basePlane.PointAt(0, 0, eachW);
                planes[i] = new Plane(origin, basePlane.Normal);
            }

            return planes;
        }


    }
}
