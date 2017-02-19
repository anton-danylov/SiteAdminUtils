using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SiteAdminUtils.Core
{
    public struct ApacheLogEntry
    {
        public string Ip;
        public DateTimeOffset DateOffset;
        public string Request;
        public int Response;
        public int BytesSent;
        public string Referer;
        public String UserAgent;

        public ApacheLogEntry(string ip, string date, string request, int response, int bytesSent, string referer, string userAgent)
        {
            Ip = ip;
            DateOffset = GetDateTimeOffsetFromApacheDate(date);
            Request = request;
            Response = response;
            BytesSent = bytesSent;
            Referer = referer;
            UserAgent = userAgent;
        }

        public static DateTimeOffset GetDateTimeOffsetFromApacheDate(string apacheDate)
        {
            const string parsePattern = "dd/MMM/yyyy:HH:mm:ss zzz";
            return DateTimeOffset.ParseExact(apacheDate, parsePattern, CultureInfo.InvariantCulture);
        }
    }

    public class ApacheLogAnalyser
    {
        const string LogEntryPattern = "^([\\d.]+|::1) (\\S+) (\\S+) \\[([\\w:/]+\\s[+\\-]\\d{4})\\] \"(.+?)\" (\\d{3}) (\\d+) \"([^\"]+)\" \"([^\"]+)\"";
        static readonly Regex _logEntryRegex = new Regex(LogEntryPattern, RegexOptions.Compiled);

        ConcurrentBag<ApacheLogEntry> _parsedLogEntries;

        IEnumerable<ApacheLogEntry> ParsedLogEntries => _parsedLogEntries;


        public static ApacheLogEntry GetLogEntryFromString(string logEntryLine)
        {
            var match = _logEntryRegex.Match(logEntryLine);

            return new ApacheLogEntry(
                match.Groups[1].Value
                , match.Groups[4].Value
                , match.Groups[5].Value
                , Convert.ToInt32(match.Groups[6].Value)
                , Convert.ToInt32(match.Groups[7].Value)
                , match.Groups[8].Value
                , match.Groups[9].Value
                );
        }


        internal void ProcessLine(string logEntryLine)
        {
            _parsedLogEntries.Add(GetLogEntryFromString(logEntryLine));
        }



        public void ProcessFile(string filePath, int? limitLinesNumber = null)
        {
            var allLines = File.ReadAllLines(filePath);
            int linesToProcess = Math.Min(limitLinesNumber ?? allLines.Length, allLines.Length);

            _parsedLogEntries = new ConcurrentBag<ApacheLogEntry>();

            Parallel.For(0, linesToProcess, i => ProcessLine(allLines[i]));
        }

    }
}
