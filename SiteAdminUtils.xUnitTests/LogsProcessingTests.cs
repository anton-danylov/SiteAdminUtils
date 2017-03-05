using SiteAdminUtils.Core;
using SiteAdminUtils.ViewModel;
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
    public class LogsProcessingTests
    {

        [Fact]
        public void TestDownloadFile()
        {
            AnalyzeAccessLogVM vm = new AnalyzeAccessLogVM();

            vm.DownloadLogsFromServer();
        }
    }
}
