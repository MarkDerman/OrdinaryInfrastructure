using Odin.System;

namespace Tests.Odin.System
{
    public sealed class Activator2Tests
    {
        [Fact]
        public void Create_class_by_type()
        {
            ResultValue<Class3> result = Activator2.TryCreate<Class3>(typeof(Class3));

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.IsType<Class3>(result.Value);
        }
        
        [Fact]
        public void Create_inherited_class_by_interface_type()
        {
            ResultValue<Interface1> result = Activator2.TryCreate<Interface1>(typeof(Inherited2));

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.IsAssignableFrom<Interface1>(result.Value);
        }
        
        [Fact]
        public void Create_class_by_typename()
        {
            ResultValue<Class3> result = Activator2.TryCreate<Class3>("Tests.Odin.System.Class3","Tests.Odin.System");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess, result.MessagesToString());
            Assert.NotNull(result.Value);
            Assert.IsType<Class3>(result.Value);
        }
        
        [Fact]
        public void Create_inherited_class_by_typename()
        {
            ResultValue<Interface1> result = Activator2.TryCreate<Interface1>("Tests.Odin.System.Inherited2","Tests.Odin.System");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.IsAssignableFrom<Interface1>(result.Value);
        }
    }
}
