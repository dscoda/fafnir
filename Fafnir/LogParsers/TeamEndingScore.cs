using System.Text.RegularExpressions;
using Fafnir.Models;

namespace Fafnir.LogParsers
{
    public class TeamEndingScore : IHLDSLogParser
    {
        private readonly MatchLog _matchLog;
        private static string dateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static readonly Regex TeamScorePattern = new Regex(@$"L (?<date>{dateTimeRegEx}): Team ""(?<team>.*?)"" scored ""(?<score>.*?)"" with ""(?<players>.*?)"" player.");

        public TeamEndingScore(MatchLog matchLog)
        {
            _matchLog = matchLog;
        }

        public bool IsMatch(string input)
        {
            return TeamScorePattern.IsMatch(input);
        }

        public void Execute(string input)
        {
            _matchLog.MatchEnded = true;
        }
    }
}
