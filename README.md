# Kneedle
Implementation of knee/elbow finding algorithm 'Kneedle' in C#.

Calculates knee points using the Kneedle algorithm. Returns the x value corresponding to the knee point when successful, null otherwise.

Contains only batch mode implementation right now. Contributoins welcome for online/streaming implementation, as well as any other improvements!

## Reference
    "Finding a ‘kneedle’ in a haystack: Detecting knee points in system behavior. "
    Satopaa, V and Albrecht, J and Irwin, D and Raghavan, B
    31-st International Conference on Distributed Computing Systems
    2011
https://raghavan.usc.edu/papers/kneedle-simplex11.pdf

## Usage



```csharp
var y = new double[] { 1, 1, 0.894, 0.864, 0.859, 0.856, 0.852, 0.851, 0.849, 0.848, 0.847, 0.845 };
var x = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

var k1 = KneedleAlgorithm.CalculateKneePoints(x, y, CurveDirection.Decreasing, Curvature.Counterclockwise, forceLinearInterpolation: false); 
// Returns 4

var k2 = KneedleAlgorithm.CalculateKneePoints(x, y, CurveDirection.Decreasing, Curvature.Clockwise, forceLinearInterpolation: false);
// Returns 2

```

## Inputs

**x**: X axis values of the points. Points must be sorted in ascending order w.r.t. X axis.

**y**: Y axis values of the points.

**direction**: If the curve is increasing or decreasing. Make sure to set this value according to the input curve.

**concavity**: Whether the curve has positive or negative curvature. In other words, concave or convex. Whether the tangent rotates clockwise or counterclockwise. Make sure to set this value according to the input curve.

**sensitivity**: Adjusts the knee detection threshold. Defaults to 1 as per the paper.

**forceLinearInterpolation**: Interpolation is done using robust cubic splines. For some inputs, spline can overshoot. This param forces linear interpolation instead of cubic spline.

## Output
Returns the x value corresponding to the knee point.

Can return null when the algorithm fails to identify a knee/elbow for various reasons:
- the number of data points is too small
- there are no local maxima on the diffs, which means either the curve is a line, or the parameters provided are incompatible with the curve

## Dependencies

*MathNet.Numerics* from NuGet.