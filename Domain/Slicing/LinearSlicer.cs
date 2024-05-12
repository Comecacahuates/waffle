using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

namespace Waffle.Domain.Slicing
{
    public class LinearSlicer
    {
        public Plane[] Planes { get; private set; }

        public LinearSlicer(Plane basePlane, Brep brep, double distanceBetweenSlices)
        {
            LinearSlicingPlanes slicingPlanes = new LinearSlicingPlanes(basePlane, brep, distanceBetweenSlices);

            int sliceCount = slicingPlanes.Count;
            Slice[] slices = new Slice[sliceCount];

            int sliceIndex = 0;
            foreach (Plane eachPlane in slicingPlanes)
            {
                Curve[] sliceCourves = getSliceCourves(brep, eachPlane);
                slices[sliceIndex] = new Slice(sliceCourves, eachPlane);
                sliceIndex++;
            }
        }

        private Curve[] getSliceCourves(Brep brep, Plane plane)
        {
            Curve[] contourCurves = Brep.CreateContourCurves(brep, plane);
            Curve[] Curves = new Curve[contourCurves.Length];

            int curveIndex = 0;
            foreach (Curve eachContourCurve in contourCurves)
            {
                Curves[curveIndex] = convertToPolyline(eachContourCurve);
                curveIndex++;
            }

            return Curves;
        }

        private Curve convertToPolyline(Curve curve)
        {
            if (curve.IsPolyline()) return curve;

            return curve.ToPolyline(
                RhinoMath.DefaultDistanceToleranceMillimeters,
                RhinoMath.DefaultAngleTolerance,
                0.01, 10.0);
        }
    }
}
