using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waffle.Domain.Math
{
    public static class Rounder
    {
        public static double RoundUp(double value, double step = 1)
        {
            return System.Math.Ceiling(value / step) * step;
        }

        public static double RoundDown(double value, double step = 1)
        {
            return System.Math.Floor(value / step) * step;
        }
    }
}
