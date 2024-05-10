using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino;

namespace Waffle.Domain.Slicing
{
    public class Slice
    {
        public Plane Plane { get; private set; }
        public Curve[] Curves { get; private set; }

        public Slice(Brep brep, Plane slicingPlane)
        {
            Plane = slicingPlane;
            Curve[] curves = Brep.CreateContourCurves(brep, slicingPlane);
            Curves = new PolylineCurve[curves.Length];

            for (int i = 0; i < Curves.Length; i++)
            {
                Curves[i] = curves[i].IsPolyline()
                    ? curves[i]
                    : curves[i].ToPolyline(
                        RhinoMath.DefaultDistanceToleranceMillimeters,
                        RhinoMath.DefaultAngleTolerance,
                        0.01, 10.0);
            }
        }
    }
}
