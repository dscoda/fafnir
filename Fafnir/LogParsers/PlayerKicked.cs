using System;
using System.Linq;
using System.Text.RegularExpressions;
using Fafnir.Models;
using Match = Fafnir.Models.Match;

namespace Fafnir.LogParsers
{
    public class PlayerKicked : IHLDSLogParser
    {
        private readonly Match _match;
        private const string DateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static readonly Regex PlayerKickedPattern = new Regex(@$"L (?<date>{DateTimeRegEx}): Kick: ""(?<player>.*?)"" was kicked by ""(?<kicker>.*?)");

        public PlayerKicked(Match match)
        {
            _match = match;
        }

        public bool IsMatch(string input)
        {
            return PlayerKickedPattern.IsMatch(input);
        }

        public void Execute(string input)
        {
            if (_match.MatchEnded)
            {
                return;
            }

            var matched = PlayerKickedPattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var playerData = matched.Groups["player"].Value;

            Player currentPlayer = _match.GetPlayer(playerData);
            DateTime leaveTime = _match.GetEntryTime(dateTime);

            currentPlayer.LeaveTime = leaveTime;

            if (currentPlayer.LeaveTime != null && currentPlayer.JoinTime != null)
            {
                currentPlayer.SecondsPlayed += ((DateTime)currentPlayer.LeaveTime - (DateTime)currentPlayer.JoinTime).TotalSeconds;
            }

            foreach (var team in currentPlayer.Teams.Where(w => w.LeaveTime == null))
            {
                team.LeaveTime = leaveTime;
                team.SecondsPlayed += ((DateTime)team.LeaveTime - (DateTime)team.JoinTime).TotalSeconds;

            }

            var openRoles = (from r in currentPlayer.Roles
                where r.EndTime == null
                select r);

            foreach (var openRole in openRoles)
            {
                openRole.EndTime = leaveTime;
                openRole.SecondsPlayed += ((DateTime)openRole.EndTime - openRole.StartTime).TotalSeconds;
            }

            currentPlayer.TimesKicked += 1;
        }
    }
}
