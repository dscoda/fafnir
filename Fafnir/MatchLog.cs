using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fafnir.Models
{
    public class MatchLog
    {
        public List<Player> players;
        public DateTime matchStartTime;
        public DateTime matchEndTime;
        public string map;
        public List<Kill> kills;

        public MatchLog()
        {
            players = new List<Player> { };
            matchStartTime =  new DateTime();
            matchEndTime = new DateTime();
            map = string.Empty;
            kills = new List<Kill> { };
        }

        public Player GetPlayer(string playerData)
        {
            var playerDataPattern = new Regex(@"(?<name>.*?)<(?<localId>.*?)><(?<steamId>.*?)><(?<team>.*?)>");

            var matched = playerDataPattern.Match(playerData);

            var name = matched.Groups[1].Value;
            var localId = matched.Groups[2].Value;
            var steamId = matched.Groups[3].Value;
            var team = matched.Groups[4].Value;

            var player = (from p in players
                          where p.Name == name && p.SteamId == steamId
                          select p).SingleOrDefault();

            if (player == null)
            {
                var newPlayer = new Player
                {
                    Name = name,
                    SteamId = steamId,
                    JoinTime = null,
                    LeaveTime = null,
                    SecondsPlayed = 0,
                    Teams = new List<Player.Team> { },
                    Roles = new List<Role> { }
                };

                players.Add(newPlayer);

                player = (from p in players
                          where p.Name == name && p.SteamId == steamId
                          select p).SingleOrDefault();
            }

            return player;
        }

        public DateTime GetEntryTime(string dateTimeData)
        {
            var dateTimePattern = new Regex(@"(?<date>\d{2}\/\d{2}\/\d{4}) - (?<time>\d{2}:\d{2}:\d{2})");

            var matched = dateTimePattern.Match(dateTimeData);

            var date = matched.Groups[1].Value;
            var time = matched.Groups[2].Value;

            return DateTime.Parse(date + " " + time);
        }
    }
}
