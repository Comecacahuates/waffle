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
        public Plane this[int key] => planes[key];

        public IEnumerator<Plane> GetEnumerator()
        {
            foreach (Plane plane in planes) yield return plane;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return planes.GetEnumerator();
        }

        public LinearSlicingPlanes(Plane plane, Brep brep, double distanceBetweenSlices)
        {
            BoundingBox boundingBox = brep.GetBoundingBox(plane);

            double minimumW = boundingBox.Min.Z,
                firstPlaneW = Rounder.RoundUp(minimumW, distanceBetweenSlices);

            double maximumW = boundingBox.Max.Z,
                lastPlaneW = Rounder.RoundDown(maximumW, distanceBetweenSlices);

            Interval wCoordinatesInterval = new Interval(firstPlaneW, lastPlaneW);

            int divisionCount = (int)System.Math.Floor(wCoordinatesInterval.Length / distanceBetweenSlices);

            Point3d[] planeOrigins = buildPlaneOrigins(plane, wCoordinatesInterval, divisionCount);

            planes = buildPlanes(plane, planeOrigins);
        }

        private Point3d[] buildPlaneOrigins(Plane plane, Interval wCoordinatesInterval, int divisionCount)
        {
            double[] wCoordinates = IntervalDivider.DivideIntervalInNumbers(wCoordinatesInterval, divisionCount);
            Point3d[] planeOrigins = new Point3d[wCoordinates.Length];

            int index = 0;
            foreach (double eachWCoordinate in wCoordinates)
                planeOrigins[index++] = plane.PointAt(0, 0, eachWCoordinate);

            return planeOrigins;
        }

        private Plane[] buildPlanes(Plane basePlane, Point3d[] planeOrigins)
        {
            Plane[] planes = new Plane[planeOrigins.Length];

            int index = 0;
            foreach (Point3d eachPlaneOrigin in planeOrigins)
            {
                Plane plane = basePlane.Clone();
                plane.Origin = eachPlaneOrigin;
                planes[index++] = plane;
            }

            return planes;
        }
    }
}
