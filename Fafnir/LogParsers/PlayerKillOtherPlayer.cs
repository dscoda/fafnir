using Fafnir.Models;
using System.Text.RegularExpressions;
using Match = Fafnir.Models.Match;

namespace Fafnir.LogParsers
{
    public class PlayerKillOtherPlayer : IHLDSLogParser
    {
        private readonly Match _match;
        private const string DateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static readonly Regex PlayerKillOtherPlayerPattern = new Regex(@$"L (?<date>{DateTimeRegEx}): ""(?<killer>.*?)"" killed ""(?<victim>.*?)"" with ""(?<weapon>.*?)""");

        public PlayerKillOtherPlayer(Match match)
        {
            _match = match;
        }

        public void Execute(string input)
        {
            if (_match.MatchEnded)
            {
                return;
            }

            var matched = PlayerKillOtherPlayerPattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var killerData = matched.Groups["killer"].Value;
            var victimData = matched.Groups["victim"].Value;
            var weapon = matched.Groups["weapon"].Value;

            var killer = _match.GetPlayer(killerData);
            var victim = _match.GetPlayer(victimData);
            var time = _match.GetEntryTime(dateTime);

            _match.Kills.Add(new Kill
            {
                TimeStamp = time,
                KillerName = killer.Name,
                KillerId = killer.SteamId,
                KillerRole = killer.currentRole,
                killerTeam = killer.currentTeam,
                VictimName = victim.Name,
                VictimId = victim.SteamId,
                VictimRole = victim.currentRole,
                VictimTeam = victim.currentTeam,
                Weapon = weapon
            });
        }

        public bool IsMatch(string input)
        {
            return PlayerKillOtherPlayerPattern.IsMatch(input);
        }
    }
}
