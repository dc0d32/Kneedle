# Kneedle
Implementation of knee/elbow finding algorithm 'Kneedle' in c#.

Calculates knee points using the Kneedle algorithm. Returns the x value corresponding to the knee point when successful, null otherwise.

## Reference
    "Finding a ‘kneedle’ in a haystack: Detecting knee points in system behavior. "
    Satopaa, V and Albrecht, J and Irwin, D and Raghavan, B
    31-st International Conference on Distributed Computing Systems
    2011
[https://raghavan.usc.edu/papers/kneedle-simplex11.pdf]


## Inputs
Needs the data points provided x-sorted.

Positive curvature is when the tangent traces anti-clockwise.

## Output
Returns the x value corresponding to the knee point.

Can return null when the algorithm fails to identify a knee/elbow for various reasons:
- the number of data points is too small
- there are no local maxima on the diffs, which means either the curve is a line, or the parameters provided are incompatible with the curve

