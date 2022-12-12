using System;
using Anvil.CSharp.Logging;
using NUnit.Framework;

namespace Anvil.CSharp.Tests.Logging
{
    public class ReadabilityExtensionTests
    {
        private class TestClass { }
        private class TestGenericClass<T> { }
        private class TestBigGenericClass<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> { }

        private class TestOuterGenericNestedClass<T>
        {
            public class TestInnerClass { }

            public class TestInnerGenericClass<InnerT> { }
        }

        [Test]
        [Order(1)]
        public static void GetShallowGenericTypeCountTest()
        {
            Assert.That(nameof(GetShallowGenericTypeCountTest), Is.EqualTo(nameof(ReadabilityExtension.GetShallowGenericTypeCount) + "Test"));

            Assert.That(typeof(int).GetShallowGenericTypeCount(), Is.EqualTo(0));

            Assert.That(typeof(int?).GetShallowGenericTypeCount(), Is.EqualTo(1));

            Assert.That(typeof(TestClass).GetShallowGenericTypeCount(), Is.EqualTo(0));

            Assert.That(typeof(TestGenericClass<int>).GetShallowGenericTypeCount(), Is.EqualTo(1));

            Assert.That(typeof(TestGenericClass<int?>).GetShallowGenericTypeCount(), Is.EqualTo(1));

            Assert.That(typeof(TestGenericClass<TestGenericClass<TestClass>>).GetShallowGenericTypeCount(), Is.EqualTo(1));

            Assert.That(typeof(TestBigGenericClass<int, int, int, int, int, int, int, int, int, int>).GetShallowGenericTypeCount(), Is.EqualTo(10));

            Assert.That(typeof(TestOuterGenericNestedClass<int>.TestInnerClass).GetShallowGenericTypeCount(), Is.EqualTo(0));

            Assert.That(typeof(TestOuterGenericNestedClass<int>.TestInnerGenericClass<float>).GetShallowGenericTypeCount(), Is.EqualTo(1));
        }

        [Test]
        public static void GetReadableNameTest()
        {
            Assert.That(nameof(GetReadableNameTest), Is.EqualTo(nameof(ReadabilityExtension.GetReadableName) + "Test"));

            Assert.That(typeof(int).GetReadableName(), Is.EqualTo("Int32"));

            Assert.That(typeof(int?).GetReadableName(), Is.EqualTo("Int32?"));

            Assert.That(typeof(TestClass).GetReadableName(), Is.EqualTo("ReadabilityExtensionTests.TestClass"));

            Assert.That(typeof(TestGenericClass<int>).GetReadableName(), Is.EqualTo("ReadabilityExtensionTests.TestGenericClass<Int32>"));

            Assert.That(typeof(TestGenericClass<int?>).GetReadableName(), Is.EqualTo("ReadabilityExtensionTests.TestGenericClass<Int32?>"));

            Assert.That(typeof(TestGenericClass<TestGenericClass<TestClass>>).GetReadableName(),
                Is.EqualTo("ReadabilityExtensionTests.TestGenericClass<ReadabilityExtensionTests.TestGenericClass<ReadabilityExtensionTests.TestClass>>"));

            Assert.That(typeof(TestBigGenericClass<int, int, int, int, int, int, int, int, int, int>).GetReadableName(),
                Is.EqualTo("ReadabilityExtensionTests.TestBigGenericClass<Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32>"));

            Assert.That(typeof(TestOuterGenericNestedClass<int>.TestInnerClass).GetReadableName(),
                Is.EqualTo("ReadabilityExtensionTests.TestOuterGenericNestedClass<Int32>.TestInnerClass"));

            Assert.That(typeof(TestOuterGenericNestedClass<int>.TestInnerGenericClass<float>).GetReadableName(),
                Is.EqualTo("ReadabilityExtensionTests.TestOuterGenericNestedClass<Int32>.TestInnerGenericClass<Single>"));

            Assert.That(typeof(TestOuterGenericNestedClass<int?>.TestInnerClass).GetReadableName(),
                Is.EqualTo("ReadabilityExtensionTests.TestOuterGenericNestedClass<Int32?>.TestInnerClass"));

            Assert.That(typeof(TestOuterGenericNestedClass<int?>.TestInnerGenericClass<float?>).GetReadableName(),
                Is.EqualTo("ReadabilityExtensionTests.TestOuterGenericNestedClass<Int32?>.TestInnerGenericClass<Single?>"));

            Assert.That(typeof(TestOuterGenericNestedClass<>.TestInnerGenericClass<>).GetReadableName(),
                Is.EqualTo("ReadabilityExtensionTests.TestOuterGenericNestedClass<>.TestInnerGenericClass<>"));
        }

        [Test]
        public static void GetDigitCountTest()
        {
            Assert.That(nameof(GetDigitCountTest), Is.EqualTo(nameof(ReadabilityExtension.GetDigitCount) + "Test"));

            Assert.That(0.GetDigitCount(), Is.EqualTo(1));
            Assert.That(0.GetDigitCount(true), Is.EqualTo(1));

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
                Assert.That(positiveValue.GetDigitCount(), Is.EqualTo(expectedDigits));
                Assert.That(positiveValue.GetDigitCount(true), Is.EqualTo(expectedDigits));
                Assert.That((-positiveValue).GetDigitCount(), Is.EqualTo(expectedDigits));
                Assert.That((-positiveValue).GetDigitCount(true), Is.EqualTo(expectedDigits+1));
            }
        }
    }
}