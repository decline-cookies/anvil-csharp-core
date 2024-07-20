using System.Collections.Generic;
using System.Linq;
using Anvil.CSharp.Collections;
using NUnit.Framework;

namespace Anvil.CSharp.Tests
{
    public static class EnumerableExtensionTests
    {
        [Test]
        public static void FindIndexTest()
        {
            Assert.That(nameof(FindIndexTest), Is.EqualTo(nameof(EnumerableExtension.FindIndex) + "Test"));

            IEnumerable<int> numbers = Enumerable.Range(10, 50).ToArray();

            Assert.That(numbers.FindIndex(x => x == 10), Is.EqualTo(0));
            Assert.That(numbers.FindIndex(x => x == 42), Is.EqualTo(32));
            Assert.That(numbers.FindIndex(x => x == -9999), Is.EqualTo(-1));
        }

        [Test]
        public static void IndexOfTest()
        {
            Assert.That(nameof(IndexOfTest), Is.EqualTo(nameof(EnumerableExtension.IndexOf) + "Test"));

            IEnumerable<int> numbers = Enumerable.Range(10, 50).ToArray();

            Assert.That(numbers.IndexOf(10), Is.EqualTo(0));
            Assert.That(numbers.IndexOf(42), Is.EqualTo(32));
            Assert.That(numbers.IndexOf(-9999), Is.EqualTo(-1));
        }
    }
}
