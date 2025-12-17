using NUnit.Framework;
using Odin.System;
using Odin.Utility;

namespace Tests.Odin.Utility
{
    [TestFixture]
    public sealed class ClassFactoryTests
    {
        [Test]
        public void Create_class_by_type()
        {
            ClassFactory activator = new ClassFactory();
            ResultValue<Class3> result = activator.TryCreate<Class3>(typeof(Class3));

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<Class3>());
        }
        
        [Test]
        public void Create_inherited_class_by_interface_type()
        {
            ClassFactory activator = new ClassFactory();
            ResultValue<Interface1> result = activator.TryCreate<Interface1>(typeof(Inherited2));

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<Interface1>());
        }
        
        [Test]
        public void Create_class_by_typename()
        {
            ClassFactory activator = new ClassFactory();
            ResultValue<Class3> result = activator.TryCreate<Class3>("Tests.Odin.Utility.Class3");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True, result.MessagesToString());
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<Class3>());
        }
        
        [Test]
        public void Create_inherited_class_by_typename()
        {
            ClassFactory activator = new ClassFactory();
            ResultValue<Interface1> result = activator.TryCreate<Interface1>("Tests.Odin.Utility.Inherited2");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<Interface1>());
        }
    }
}