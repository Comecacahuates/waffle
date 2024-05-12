using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waffle.Domain.Comparing.Points;

namespace Waffle.Domain.Slotting.Marking
{
    internal class SortedSlottingPlaneIntersections : IEnumerable<SlottingPlaneIntersection>
    {
        private SortedList<Point3d, SlottingPlaneIntersection> sortedList;

        public SlottingPlaneIntersection this[int index]
        {
            get => sortedList.Values[index];
        }

        public IEnumerator<SlottingPlaneIntersection> GetEnumerator()
        {
            return sortedList.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get => sortedList.Count; }

        public SortedSlottingPlaneIntersections(Plane plane)
        {
            VCoordinateComparer comparer = new VCoordinateComparer(plane);
            sortedList = new SortedList<Point3d, SlottingPlaneIntersection>(comparer);
        }

        public void Add(SlottingPlaneIntersection intersection)
        {
            sortedList.Add(intersection.Point, intersection);
        }
    }
}
