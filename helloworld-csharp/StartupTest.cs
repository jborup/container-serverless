using Xunit;

namespace helloworld_csharp
{


    public class StartupTest
    {
        [Fact]
        public void PassingGetTargetTest()
        {
            Assert.Equal("DEFAULT", Startup.getTarget(null, "DEFAULT"));
            Assert.NotEqual("DEFAULT", Startup.getTarget("Non-default-value", "DEFAULT"));
        }


        [Fact]
        public void FailingGetTargetTest()
        {
            Assert.NotEqual("DEFAULT", Startup.getTarget("NOT DEFAULT", "DEFAULT"));
        }

        [Theory]
        [InlineData(null, "DEFAULT", "DEFAULT")]
        [InlineData("", "DEFAULT", "DEFAULT")]
        [InlineData("UNIQUE", "DEFAULT", "UNIQUE")]
        public void PassingGetTargetTheory(string value, string defaultValue, string result)
        {
            Assert.Equal(result, Startup.getTarget(value, defaultValue));
        }
    }

}
