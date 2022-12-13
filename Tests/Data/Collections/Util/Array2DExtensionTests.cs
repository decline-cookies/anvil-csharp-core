using NUnit.Framework;
using System;
using Anvil.CSharp.Data;

namespace Anvil.CSharp.Tests
{
    public static class Array2DExtensionTests
    {
        [Test, Order(0)]
        public static void PopulateTest()
        {
            Assert.That(nameof(PopulateTest), Is.EqualTo(nameof(Array2DExtension.Populate) + "Test"));

            const int SIZE = 5;

            int[,] array = new int[SIZE, SIZE];
            array.Populate((x, y) => x + SIZE*y);

            for (int y = 0; y < SIZE; y++)
            {
                for (int x = 0; x < SIZE; x++)
                {
                    Assert.That(array[x, y], Is.EqualTo(x + SIZE*y));
                }
            }
        }

        [Test]
        public static void GetLengthTest()
        {
            Assert.That(nameof(GetLengthTest), Is.EqualTo(nameof(Array2DExtension.GetLength) + "Test"));

            Assert.That(new int[1, 1].GetLength(), Is.EqualTo((1, 1)));
            Assert.That(new int[2, 3].GetLength(), Is.EqualTo((2, 3)));
            Assert.That(new int[24, 42].GetLength(), Is.EqualTo((24, 42)));
        }

        [Test]
        public static void AnyAllTest()
        {
            Assert.That(nameof(AnyAllTest), Is.EqualTo($"{nameof(Array2DExtension.Any)}{nameof(Array2DExtension.All)}Test"));

            bool[,] array = new bool[5, 5];
            array.Populate((x, y) => true);

            Assert.That(array.All((val, x, y) => val == true), Is.EqualTo(true));
            Assert.That(array.All((val, x, y) => val == false), Is.EqualTo(false));
            Assert.That(array.Any((val, x, y) => val == true), Is.EqualTo(true));
            Assert.That(array.Any((val, x, y) => val == false), Is.EqualTo(false));

            array[2, 2] = false;

            Assert.That(array.All((val, x, y) => val == true), Is.EqualTo(false));
            Assert.That(array.All((val, x, y) => val == false), Is.EqualTo(false));
            Assert.That(array.Any((val, x, y) => val == true), Is.EqualTo(true));
            Assert.That(array.Any((val, x, y) => val == false), Is.EqualTo(true));
        }

        [Test]
        public static void ForEachTest()
        {
            Assert.That(nameof(ForEachTest), Is.EqualTo(nameof(Array2DExtension.ForEach) + "Test"));

            const int SIZE = 5;

            int[,] array = new int[SIZE, SIZE];
            array.Populate((x, y) => x + SIZE*y);

            array.ForEach((value, x, y) => Assert.That(value, Is.EqualTo(x + SIZE*y)));

            int index = 0;
            array.ForEach(value => Assert.That(value, Is.EqualTo(index++)));
        }

        [Test]
        public static void FindIndexTest()
        {
            Assert.That(nameof(FindIndexTest), Is.EqualTo(nameof(Array2DExtension.FindIndex) + "Test"));

            const int SIZE = 5;

            int[,] array = new int[SIZE, SIZE];
            array.Populate((x, y) => x + SIZE*y);

            Assert.That(array.FindIndex(value => value == 13), Is.EqualTo((3, 2)));

            Assert.That(array.FindIndex(value => value == -5), Is.EqualTo((-1, -1)));
        }

        [Test]
        public static void GetElementOrDefaultAtTest()
        {
            Assert.That(nameof(GetElementOrDefaultAtTest), Is.EqualTo(nameof(Array2DExtension.GetElementOrDefaultAt) + "Test"));

            const int SIZE = 5;

            int[,] array = new int[SIZE, SIZE];
            array.Populate((x, y) => x + SIZE*y);

            Assert.That(array.GetElementOrDefaultAt(3, 2), Is.EqualTo(13));

            Assert.That(array.GetElementOrDefaultAt(6, 8), Is.EqualTo(default(int)));
            Assert.That(array.GetElementOrDefaultAt(6, 8, -1), Is.EqualTo(-1));
        }
    }
}