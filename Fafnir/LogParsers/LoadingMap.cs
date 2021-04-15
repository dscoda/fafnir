using System;
using Fafnir.Models;
using System.Text.RegularExpressions;

namespace Fafnir.LogParsers
{
    public class LoadingMap : IHLDSLogParser
    {
        private readonly MatchLog _matchLog;
        private static string dateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static readonly Regex LoadingMapPattern = new Regex(@$"L (?<date>{dateTimeRegEx}): Loading map ""(?<map>.*?)""");

        public LoadingMap(MatchLog matchLog)
        {
            _matchLog = matchLog;
        }

        public void Execute(string input)
        {
            var matched = LoadingMapPattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var map = matched.Groups["map"].Value;

            if (!string.IsNullOrEmpty(_matchLog.Map))
            {
                throw new Exception("Log file read out of order or log file is missing.");
            }

            _matchLog.Map = map;
            _matchLog.MatchStartTime = _matchLog.GetEntryTime(dateTime);
        }

        public bool IsMatch(string input)
        {
            return LoadingMapPattern.IsMatch(input);
        }
    }
}
