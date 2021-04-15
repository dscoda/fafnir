using System.Text.RegularExpressions;
using Match = Fafnir.Models.Match;

namespace Fafnir.LogParsers
{
    public class TeamEndingScore : IHLDSLogParser
    {
        private readonly Match _match;
        private const string DateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static readonly Regex TeamScorePattern = new Regex(@$"L (?<date>{DateTimeRegEx}): Team ""(?<team>.*?)"" scored ""(?<score>.*?)"" with ""(?<players>.*?)"" player.");

        public TeamEndingScore(Match match)
        {
            _match = match;
        }

        public bool IsMatch(string input)
        {
            return TeamScorePattern.IsMatch(input);
        }

        public void Execute(string input)
        {
            _match.MatchEnded = true;
        }
    }
}
