using Fafnir.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Match = Fafnir.Models.Match;

namespace Fafnir.LogParsers
{
    public class PlayerDisconnected : IHLDSLogParser
    {
        private readonly Match _match;
        private const string DateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static readonly Regex PlayerDisconnectedPattern = new Regex(@$"L (?<date>{DateTimeRegEx}): ""(?<player>.*?)"" disconnected");

        public PlayerDisconnected(Match match)
        {
            _match = match;
        }

        public void Execute(string input)
        {
            if (_match.MatchEnded)
            {
                return;
            }

            var matched = PlayerDisconnectedPattern.Match(input);

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
        }

        public bool IsMatch(string input)
        {
            return PlayerDisconnectedPattern.IsMatch(input);
        }
    }
}
