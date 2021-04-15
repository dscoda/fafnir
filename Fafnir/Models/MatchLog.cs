using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fafnir.Models
{
    public class MatchLog
    {
        public List<Player> Players;
        public DateTime MatchStartTime;
        public DateTime MatchEndTime;
        public string Map;
        public List<Kill> Kills;
        public string StartingLogFile;
        public List<string> LogFiles;
        public bool MatchEnded;

        public MatchLog(string startingLogFile)
        {
            Players = new List<Player> { };
            MatchStartTime =  new DateTime();
            MatchEndTime = new DateTime();
            Map = string.Empty;
            Kills = new List<Kill> { };
            StartingLogFile = startingLogFile;
            LogFiles = new List<string>(); 
            LogFiles.Add(startingLogFile);
            MatchEnded = false;
        }

        public bool IsValidStart => !string.IsNullOrEmpty(this.Map) && this.MatchStartTime != DateTime.MinValue;
        public bool IsMatchFinished => MatchEndTime != DateTime.MinValue;

        public Player GetPlayer(string playerData)
        {
            var playerDataPattern = new Regex(@"(?<name>.*?)<(?<localId>.*?)><(?<steamId>.*?)><(?<team>.*?)>");

            var matched = playerDataPattern.Match(playerData);

            var name = matched.Groups[1].Value;
            var localId = matched.Groups[2].Value;
            var steamId = matched.Groups[3].Value;
            var team = matched.Groups[4].Value;

            var player = (from p in Players
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

                Players.Add(newPlayer);

                player = (from p in Players
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
