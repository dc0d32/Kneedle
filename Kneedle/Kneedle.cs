

using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kneedle
{
    public enum CurveDirection
    {
        Increasing,
        Decreasing
    }

    public enum Curvature
    {
        /* tangent goes anti-clockwise */
        Counterclockwise,
        /* tangent goes clockwise */
        Clockwise
    }

    public static class KneedleAlgorithm
    {
        /**
         * <summary>
         * <para>
         * Calculates knee points using the Kneedle algorithm. Returns the x value corresponding to the knee
         * point when successful, null otherwise.
         * </para>
         * <para>
         * Reference: 
         *      Finding a ‘kneedle’in a haystack: Detecting knee points in system behavior. 
         *      Satopaa, V and Albrecht, J and Irwin, D and Raghavan, B
         *      <see cref="https://raghavan.usc.edu/papers/kneedle-simplex11.pdf"/>
         * </para>
         * 
         *  <list type="bullet">
         *  <param name="x">x: X axis values of the points. Points must be sorted in ascending order w.r.t. X axis.</param>
         *  <param name="y">y: Y axis values of the points.</param>
         *  <param name="direction">direction: If the curve is increasing or decreasing. Make sure to set this value according to the input curve.</param>
         *  <param name="concavity">concavity: Whether the curve has positive or negative curvature. In other words, concave or convex. Whether the tangent rotates clockwise or counterclockwise. Make sure to set this value according to the input curve.</param>
         *  <param name="sensitivity">sensitivity: Adjusts the knee detection threshold. Defaults to 1 as per the paper.</param>
         *  <param name="forceLinearInterpolation">forceLinearInterpolation: Interpolation is done using robust cubic splines. For some inputs, spline can overshoot. This param forces linear interpolation instead of cubic spline.</param>
         *  </list>
         *  
         * Can return null when the algorithm fails to identify a knee/elbow for various reasons:
         *      - the number of data points is too small
         *      - there are no local maxima on the diffs, which means either the curve is a line, or the
         *        parameters provided are incompatible with the curve
         *  
         *  <list type="bullet">
         *  2019-01-08: rename curvature enum to be easy to interpret and remember (Prashant Borole)
         *  2019-01-07: initial version (Prashant Borole)
         *  </list>
         *  </summary>
         */
        public static double? CalculateKneePoints(double[] x, double[] y, CurveDirection direction, Curvature concavity, double sensitivity = 1, bool forceLinearInterpolation = true)
        {
            if (x == null || y == null || x.Length != y.Length || x.Length < 2)
                return null;

            var numPoints = x.Length;

            MathNet.Numerics.Interpolation.IInterpolation interpolator = null;
            if (numPoints > 5 && !forceLinearInterpolation)
            {
                interpolator = Interpolate.CubicSplineRobust(x, y);
            }
            else
            {
                interpolator = Interpolate.Linear(x, y);
            }
            var x_spaced = Generate.LinearSpaced(numPoints, x.Min(), x.Max());
            var y_spaced = Generate.Map(x_spaced, interpolator.Interpolate);

            var x_norm = MinMaxNormalize(x_spaced);
            var y_norm = MinMaxNormalize(y_spaced);

            var x_diff = x_norm;
            var y_diff = new double[numPoints];
            if (direction == CurveDirection.Decreasing)
            {
                for (int i = 0; i < numPoints; i++)
                {
                    y_diff[i] = x_norm[i] + y_norm[i];
                }
                if (concavity == Curvature.Counterclockwise)
                {
                    for (int i = 0; i < numPoints; i++)
                    {
                        y_diff[i] = 1 - y_diff[i];
                    }
                }
            }
            else
            {
                // increasing
                for (int i = 0; i < numPoints; i++)
                {
                    y_diff[i] = y_norm[i] - x_norm[i];
                }
                if (concavity == Curvature.Counterclockwise)
                {
                    for (int i = 0; i < numPoints; i++)
                    {
                        y_diff[i] = Math.Abs(y_diff[i]);
                    }
                }
            }


            // find local maxima
            var xmx_idxs = FindLocalExtrema(y_diff, true);
            if (xmx_idxs.Count == 0)
                return null;
            var xmx = xmx_idxs.Select(idx => x_diff[idx]).ToArray();
            var ymx = xmx_idxs.Select(idx => y_diff[idx]).ToArray();

            // minima
            var xmn_idxs = FindLocalExtrema(y_diff, false);
            var xmn = xmn_idxs.Select(idx => x_diff[idx]).ToArray();
            var ymn = xmn_idxs.Select(idx => y_diff[idx]).ToArray();

            var tmx = Threshold(ymx, x_norm, sensitivity);

            // now find the knee point between each of the local maxima
            var curMaximaIdx = 0;
            var xmn_idxs_set = new HashSet<int>(xmn_idxs);
            double? knee = null;
            for (int x_i = xmx_idxs[0] + 1; x_i < x.Length; x_i++)
            {
                if (curMaximaIdx < xmx_idxs.Count - 1 && x_i == xmx_idxs[curMaximaIdx + 1])
                {
                    curMaximaIdx++;
                    x_i++;
                    continue;
                }

                if (xmn_idxs_set.Contains(x_i))
                {
                    if (x_i < x.Length - 1 && y_diff[x_i + 1] > y_diff[x_i])
                    {
                        tmx[curMaximaIdx] = 0;
                    }
                }

                if (y_diff[x_i] < tmx[curMaximaIdx] || tmx[curMaximaIdx] < 0)
                {
                    knee = x[xmx_idxs[curMaximaIdx]];
                }
            }

            return knee;

        }

        private static double[] Threshold(double[] ymx, double[] x_norm, double scale)
        {
            var diffSum = 0d;
            for (int i = 1; i < x_norm.Length; i++)
            {
                diffSum += x_norm[i] - x_norm[i - 1];
            }
            var diffMean = diffSum / (x_norm.Length - 1);
            var ret = new double[ymx.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = ymx[i] - (scale * diffMean);
            }
            return ret;
        }

        public static List<int> FindLocalExtrema(double[] arr, bool max)
        {
            var ret = new List<int>();
            for (int i = 0; i < arr.Length; i++)
            {
                var prev = arr[LimitValueToRange(i - 1, 0, arr.Length - 1)];
                var cur = arr[i];
                var next = arr[LimitValueToRange(i + 1, 0, arr.Length - 1)];

                var lcomp = max ? cur > prev : cur < prev;
                var rcomp = max ? cur > next : cur < next;

                if (lcomp && rcomp)
                {
                    ret.Add(i);
                }
            }
            return ret;
        }

        public static int LimitValueToRange(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private static double[] MinMaxNormalize(double[] vec)
        {
            var ret = new double[vec.Length];
            double min = vec[0], max = vec[0];
            for (int i = 1; i < vec.Length; i++)
            {
                min = Math.Min(min, vec[i]);
                max = Math.Max(max, vec[i]);
            }

            var denom = max - min;
            for (int i = 0; i < vec.Length; i++)
            {
                ret[i] = (vec[i] - min) / denom;
            }

            return ret;
        }
    }
}
