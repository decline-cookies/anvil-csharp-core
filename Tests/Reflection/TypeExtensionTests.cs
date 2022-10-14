using NUnit.Framework;
using Anvil.CSharp.Reflection;

namespace Anvil.CSharp.Tests
{
    public static class TypeExtensionTests
    {
        private class TestClass { }
        private class TestGenericClass<T> { }
        private class TestBigGenericClass<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> { }

        [Test]
        public static void GetReadableNameTest()
        {
            Assert.That(typeof(int).GetReadableName(), Is.EqualTo("Int32"));

            Assert.That(typeof(int?).GetReadableName(), Is.EqualTo("Int32?"));

            Assert.That(typeof(TestClass).GetReadableName(), Is.EqualTo("TestClass"));

            Assert.That(typeof(TestGenericClass<int>).GetReadableName, Is.EqualTo("TestGenericClass<Int32>"));

            Assert.That(typeof(TestGenericClass<int?>).GetReadableName, Is.EqualTo("TestGenericClass<Int32?>"));

            Assert.That(typeof(TestGenericClass<TestGenericClass<TestClass>>).GetReadableName(),
                Is.EqualTo("TestGenericClass<TestGenericClass<TestClass>>"));

            Assert.That(typeof(TestBigGenericClass<int, int, int, int, int, int, int, int, int, int>).GetReadableName,
                Is.EqualTo("TestBigGenericClass<Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32>"));
        }
    }
}