using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waffle.Domain
{
    public static class Math
    {
        public static double RoundUp(double value, double step)
        {
            return System.Math.Ceiling(value / step) * step;
        }
    }
}
