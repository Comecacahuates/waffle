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

        public SlotMark(int curveIndex, Line line)
        {
            CurveIndex = curveIndex;
            this.line = line;
        }
    }
}
