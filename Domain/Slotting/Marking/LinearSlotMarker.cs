
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Waffle.Domain.Slicing;

namespace Waffle.Domain.Slotting.Marking
{
    internal class LinearSlotMarker
    {
        private Slice slice;

        public LinearSlotMarker(Slice slice)
        {
            this.slice = slice;
        }

        public SlotMark[] getSlotMarks(Plane slotPlane)
        {

            SortedSlottingPlaneIntersections intersections = getIntersectionsWithSlotPlane(slotPlane);

            if (intersections.Count % 2 != 0)
                throw new Exception("Odd number of intersections");

            SlotMark[] slotMarks = buildSlotMarks(intersections);

            return slotMarks;
        }

        private SortedSlottingPlaneIntersections getIntersectionsWithSlotPlane(Plane slotPlane)
        {
            SortedSlottingPlaneIntersections intersections = new SortedSlottingPlaneIntersections(slotPlane);

            int curveIndex = 0;
            foreach (Curve eachCurve in slice.Curves)
            {
                CurveIntersections curveIntersections = Intersection.CurvePlane(eachCurve, slotPlane, RhinoMath.DefaultDistanceToleranceMillimeters);
                foreach (IntersectionEvent eachIntersectionEvent in curveIntersections)
                {
                    SlottingPlaneIntersection intersection = new SlottingPlaneIntersection(eachIntersectionEvent, curveIndex);
                    intersections.Add(intersection);
                }

                curveIndex++;
            }

            return intersections;
        }

        private SlotMark[] buildSlotMarks(SortedSlottingPlaneIntersections intersections)
        {
            if (intersections.Count % 2 != 0)
                throw new Exception("Odd number of intersections");

            int slotCount = intersections.Count / 2;
            SlotMark[] slotMarks = new SlotMark[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                SlottingPlaneIntersection startIntersection = intersections[i * 2];
                SlottingPlaneIntersection endIntersection = intersections[i * 2 + 1];

                slotMarks[i] = new SlotMark(startIntersection, endIntersection);
            }

            return slotMarks;
        }
    }
}
