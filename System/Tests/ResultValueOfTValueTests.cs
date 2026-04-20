using System.Text.Json;
using Odin.System;

namespace Tests.Odin.System
{
    public sealed class ResultValueOfTValueTests
    {
        [Fact]
        public void Succeed_with_object_value()
        {
            object obj = new object();
            ResultValue<object> sut = ResultValue<object>.Success(obj);
            
            Assert.True(sut.IsSuccess);
            Assert.True(string.IsNullOrEmpty(sut.MessagesToString()));
            Assert.Empty(sut.Messages);
            Assert.Same(obj, sut.Value);
        }
        
        [Fact]
        public void Succeed_with_string_value_and_no_message()
        {
            string stringVal = "123";
            ResultValue<string> sut = ResultValue<string>.Success(stringVal);
            
            Assert.True(sut.IsSuccess);
            Assert.True(string.IsNullOrEmpty(sut.MessagesToString()));
            Assert.Empty(sut.Messages);
            Assert.Equal(stringVal, sut.Value);
            Assert.Equal(stringVal, sut.Value);
        }
        
        [Fact]
        public void Succeed_with_string_value_and_message()
        {
            string stringVal = "123";
            ResultValue<string> sut = ResultValue<string>.Success(stringVal, "message");
            
            Assert.True(sut.IsSuccess);
            Assert.Equal("message", sut.MessagesToString());
            Assert.Equal(1, sut.Messages.Count);
            Assert.Equal(stringVal, sut.Value);

        }
        
        [Fact]
        public void Succeed_with_struct_value_and_message()
        {
            double num = 13.8;
            ResultValue<double> sut = ResultValue<double>.Success(num, "message");
            
            Assert.True(sut.IsSuccess);
            Assert.Equal("message", sut.MessagesToString());
            Assert.Equal(1, sut.Messages.Count);
            Assert.Equal(num, sut.Value);
        }
        
                
        [Fact]
        public void Result_of_T_serialises_with_system_dot_text_dot_json()
        {
            ResultValue<int> sut = ResultValue<int>.Success(3, "cool man");
            
            string result = JsonSerializer.Serialize(sut);

            Assert.Equal("{\"IsSuccess\":true,\"Value\":3,\"Messages\":[\"cool man\"]}", result);
        }
        
        [Fact]
        public void Result_of_T_deserialises_with_system_dot_text_dot_json()
        {
            string serialised = "{\"Value\":3,\"IsSuccess\":true,\"Messages\":[\"cool man\"]}";
            
            ResultValue<int>? result = JsonSerializer.Deserialize<ResultValue<int>>(serialised);
            
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);
            Assert.Equal(3, result.Value);
            Assert.Equal("cool man", result.Messages[0]);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("test")]
        public void String_as_TValue_success(string testValue)
        {
            ResultValue<string> success = ResultValue<string>.Success(testValue);
            
            Assert.True(success.IsSuccess);
            Assert.Empty(success.Messages);
            Assert.Equal(testValue, success.Value);
        }
        
        [Fact]
        public void Success_requires_a_value()
        {
            Assert.Throws<ArgumentException>(() => ResultValue<string>.Success((null as string)!));
        }
        
        
        
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("test")]
        public void String_as_TValue_fail(string? testValue)
        {
            ResultValue<string> fail = ResultValue<string>.Failure("message", testValue);
            
            Assert.False(fail.IsSuccess);
            Assert.Equal(1, fail.Messages.Count);
            Assert.Equal(testValue, fail.Value);
        }
    }
}
