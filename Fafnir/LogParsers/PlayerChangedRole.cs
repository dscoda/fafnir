using Fafnir.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Match = Fafnir.Models.Match;

namespace Fafnir.LogParsers
{
    public class PlayerChangedRole : IHLDSLogParser
    {
        private readonly Match _match;
        private const string DateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static readonly Regex PlayerChangedRolePattern = new Regex(@$"L (?<date>{DateTimeRegEx}): ""(?<player>.*?)"" changed role to ""(?<newrole>.*?)""");

        public PlayerChangedRole(Match match)
        {
            _match = match;
        }

        public void Execute(string input)
        {
            var matched = PlayerChangedRolePattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var playerData = matched.Groups["player"].Value;
            var newRole = matched.Groups["newrole"].Value;

            var player = _match.GetPlayer(playerData);
            var time = _match.GetEntryTime(dateTime);
            var playerRole = player.Roles.SingleOrDefault(s => s.Name == newRole);

            var openRoles = (from r in player.Roles
                             where r.Name != newRole && r.EndTime == null
                             select r);

            foreach (var openRole in openRoles)
            {
                openRole.EndTime = time;
                openRole.SecondsPlayed += ((DateTime)openRole.EndTime - openRole.StartTime).TotalSeconds;
            }

            if (playerRole == null)
            {
                player.Roles.Add(new Role
                {
                    Name = newRole,
                    StartTime = time,
                    EndTime = null,
                    SecondsPlayed = 0
                });
            }
            else
            {
                if (playerRole != null)
                {
                    playerRole.StartTime = time;
                    playerRole.EndTime = null;
                }
            }

            player.currentRole = newRole;
        }

        public bool IsMatch(string input)
        {
            return PlayerChangedRolePattern.IsMatch(input);
        }
    }
}
