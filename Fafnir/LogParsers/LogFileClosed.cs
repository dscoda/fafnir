using Fafnir.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fafnir.LogParsers
{
    public class LogFileClosed : IHLDSLogParser
    {
        private MatchLog _matchLog;
        private static string dateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static Regex logFileClosedPattern = new Regex(@$"L (?<date>{dateTimeRegEx}): Log file closed");

        public LogFileClosed(MatchLog matchLog)
        {
            _matchLog = matchLog;
        }

        public void Execute(string input)
        {
            var matched = logFileClosedPattern.Match(input);

            var dateTime = matched.Groups["date"].Value;

            _matchLog.matchEndTime = _matchLog.GetEntryTime(dateTime);

            foreach (var player in _matchLog.players)
            {
                if (player.LeaveTime == null && player.JoinTime != null)
                {
                    player.LeaveTime = _matchLog.matchEndTime;
                    player.SecondsPlayed += ((DateTime)player.LeaveTime - (DateTime)player.JoinTime).TotalSeconds;

                    foreach (var team in player.Teams.Where(w => w.LeaveTime == null))
                    {
                        team.LeaveTime = _matchLog.matchEndTime;
                        team.SecondsPlayed += ((DateTime)team.LeaveTime - (DateTime)team.JoinTime).TotalSeconds;
                    }
                }

                var openRoles = (from r in player.Roles where r.EndTime == null select r);

                foreach (var openRole in openRoles)
                {
                    openRole.EndTime = _matchLog.matchEndTime;
                    openRole.SecondsPlayed += ((DateTime)openRole.EndTime - openRole.StartTime).TotalSeconds;
                }
            }
        }

        public bool IsMatch(string input)
        {
            return logFileClosedPattern.IsMatch(input);
        }
    }
}
