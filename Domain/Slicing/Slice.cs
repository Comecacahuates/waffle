using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Waffle.Domain.Slicing
{
    public class Slice
    {
        public Plane Plane { get; private set; }
        public Curve[] Curves { get; private set; }

        public Slice(Brep brep, Plane slicingPlane)
        {
            Plane = slicingPlane;
            Curves = Brep.CreateContourCurves(brep, slicingPlane);
        }
    }
}
