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

        public Slice(Curve[] curves, Plane plane)
        {
            Curves = curves;
            Plane = plane;
        }

        public Slice Duplicate()
        {
            Curve[] curves = new Curve[Curves.Length];

            int curveIndex = 0;
            foreach (Curve eachCurve in Curves)
                curves[curveIndex++] = eachCurve.DuplicateCurve();

            return new Slice(curves, Plane);
        }
    }
}
