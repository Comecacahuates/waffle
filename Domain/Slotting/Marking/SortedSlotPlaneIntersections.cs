using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waffle.Domain.Comparing.Points;

namespace Waffle.Domain.Slotting.Marking
{
    internal class SortedSlotPlaneIntersections : IEnumerable<SlotPlaneIntersection>
    {
        private SortedList<Point3d, SlotPlaneIntersection> sortedList;

        public SlotPlaneIntersection this[int index]
        {
            get => sortedList.Values[index];
        }

        public IEnumerator<SlotPlaneIntersection> GetEnumerator()
        {
            return sortedList.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get => sortedList.Count; }

        public SortedSlotPlaneIntersections(Plane plane)
        {
            VCoordinateComparer comparer = new VCoordinateComparer(plane);
            sortedList = new SortedList<Point3d, SlotPlaneIntersection>(comparer);
        }

        public void Add(SlotPlaneIntersection intersection)
        {
            sortedList.Add(intersection.Point, intersection);
        }
    }
}
