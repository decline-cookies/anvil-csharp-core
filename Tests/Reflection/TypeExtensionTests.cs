using NUnit.Framework;
using Anvil.CSharp.Reflection;

namespace Anvil.CSharp.Tests
{
    public static class TypeExtensionTests
    {
        private class TestClass { }
        private sealed class TestSealedClass { }
        private static class TestStaticClass { }
        private abstract class TestAbstractClass { }
        private struct TestStruct { }


        [Test]
        public static void CreateDefaultValueTest()
        {
            Assert.That(nameof(CreateDefaultValueTest), Is.EqualTo(nameof(TypeExtension.CreateDefaultValue) + "Test"));

            Assert.That(typeof(TestClass).CreateDefaultValue(), Is.EqualTo(null));
            Assert.That(typeof(TestSealedClass).CreateDefaultValue(), Is.EqualTo(null));
            Assert.That(typeof(TestStaticClass).CreateDefaultValue(), Is.EqualTo(null));
            Assert.That(typeof(TestAbstractClass).CreateDefaultValue(), Is.EqualTo(null));
            Assert.That(typeof(TestStruct).CreateDefaultValue(), Is.EqualTo(default(TestStruct)));
        }

        [Test]
        public static void IsStaticTest()
        {
            Assert.That(nameof(IsStaticTest), Is.EqualTo(nameof(TypeExtension.IsStatic) + "Test"));

            Assert.That(typeof(TestClass).IsStatic(), Is.EqualTo(false));
            Assert.That(typeof(TestSealedClass).IsStatic(), Is.EqualTo(false));
            Assert.That(typeof(TestStaticClass).IsStatic(), Is.EqualTo(true));
            Assert.That(typeof(TestAbstractClass).IsStatic(), Is.EqualTo(false));
            Assert.That(typeof(TestStruct).IsStatic(), Is.EqualTo(false));
        }


    }
}