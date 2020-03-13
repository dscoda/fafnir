using Fafnir.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fafnir.LogParsers
{
    public class PlayerJoinedTeam : IHLDSLogParser
    {
        private MatchLog _matchLog;
        private static string dateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static Regex playerJoinedTeamPattern = new Regex(@$"L (?<date>{dateTimeRegEx}): ""(?<player>.*?)"" joined team ""(?<team>.*?)""");

        public PlayerJoinedTeam(MatchLog matchLog)
        {
            _matchLog = matchLog;
        }

        public void Execute(string input)
        {
            var matched = playerJoinedTeamPattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var playerData = matched.Groups["player"].Value;
            var teamName = matched.Groups["team"].Value;

            var currentPlayer = _matchLog.GetPlayer(playerData);
            var time = _matchLog.GetEntryTime(dateTime);
            var team = currentPlayer.Teams.SingleOrDefault(s => s.Name == teamName);

            var openTeamEntries = (from t in currentPlayer.Teams
                                   where t.LeaveTime == null && t.Name != teamName
                                   select t);

            foreach (var openTeam in openTeamEntries)
            {
                openTeam.LeaveTime = time;

                openTeam.SecondsPlayed += ((DateTime)openTeam.LeaveTime - (DateTime)openTeam.JoinTime).TotalSeconds;
            }

            var openRoles = (from r in currentPlayer.Roles
                             where r.EndTime == null
                             select r);

            foreach (var openRole in openRoles)
            {
                openRole.EndTime = time;
                openRole.SecondsPlayed += ((DateTime)openRole.EndTime - openRole.StartTime).TotalSeconds;
            }

            if (team != null)
            {
                if (team.LeaveTime != null)
                {
                    team.LeaveTime = null;
                    team.JoinTime = time;
                }
            }
            else
            {
                currentPlayer.Teams.Add(new Player.Team
                {
                    Name = teamName,
                    JoinTime = time,
                    LeaveTime = null,
                    SecondsPlayed = 0,
                });
            }
        }

        public bool IsMatch(string input)
        {
            return playerJoinedTeamPattern.IsMatch(input);
        }
    }
}
