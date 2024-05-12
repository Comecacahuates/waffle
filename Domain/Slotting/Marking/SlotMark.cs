using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waffle.Domain.Slotting.Marking
{
    internal class SlotMark
    {
        public int CurveIndex { get; private set; }
        public Line line { get; private set; }

        public SlotMark(SlottingPlaneIntersection startIntersection, SlottingPlaneIntersection endIntersection)
        {
            CurveIndex = startIntersection.CurveIndex;
            line = new Line(startIntersection.Point, endIntersection.Point);
        }
    }
}
