using Fafnir.Models;
using System.Text.RegularExpressions;

namespace Fafnir.LogParsers
{
    public class PlayerKillOtherPlayer : IHLDSLogParser
    {
        private MatchLog _matchLog;
        private static string dateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2}";
        private static Regex playerKillOtherPlayerPattern = new Regex(@$"L (?<date>{dateTimeRegEx}): ""(?<killer>.*?)"" killed ""(?<victim>.*?)"" with ""(?<weapon>.*?)""");

        public PlayerKillOtherPlayer(MatchLog matchLog)
        {
            _matchLog = matchLog;
        }

        public void Execute(string input)
        {
            var matched = playerKillOtherPlayerPattern.Match(input);

            var dateTime = matched.Groups["date"].Value;
            var killerData = matched.Groups["killer"].Value;
            var victimData = matched.Groups["victim"].Value;
            var weapon = matched.Groups["weapon"].Value;

            var killer = _matchLog.GetPlayer(killerData);
            var victim = _matchLog.GetPlayer(victimData);
            var time = _matchLog.GetEntryTime(dateTime);

            _matchLog.kills.Add(new Kill
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
            return playerKillOtherPlayerPattern.IsMatch(input);
        }
    }
}
