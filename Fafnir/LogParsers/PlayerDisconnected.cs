using Fafnir.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fafnir.LogParsers
{
    public class PlayerDisconnected : IHLDSLogParser
    {
        private MatchLog _matchLog;
        private static string dateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static Regex playerDisconnectedPattern = new Regex(@$"L (?<date>{dateTimeRegEx}): ""(?<player>.*?)"" disconnected");

        public PlayerDisconnected(MatchLog matchLog)
        {
            _matchLog = matchLog;
        }

        public void Execute(string input)
        {
            var matched = playerDisconnectedPattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var playerData = matched.Groups["player"].Value;

            Player currentPlayer = _matchLog.GetPlayer(playerData);
            DateTime leaveTime = _matchLog.GetEntryTime(dateTime);

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
            return playerDisconnectedPattern.IsMatch(input);
        }
    }
}
