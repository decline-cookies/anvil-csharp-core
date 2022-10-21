using NUnit.Framework;
using Anvil.CSharp.Reflection;

namespace Anvil.CSharp.Tests
{
    public static class TypeExtensionTests
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
        public static void GetShallowGenericTypeCount()
        {
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
            Assert.That(typeof(int).GetReadableName(), Is.EqualTo("Int32"));

            Assert.That(typeof(int?).GetReadableName(), Is.EqualTo("Int32?"));

            Assert.That(typeof(TestClass).GetReadableName(), Is.EqualTo("TypeExtensionTests.TestClass"));

            Assert.That(typeof(TestGenericClass<int>).GetReadableName(), Is.EqualTo("TypeExtensionTests.TestGenericClass<Int32>"));

            Assert.That(typeof(TestGenericClass<int?>).GetReadableName(), Is.EqualTo("TypeExtensionTests.TestGenericClass<Int32?>"));

            Assert.That(typeof(TestGenericClass<TestGenericClass<TestClass>>).GetReadableName(),
                Is.EqualTo("TypeExtensionTests.TestGenericClass<TypeExtensionTests.TestGenericClass<TypeExtensionTests.TestClass>>"));

            Assert.That(typeof(TestBigGenericClass<int, int, int, int, int, int, int, int, int, int>).GetReadableName(),
                Is.EqualTo("TypeExtensionTests.TestBigGenericClass<Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32>"));

            Assert.That(typeof(TestOuterGenericNestedClass<int>.TestInnerClass).GetReadableName(),
                Is.EqualTo("TypeExtensionTests.TestOuterGenericNestedClass<Int32>.TestInnerClass"));

            Assert.That(typeof(TestOuterGenericNestedClass<int>.TestInnerGenericClass<float>).GetReadableName(),
                Is.EqualTo("TypeExtensionTests.TestOuterGenericNestedClass<Int32>.TestInnerGenericClass<Single>"));

            Assert.That(typeof(TestOuterGenericNestedClass<int?>.TestInnerClass).GetReadableName(),
                Is.EqualTo("TypeExtensionTests.TestOuterGenericNestedClass<Int32?>.TestInnerClass"));

            Assert.That(typeof(TestOuterGenericNestedClass<int?>.TestInnerGenericClass<float?>).GetReadableName(),
                Is.EqualTo("TypeExtensionTests.TestOuterGenericNestedClass<Int32?>.TestInnerGenericClass<Single?>"));

            Assert.That(typeof(TestOuterGenericNestedClass<>.TestInnerGenericClass<>).GetReadableName(),
                Is.EqualTo("TypeExtensionTests.TestOuterGenericNestedClass<>.TestInnerGenericClass<>"));
        }


    }
}