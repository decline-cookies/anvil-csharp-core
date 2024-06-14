using Anvil.CSharp.Mathematics;
using NUnit.Framework;

namespace Anvil.CSharp.Tests
{
    public static class MathUtilTests
    {
        [Test]
        public static void FindPrimeNumberTest()
        {
            Assert.That(nameof(FindPrimeNumberTest), Is.EqualTo(nameof(MathUtil.FindPrimeNumber) + "Test"));

            //TODO: #129 - There's probably a more NUnit way to do this
            int[] primeNumbers = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            for (int i = 0; i < primeNumbers.Length; i++)
            {
                Assert.That(MathUtil.FindPrimeNumber(i), Is.EqualTo(primeNumbers[i]));
            }

            Assert.That(MathUtil.FindPrimeNumber(100), Is.EqualTo(541));
        }

        [Test]
        public static void ClosestDeltaTest()
        {
            Assert.That(nameof(ClosestDeltaTest), Is.EqualTo(nameof(MathUtil.ClosestDelta) + "Test"));

            //TODO: #129 - There's probably a more NUnit way to do this

            Assert.That(MathUtil.ClosestDelta(5, 10), Is.EqualTo(5));
            Assert.That(MathUtil.ClosestDelta(10, 5), Is.EqualTo(-5));

            Assert.That(MathUtil.ClosestDelta(-5, -10), Is.EqualTo(-5));
            Assert.That(MathUtil.ClosestDelta(-10, -5), Is.EqualTo(5));

            Assert.That(MathUtil.ClosestDelta(int.MaxValue - 1, int.MinValue + 1), Is.EqualTo(3));
            Assert.That(MathUtil.ClosestDelta(int.MinValue + 1, int.MaxValue - 1), Is.EqualTo(-3));
        }
    }
}