using NUnit.Framework;
using Odin.System;

namespace Tests.Odin.System
{
    [TestFixture]
    public sealed class Activator2Tests
    {
        [Test]
        public void Create_class_by_type()
        {
            ResultValue<Class3> result = Activator2.TryCreate<Class3>(typeof(Class3));

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<Class3>());
        }
        
        [Test]
        public void Create_inherited_class_by_interface_type()
        {
            ResultValue<Interface1> result = Activator2.TryCreate<Interface1>(typeof(Inherited2));

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<Interface1>());
        }
        
        [Test]
        public void Create_class_by_typename()
        {
            ResultValue<Class3> result = Activator2.TryCreate<Class3>("Tests.Odin.System.Class3","Tests.Odin.System");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True, result.MessagesToString());
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<Class3>());
        }
        
        [Test]
        public void Create_inherited_class_by_typename()
        {
            ResultValue<Interface1> result = Activator2.TryCreate<Interface1>("Tests.Odin.System.Inherited2","Tests.Odin.System");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<Interface1>());
        }
    }
}