using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Waffle.Domain.Slicing
{
    public class LinearSlicer
    {
        public Plane[] Planes { get; private set; }

        public LinearSlicer(Plane basePlane, Brep brep, double distanceBetweenSlices)
        {
            LinearSlicingPlanes slicingPlanes = new LinearSlicingPlanes(basePlane, brep, distanceBetweenSlices);

            int sliceCount = slicingPlanes.Length;
            Slice[] slices = new Slice[sliceCount];

            for (int i = 0; i < sliceCount; i++)
            {
                Plane eachPlane = slicingPlanes[i];
                slices[i] = new Slice(brep, eachPlane);
            }
        }
    }
}
