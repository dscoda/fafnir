using System.Text.RegularExpressions;
using Match = Fafnir.Models.Match;

namespace Fafnir.LogParsers
{
    public class PlayerEnteredGame : IHLDSLogParser
    {
        private readonly Match _match;
        private const string DateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static readonly Regex PlayerEnteredGamePattern = new Regex(@$"L (?<date>{DateTimeRegEx}): ""(?<player>.*?)"" entered the game");

        public PlayerEnteredGame(Match match)
        {
            _match = match;
        }

        public void Execute(string input)
        {
            var matched = PlayerEnteredGamePattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var playerData = matched.Groups["player"].Value;

            var joinedTime = _match.GetEntryTime(dateTime);
            var currentPlayer = _match.GetPlayer(playerData);

            currentPlayer.JoinTime = joinedTime;
            currentPlayer.LeaveTime = null;
        }

        public bool IsMatch(string input)
        {
            return PlayerEnteredGamePattern.IsMatch(input);
        }
    }
}
