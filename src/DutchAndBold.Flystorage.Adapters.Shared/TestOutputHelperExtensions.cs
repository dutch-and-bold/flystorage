using System;
using System.Reflection;
using Xunit.Abstractions;

namespace DutchAndBold.Flystorage.Adapters.Shared
{
    public static class TestOutputHelperExtensions
    {
        public static string GetTestName(this ITestOutputHelper testOutputHelper)
        {
            var type = testOutputHelper.GetType();
            var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
            var test = (ITest)testMember?.GetValue(testOutputHelper) ??
                       throw new NullReferenceException(nameof(testMember));

            return test.DisplayName;
        }
    }
}