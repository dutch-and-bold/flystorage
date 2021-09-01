using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace DutchAndBold.Flystorage.Adapters.Tests
{
    public static class TestOutputHelperExtensions
    {
        public static string GetTestName(this ITestOutputHelper testOutputHelper)
        {
            return GetTestName(testOutputHelper, t => t.DisplayName);
        }

        private static string GetTestName(ITestOutputHelper testOutputHelper, Func<ITest, string> select)
        {
            var type = testOutputHelper.GetType();
            var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
            var test = (ITest)testMember?.GetValue(testOutputHelper) ??
                       throw new NullReferenceException(nameof(testMember));

            return Regex.Replace(select(test), "[^a-zA-Z0-9]+", "-").Trim('-');
        }
    }
}