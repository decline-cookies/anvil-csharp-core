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
        public static void CreateDefaultValue()
        {
            Assert.That(typeof(TestClass).CreateDefaultValue(), Is.EqualTo(null));
            Assert.That(typeof(TestSealedClass).CreateDefaultValue(), Is.EqualTo(null));
            Assert.That(typeof(TestStaticClass).CreateDefaultValue(), Is.EqualTo(null));
            Assert.That(typeof(TestAbstractClass).CreateDefaultValue(), Is.EqualTo(null));
            Assert.That(typeof(TestStruct).CreateDefaultValue(), Is.EqualTo(default(TestStruct)));
        }

        [Test]
        public static void IsStatic()
        {
            Assert.That(typeof(TestClass).isStatic(), Is.EqualTo(false));
            Assert.That(typeof(TestSealedClass).isStatic(), Is.EqualTo(false));
            Assert.That(typeof(TestStaticClass).isStatic(), Is.EqualTo(true));
            Assert.That(typeof(TestAbstractClass).isStatic(), Is.EqualTo(false));
            Assert.That(typeof(TestStruct).isStatic(), Is.EqualTo(false));
        }


    }
}