using System;
using Anvil.CSharp.Mathematics;
using NUnit.Framework;

namespace Anvil.CSharp.Tests
{
    public static class MathUtilTests
    {
        [Test]
        public static void FindPrimeNumber()
        {
            //TODO: #129 - There's probably a more NUnit way to do this
            int[] primeNumbers = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            for (int i = 0; i < primeNumbers.Length; i++)
            {
                Assert.That(MathUtil.FindPrimeNumber(i), Is.EqualTo(primeNumbers[i]));
            }

            Assert.That(MathUtil.FindPrimeNumber(100), Is.EqualTo(541));
        }

        [Test]
        public static void GetDigitCount()
        {
            Assert.That(MathUtil.GetDigitCount(0), Is.EqualTo(1));
            Assert.That(MathUtil.GetDigitCount(0, true), Is.EqualTo(1));

            TestValue(1, 1);

            int iterations = (int)(Math.Log(int.MaxValue) / Math.Log(10));
            for (int i = 1; i <= iterations; i++)
            {
                int val = (int)Math.Pow(10, i);
                TestValue(val-1, i);   // Ex: 99, 2
                TestValue(val, i+1);   // Ex: 100, 3
            }

            void TestValue(int positiveValue, int expectedDigits)
            {
                Assert.That(MathUtil.GetDigitCount(positiveValue), Is.EqualTo(expectedDigits));
                Assert.That(MathUtil.GetDigitCount(positiveValue, true), Is.EqualTo(expectedDigits));
                Assert.That(MathUtil.GetDigitCount(-positiveValue), Is.EqualTo(expectedDigits));
                Assert.That(MathUtil.GetDigitCount(-positiveValue, true), Is.EqualTo(expectedDigits+1));
            }
        }
    }
}