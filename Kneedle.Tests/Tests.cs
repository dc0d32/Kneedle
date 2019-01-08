using NUnit.Framework;
using System;
using System.Linq;

namespace KneedleTests
{
    public class Tests
    {
        static double[] x_all, y_pos_inc, y_pos_dec, y_neg_inc, y_neg_dec;

        [SetUp]
        public void Setup()
        {
            x_all = Enumerable.Range(0, 100).Select(v => (double)v).ToArray();
            y_pos_inc = x_all.Select(v => Math.Pow(1.05, v)).ToArray();
            y_pos_dec = y_pos_inc.Reverse().ToArray();
            y_neg_inc = y_pos_dec.Select(v => y_pos_dec.Max() - v).ToArray();
            y_neg_dec = y_neg_inc.Reverse().ToArray();
        }

        [Test]
        public void TestInvalidInputs()
        {
            var k = Kneedle.KneedleAlgorithm.CalculateKneePoints(null, null, Kneedle.CurveDirection.Decreasing, Kneedle.Curvature.Negative);
            Assert.IsNull(k);
            k = Kneedle.KneedleAlgorithm.CalculateKneePoints(new double[0], new double[1], Kneedle.CurveDirection.Decreasing, Kneedle.Curvature.Negative);
            Assert.IsNull(k);
            k = Kneedle.KneedleAlgorithm.CalculateKneePoints(new double[1], new double[1], Kneedle.CurveDirection.Decreasing, Kneedle.Curvature.Negative);
            Assert.IsNull(k);
        }

        [Test]
        public void TestCurveWithoutKnee()
        {
            var k = Kneedle.KneedleAlgorithm.CalculateKneePoints(new double[2], new double[2], Kneedle.CurveDirection.Decreasing, Kneedle.Curvature.Negative);
            Assert.IsNull(k);
        }

        [Test]
        public void TestShortCurve()
        {
            var y = new double[] { 2, 4, 10 };
            var x = Enumerable.Range(0, y.Length).Select(v => (double)v).ToArray();
            var k = Kneedle.KneedleAlgorithm.CalculateKneePoints(x, y, Kneedle.CurveDirection.Increasing, Kneedle.Curvature.Positive);
            Assert.IsNotNull(k);
            Assert.AreEqual(1d, k.Value);
        }

        [Test]
        public void TestSparseCurve()
        {
            var y = new double[] { 2, 4, 10 };
            var x = new double[] { 0, 7, 100 };
            var k = Kneedle.KneedleAlgorithm.CalculateKneePoints(x, y, Kneedle.CurveDirection.Increasing, Kneedle.Curvature.Positive);
            Assert.IsNotNull(k);
            Assert.AreEqual(7d, k.Value);
        }

        [Test]
        public void TestIncreasingPositiveCurvature()
        {
            var k = Kneedle.KneedleAlgorithm.CalculateKneePoints(x_all, y_pos_inc, Kneedle.CurveDirection.Increasing, Kneedle.Curvature.Positive);
            Assert.IsNotNull(k);
            Console.WriteLine(k.Value);
            Assert.AreEqual(k.Value, 67d);
        }

        [Test]
        public void TestDecreasingPositiveCurvature()
        {
            var k = Kneedle.KneedleAlgorithm.CalculateKneePoints(x_all, y_pos_dec, Kneedle.CurveDirection.Decreasing, Kneedle.Curvature.Positive);
            Assert.IsNotNull(k);
            Console.WriteLine(k.Value);
            Assert.AreEqual(k.Value, 32d);
        }

        [Test]
        public void TestIncreasingNegativeCurvature()
        {
            var k = Kneedle.KneedleAlgorithm.CalculateKneePoints(x_all, y_neg_inc, Kneedle.CurveDirection.Increasing, Kneedle.Curvature.Negative);
            Assert.IsNotNull(k);
            Console.WriteLine(k.Value);
            Assert.AreEqual(k.Value, 32d);
        }

        [Test]
        public void TestDecreasingNegativeCurvature()
        {
            var k = Kneedle.KneedleAlgorithm.CalculateKneePoints(x_all, y_neg_dec, Kneedle.CurveDirection.Decreasing, Kneedle.Curvature.Negative);
            Assert.IsNotNull(k);
            Console.WriteLine(k.Value);
            Assert.AreEqual(k.Value, 67d);
        }

        [Test]
        public void TestGaussianNegativeCurvature()
        {
            var numSamples = 10000;
            var rng = new Random();
            var x = MathNet.Numerics.Distributions.Normal.Samples(rng, 50, 10).Take(numSamples).ToArray();
            Array.Sort(x);
            var y = Enumerable.Range(0, numSamples).Select(v => 1.0 * v / numSamples).ToArray();

            var k = Kneedle.KneedleAlgorithm.CalculateKneePoints(x, y, Kneedle.CurveDirection.Increasing, Kneedle.Curvature.Negative);
            Assert.IsNotNull(k);
            Console.WriteLine(k.Value);
            Assert.IsTrue(Math.Abs(k.Value - 60.5) <= 5.0);
        }
    }
}