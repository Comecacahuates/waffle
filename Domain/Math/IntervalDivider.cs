using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Waffle.Domain.Math
{
    public static class IntervalDivider
    {
        public static double[] DivideIntervalInNumbers(Interval interval, int divisionCount)
        {
            double step = interval.Length / divisionCount;
            double[] values = new double[divisionCount];

            for (int i = 0; i < divisionCount + 1; i++)
                values[i] = interval.T0 + step * i;

            return values;
        }
    }
}
