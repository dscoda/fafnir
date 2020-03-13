using Fafnir.Models;
using System.Text.RegularExpressions;

namespace Fafnir.LogParsers
{
    public class LoadingMap : IHLDSLogParser
    {
        private MatchLog _matchLog;
        private static string dateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static Regex loadingMapPattern = new Regex(@$"L (?<date>{dateTimeRegEx}): Loading map ""(?<map>.*?)""");

        public LoadingMap(MatchLog matchLog)
        {
            _matchLog = matchLog;
        }

        public void Execute(string input)
        {
            var matched = loadingMapPattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var map = matched.Groups["map"].Value;

            _matchLog.map = map;
            _matchLog.matchStartTime = _matchLog.GetEntryTime(dateTime);
        }

        public bool IsMatch(string input)
        {
            return loadingMapPattern.IsMatch(input);
        }
    }
}
