using System;
using System.Text.RegularExpressions;
using Match = Fafnir.Models.Match;

namespace Fafnir.LogParsers
{
    public class LoadingMap : IHLDSLogParser
    {
        private readonly Match _match;
        private const string DateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static readonly Regex LoadingMapPattern = new Regex(@$"L (?<date>{DateTimeRegEx}): Loading map ""(?<map>.*?)""");

        public LoadingMap(Match match)
        {
            _match = match;
        }

        public void Execute(string input)
        {
            var matched = LoadingMapPattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var map = matched.Groups["map"].Value;

            if (!string.IsNullOrEmpty(_match.Map))
            {
                throw new Exception("Log file read out of order or log file is missing.");
            }

            _match.Map = map;
            _match.MatchStartTime = _match.GetEntryTime(dateTime);
        }

        public bool IsMatch(string input)
        {
            return LoadingMapPattern.IsMatch(input);
        }
    }
}
