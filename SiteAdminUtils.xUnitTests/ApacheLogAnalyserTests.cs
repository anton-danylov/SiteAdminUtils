using SiteAdminUtils.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SiteAdminUtils.xUnitTests
{
    public class ApacheLogAnalyserTests
    {
        [Theory]
        [InlineData("100.11.222.999 - - [18/Feb/2017:15:36:04 -0500] \"GET /request\" 200 1756 \"http://url.url\" \"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/0000 (KHTML, like Gecko) Chrome/0.0.000.00\"")]
        [InlineData("::1 - - [13/Feb/2017:06:48:04 -0500] \"OPTIONS * HTTP/1.0\" 200 111 \"-\" \"Apache/3.3.3 (Ubuntu) PHP/0.0.0-0ubuntu0.22 (internal dummy connection)\"")]
        [InlineData("212.193.117.227 - - [13/Feb/2017:20:42:17 -0500] \"GET / HTTP/1.1\" 200 562 \"\" \"Mozilla/5.0 (compatible; statdom.ru/Bot; +http://statdom.ru/bot.html)\"")]
        public void TestGetLogEntryFromString(string str)
        {
            var entry = ApacheLogAnalyser.GetLogEntryFromString(str);

            Assert.Equal(entry.Response, 200);
        }

       
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
