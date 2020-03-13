using Fafnir.Models;
using System.Text.RegularExpressions;

namespace Fafnir.LogParsers
{
    class PlayerEnteredGame : IHLDSLogParser
    {
        private MatchLog _matchLog;
        private static string dateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static Regex playerEnteredGamePattern = new Regex(@$"L (?<date>{dateTimeRegEx}): ""(?<player>.*?)"" entered the game");

        public PlayerEnteredGame(MatchLog matchLog)
        {
            _matchLog = matchLog;
        }

        public void Execute(string input)
        {
            var matched = playerEnteredGamePattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var playerData = matched.Groups["player"].Value;

            var joinedTime = _matchLog.GetEntryTime(dateTime);
            var currentPlayer = _matchLog.GetPlayer(playerData);

            currentPlayer.JoinTime = joinedTime;
            currentPlayer.LeaveTime = null;
        }

        public bool IsMatch(string input)
        {
            return playerEnteredGamePattern.IsMatch(input);
        }
    }
}
