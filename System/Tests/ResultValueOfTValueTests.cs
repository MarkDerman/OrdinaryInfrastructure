using System.Text.Json;
using NUnit.Framework;
using Odin.System;

namespace Tests.Odin.System
{
    [TestFixture]
    public sealed class ResultValueOfTValueTests
    {
        [Test]
        public void Succeed_with_object_value()
        {
            object obj = new CategoryAttribute("123");
            ResultValue<object> sut = ResultValue<object>.Success(obj);
            
            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.MessagesToString(), Is.Empty.Or.Null);
            Assert.That(sut.Messages, Is.Empty);
            Assert.That(obj, Is.EqualTo(sut.Value));
        }
        
        [Test]
        public void Succeed_with_string_value_and_no_message()
        {
            string stringVal = "123";
            ResultValue<string> sut = ResultValue<string>.Success(stringVal);
            
            Assert.That(sut.IsSuccess, Is.True);
            Assert.That(sut.MessagesToString(), Is.Empty.Or.Null);
            Assert.That(sut.Messages, Is.Empty);
            Assert.That(stringVal,Is.EqualTo(sut.Value));
            Assert.That(stringVal, Is.EqualTo(sut.Value));      
        }
        
        [Test]
        public void Succeed_with_string_value_and_message()
        {
            string stringVal = "123";
            ResultValue<string> sut = ResultValue<string>.Success(stringVal, "message");
            
            Assert.That(sut.IsSuccess, Is.True);      
            Assert.That(sut.MessagesToString(), Is.EqualTo("message"));
            Assert.That(1, Is.EqualTo(sut.Messages.Count));
            Assert.That(stringVal, Is.EqualTo(sut.Value));       

        }
        
        [Test]
        public void Succeed_with_struct_value_and_message()
        {
            double num = 13.8;
            ResultValue<double> sut = ResultValue<double>.Success(num, "message");
            
            Assert.That(sut.IsSuccess, Is.True);       
            Assert.That(sut.MessagesToString(), Is.EqualTo("message"));
            Assert.That(1, Is.EqualTo(sut.Messages.Count));
            Assert.That(num, Is.EqualTo(sut.Value));        
        }
        
                
        [Test]
        public void Result_of_T_serialises_with_system_dot_text_dot_json()
        {
            ResultValue<int> sut = ResultValue<int>.Success(3, "cool man");
            
            string result = JsonSerializer.Serialize(sut);

            Assert.That(result, Is.EqualTo("{\"IsSuccess\":true,\"Value\":3,\"Messages\":[\"cool man\"]}"));
        }
        
        [Test]
        public void Result_of_T_deserialises_with_system_dot_text_dot_json()
        {
            string serialised = "{\"Value\":3,\"IsSuccess\":true,\"Messages\":[\"cool man\"]}";
            
            ResultValue<int>? result = JsonSerializer.Deserialize<ResultValue<int>>(serialised);
            
            Assert.That(result, Is.Not.Null);    
            Assert.That(result!.IsSuccess, Is.True);       
            Assert.That(result.Value, Is.EqualTo(3));      
            Assert.That(result.Messages[0], Is.EqualTo("cool man"));
        }
        
        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("test")]
        public void String_as_TValue_success(string testValue)
        {
            ResultValue<string> success = ResultValue<string>.Success(testValue);
            
            Assert.That(success.IsSuccess, Is.True);
            Assert.That(success.Messages, Is.Empty);
            Assert.That(success.Value, Is.EqualTo(testValue));
        }
        
        [Test]
        public void Success_requires_a_value()
        {
            Assert.Throws<ArgumentException>(() => ResultValue<string>.Success((null as string)!));
        }
        
        
        
        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        [TestCase("test")]
        public void String_as_TValue_fail(string testValue)
        {
            ResultValue<string> fail = ResultValue<string>.Failure("message", testValue);
            
            Assert.That(fail.IsSuccess, Is.False);
            Assert.That(fail.Messages.Count, Is.EqualTo(1));
            Assert.That(fail.Value, Is.EqualTo(testValue));
        }
    }
}