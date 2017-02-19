using SiteAdminUtils.Core;
using System;
using System.Linq;
using Xunit;

namespace SiteAdminUtils.xUnitTests
{
    public class ApacheLogAnalyserTests
    {

        [Theory]
        [InlineData("13/Feb/2017:06:48:04 -0500", 2017, 2, 13, 6, 48, 4, -5.0)]
        [InlineData("13/Feb/2017:06:48:54 -0500", 2017, 2, 13, 6, 48, 54, -5.0)]
        [InlineData("18/Feb/2017:15:36:07 -0500", 2017, 2, 18, 15, 36, 7, -5.0)]
        public void TestDateTimeConversion(string sampleDateTime, int year, int month, int day, int hour, int minute, int second, double offset)
        {
            Assert.Equal(ApacheLogEntry.GetDateTimeOffsetFromApacheDate(sampleDateTime)
                , new DateTimeOffset(year, month, day, hour, minute, second, 0, TimeSpan.FromHours(offset)));
        }
    }
}
