using System;
using System.Linq;
using System.Text.RegularExpressions;
using Match = Fafnir.Models.Match;

namespace Fafnir.LogParsers
{
    public class LogFileClosed : IHLDSLogParser
    {
        private readonly Match _match;
        private const string DateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static readonly Regex LogFileClosedPattern = new Regex(@$"L (?<date>{DateTimeRegEx}): Log file closed");

        public LogFileClosed(Match match)
        {
            _match = match;
        }

        public void Execute(string input)
        {
            var matched = LogFileClosedPattern.Match(input);

            var dateTime = matched.Groups["date"].Value;

            if (!_match.MatchEnded)
            {
                return;
            }

            _match.MatchEndTime = _match.GetEntryTime(dateTime);

            foreach (var player in _match.Players)
            {
                if (player.LeaveTime == null && player.JoinTime != null)
                {
                    player.LeaveTime = _match.MatchEndTime;
                    player.SecondsPlayed += ((DateTime)player.LeaveTime - (DateTime)player.JoinTime).TotalSeconds;

                    foreach (var team in player.Teams.Where(w => w.LeaveTime == null))
                    {
                        team.LeaveTime = _match.MatchEndTime;
                        team.SecondsPlayed += ((DateTime)team.LeaveTime - (DateTime)team.JoinTime).TotalSeconds;
                    }
                }

                var openRoles = (from r in player.Roles where r.EndTime == null select r);

                foreach (var openRole in openRoles)
                {
                    openRole.EndTime = _match.MatchEndTime;
                    openRole.SecondsPlayed += ((DateTime)openRole.EndTime - openRole.StartTime).TotalSeconds;
                }
            }
        }

        public bool IsMatch(string input)
        {
            return LogFileClosedPattern.IsMatch(input);
        }
    }
}
