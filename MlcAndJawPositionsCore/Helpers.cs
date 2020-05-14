namespace Juntendo.MedPhys.Esapi.MlcAndJaw
{
    public static class Helpers
    {

        /// <summary>
        /// 1D linear interpolation
        /// </summary>
        /// <param name="x"> the value of the coordinate to be evaluated </param>
        /// <param name="x1"> the first coordinate where the function value is given </param>
        /// <param name="x2"> the second coordinate where the function value is given </param>
        /// <param name="f1"> function value at x1 </param>
        /// <param name="f2"> function value at x2 </param>
        /// <returns> interpolated value at x </returns>
        public static double LinearInterpolation1D(double x, double x1, double x2, double f1, double f2)
        {
            double gradient = (f2 - f1) / (x2 - x1);
            return f1 + gradient * (x - x1);
        }

        /// <summary>
        /// 2D bilinear interpolation
        /// </summary>
        /// <param name="x"> the value of the first coordinate to be evaluated </param>
        /// <param name="y"> the value of the second coordinate to be evaluated </param>
        /// <param name="x1"> the start value of the first coordinate where the function value is given </param>
        /// <param name="x2"> the end value of the first coordinate where the function value is given </param>
        /// <param name="y1"> the start value of the second coordinate where the function value is given </param>
        /// <param name="y2"> the end value of the second coordinate where the function value is given </param>
        /// <param name="f11"> function value at (x1, y1) </param>
        /// <param name="f12"> function value at (x1, y2) </param>
        /// <param name="f21"> function value at (x2, y1) </param>
        /// <param name="f22"> function value at (x2, y2) </param>
        /// <returns> interpolated value at (x, y) </returns>
        public static double BilinearInterpolation2D(double x, double y, double x1, double x2, double y1, double y2,
            double f11, double f12, double f21, double f22)
        {
            double denominator = 1.0 / ((x2 - x1) * (y2 - y1));
            double numerator = f11 * (x2 - x) * (y2 - y) + f12 * (x2 - x) * (y - y1)
                + f21 * (x - x1) * (y2 - y) + f22 * (x - x1) * (y - y1);
            return numerator / denominator;
        }

        /// <summary>
        /// 3D trilinear interpolation
        /// </summary>
        /// <param name="x"> the value of the first coordinate to be evaluated </param>
        /// <param name="y"> the value of the second coordinate to be evaluated </param>
        /// <param name="z"> the value of the third coordinate to be evaluated </param>
        /// <param name="x1"> the start value of the first coordinate where the function value is given </param>
        /// <param name="x2"> the end value of the first coordinate where the function value is given </param>
        /// <param name="y1"> the start value of the second coordinate where the function value is given </param>
        /// <param name="y2"> the end value of the second coordinate where the function value is given </param>
        /// <param name="z1"> the start value of the third coordinate where the function value is given </param>
        /// <param name="z2"> the start value of the third coordinate where the function value is given </param>
        /// <param name="f111"> function value at (x1, y1, z1) </param>
        /// <param name="f112"> function value at (x1, y1, z2) </param>
        /// <param name="f121"> function value at (x1, y2, z1) </param>
        /// <param name="f122"> function value at (x1, y2, z2) </param>
        /// <param name="f211"> function value at (x2, y1, z1) </param>
        /// <param name="f212"> function value at (x2, y1, z2) </param>
        /// <param name="f221"> function value at (x2, y2, z1) </param>
        /// <param name="f222"> function value at (x2, y2, z2) </param>
        /// <returns> interpolated value at (x, y) </returns>
        public static double TrilinearInterpolation3D(double x, double y, double z,
            double x1, double x2,
            double y1, double y2,
            double z1, double z2,
            double f111, double f112, double f121, double f122,
            double f211, double f212, double f221, double f222)
        {
            double denominator = 1.0 / ((x2 - x1) * (y2 - y1) * (z2 - z1));
            double numerator = f111 * (x2 - x) * (y2 - y) * (z2 - z)
                + f112 * (x2 - x) * (y2 - y) * (z - z1)
                + f121 * (x2 - x) * (y - y1) * (z2 - z)
                + f122 * (x2 - x) * (y - y1) * (z - z1)
                + f211 * (x - x1) * (y2 - y) * (z2 - z)
                + f212 * (x - x1) * (y2 - y) * (z - z1)
                + f221 * (x - x1) * (y - y1) * (z2 - z)
                + f222 * (x - x1) * (y - y1) * (z - z1);
            return numerator / denominator;
        }
    }
}
