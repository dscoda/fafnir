using Fafnir.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fafnir
{
    class Program
    {
        public static List<Player> _players = new List<Player> { };
        public static DateTime _matchStartTime = new DateTime();
        public static DateTime _matchEndTime = new DateTime();
        public static string _map = string.Empty;
        public static List<Kill> _kills = new List<Kill> { };

        static void Main(string[] args)
        {
            string line;

            System.IO.StreamReader file =
                new System.IO.StreamReader(@"C:\tmp\log_exmples\L0309001.log");

            string dateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2})";

            while ((line = file.ReadLine()) != null)
            {
                var playerEnteredGamePattern = new Regex(@$"L (?<date>{dateTimeRegEx}: ""(?<player>.*?)"" entered the game");
                var playerJoinedTeamPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: ""(?<player>.*?)"" joined team ""(?<team>.*?)""");
                var playerDisconnectedPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: ""(?<player>.*?)"" disconnected");
                //var playerInfectedAnotherPlayerPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: ""(?<player>.*?)"" triggered ""Medic_Infection"" against ""(?<victim>.*?)""");
                var playerChangedRolePattern = new Regex(@$"L (?<date>{dateTimeRegEx}: ""(?<player>.*?)"" changed role to ""(?<newrole>.*?)""");
                var playerKillOtherPlayerPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: ""(?<killer>.*?)"" killed ""(?<victim>.*?)"" with ""(?<weapon>.*?)""");
                var logFileClosedPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: Log file closed");
                var loadingMapPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: Loading map ""(?<map>.*?)""");

                if (playerEnteredGamePattern.IsMatch(line))
                {
                    var matched = playerEnteredGamePattern.Match(line);

                    var dateTime = matched.Groups["date"].Value;
                    var playerData = matched.Groups["player"].Value;

                    PlayerEnteredGame(dateTime, playerData);
                }

                if (loadingMapPattern.IsMatch(line))
                {
                    var matched = loadingMapPattern.Match(line);

                    var dateTime = matched.Groups[1].Value;
                    var map = matched.Groups[2].Value;

                    LoadMap(dateTime, map);
                }

                if (logFileClosedPattern.IsMatch(line))
                {
                    var matched = logFileClosedPattern.Match(line);

                    var dateTime = matched.Groups[1].Value;

                    MatchEnded(dateTime);
                }

                if (playerDisconnectedPattern.IsMatch(line))
                {
                    var matched = playerDisconnectedPattern.Match(line);

                    var dateTime = matched.Groups[1].Value;
                    var playerData = matched.Groups[2].Value;

                    PlayerDisconnected(dateTime, playerData);
                }

                if (playerJoinedTeamPattern.IsMatch(line))
                {
                    var matched = playerJoinedTeamPattern.Match(line);

                    var dateTime = matched.Groups[1].Value;
                    var playerData = matched.Groups[2].Value;
                    var team = matched.Groups[3].Value;

                    PlayerJoinedTeam(dateTime, playerData, team);
                }

                if (playerChangedRolePattern.IsMatch(line))
                {
                    var matched = playerChangedRolePattern.Match(line);

                    var dateTime = matched.Groups[1].Value;
                    var playerData = matched.Groups[2].Value;
                    var newRole = matched.Groups[3].Value;

                    PlayerChangedRole(dateTime, playerData, newRole);
                }

                if (playerKillOtherPlayerPattern.IsMatch(line))
                {
                    var matched = playerKillOtherPlayerPattern.Match(line);

                    var dateTime = matched.Groups[1].Value;
                    var killerData = matched.Groups[2].Value;
                    var victimData = matched.Groups[3].Value;
                    var weapon = matched.Groups[4].Value;

                    PlayerKilledPlayer(dateTime, killerData, victimData, weapon);
                }
            }

            foreach (var player in _players)
            {
                if (player.LeaveTime == null)
                {
                    player.LeaveTime = _matchEndTime;
                    player.SecondsPlayed += ((DateTime)player.LeaveTime - (DateTime)player.JoinTime).TotalSeconds;

                    foreach (var team in player.Teams.Where(w => w.LeaveTime == null))
                    {
                        team.LeaveTime = _matchEndTime;
                        team.SecondsPlayed += ((DateTime)team.LeaveTime - (DateTime)team.JoinTime).TotalSeconds;
                    }
                }

                var openRoles = (from r in player.Roles where r.EndTime == null select r);

                foreach (var openRole in openRoles)
                {
                    openRole.EndTime = _matchEndTime;
                    openRole.SecondsPlayed += ((DateTime)openRole.EndTime - openRole.StartTime).TotalSeconds;
                }
            }

            foreach (var player in _players)
            {
                Console.WriteLine("Player: {0}", player.Name);
                Console.WriteLine("     Time Played: {0} seconds", player.SecondsPlayed);
                Console.WriteLine("     SteamId: {0}", player.SteamId);
                Console.WriteLine("     StartTime: {0}", player.JoinTime);
                Console.WriteLine("     EndTime: {0}", player.LeaveTime);

                var kills = _kills.Where(c => c.KillerName == player.Name && c.KillerId == player.SteamId);
                var deaths = _kills.Where(c => c.VictimName == player.Name && c.VictimId == player.SteamId);

                var favWeapon = (from k in kills
                                 group k by k.Weapon into grp
                                 select new
                                 {
                                     Weapon = grp.Key,
                                     Count = grp.Count()
                                 }).OrderByDescending(o => o.Count).FirstOrDefault();


                Console.WriteLine("     Total Kills: {0}", kills.Count());
                Console.WriteLine("     Total Deaths: {0}", deaths.Count());
                Console.WriteLine("     Fav Weapon: {0}", favWeapon != null ? favWeapon.Weapon : string.Empty);

                foreach (var team in player.Teams)
                {
                    Console.WriteLine("          Team: {0}", team.Name);
                    Console.WriteLine("          Time Played: {0}", team.SecondsPlayed);
                    Console.WriteLine("          -----------------");
                }

                foreach (var role in player.Roles)
                {
                    Console.WriteLine("               Role: {0}", role.Name);
                    Console.WriteLine("               Time Played: {0}", role.SecondsPlayed);
                    Console.WriteLine("               Kills: {0}", kills.Count(c => c.KillerRole == role.Name));
                    Console.WriteLine("               Deaths: {0}", deaths.Count(c => c.VictimRole == role.Name));
                    Console.WriteLine("               -----------------------");
                }
            }

            Console.WriteLine("Map: {0}", _map);
            Console.WriteLine("Start Time: {0}", _matchStartTime);
            Console.WriteLine("End Time: {0}", _matchEndTime);
            Console.WriteLine("Elapsed Time(min): {0}", (_matchEndTime - _matchStartTime).TotalSeconds);

            Console.WriteLine("Press Key To Exit...");
            Console.ReadKey();
        }

        private static void PlayerKilledPlayer(string dateTimeData, string killerData, string victimData, string weapon)
        {
            var killer = GetPlayer(killerData);
            var victim = GetPlayer(victimData);
            var time = GetEntryTime(dateTimeData);

            _kills.Add(new Kill
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

        private static void PlayerJoinedTeam(string dateTimeData, string playerData, string teamName)
        {
            var currentPlayer = GetPlayer(playerData);
            var time = GetEntryTime(dateTimeData);
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

        private static void PlayerChangedRole(string dateTimeData, string playerData, string newRole)
        {
            var player = GetPlayer(playerData);
            var time = GetEntryTime(dateTimeData);
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

        private static void PlayerDisconnected(string dateTimeData, string playerData)
        {
            Player currentPlayer;

            currentPlayer = GetPlayer(playerData);
            DateTime leaveTime = GetEntryTime(dateTimeData);

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
        private static DateTime GetEntryTime(string dateTimeData)
        {
            var dateTimePattern = new Regex(@"(?<date>\d{2}\/\d{2}\/\d{4}) - (?<time>\d{2}:\d{2}:\d{2})");

            var matched = dateTimePattern.Match(dateTimeData);

            var date = matched.Groups[1].Value;
            var time = matched.Groups[2].Value;

            return DateTime.Parse(date + " " + time);
        }

        private static void LoadMap(string dateTime, string map)
        {
            _map = map;
            _matchStartTime = GetEntryTime(dateTime);
        }

        private static void MatchEnded(string dateTime)
        {
            _matchEndTime = GetEntryTime(dateTime);
        }

        private static void PlayerEnteredGame(string dateTimeData, string playerData)
        {
            var joinedTime = GetEntryTime(dateTimeData);

            Player currentPlayer;

            currentPlayer = GetPlayer(playerData);

            currentPlayer.JoinTime = joinedTime;
            currentPlayer.LeaveTime = null;
        }

        private static Player GetPlayer(string playerData)
        {
            var playerDataPattern = new Regex(@"(?<name>.*?)<(?<localId>.*?)><(?<steamId>.*?)><(?<team>.*?)>");

            var matched = playerDataPattern.Match(playerData);

            var name = matched.Groups[1].Value;
            var localId = matched.Groups[2].Value;
            var steamId = matched.Groups[3].Value;
            var team = matched.Groups[4].Value;

            var player = (from p in _players
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

                _players.Add(newPlayer);

                player = (from p in _players
                          where p.Name == name && p.SteamId == steamId
                          select p).SingleOrDefault();
            }

            return player;
        }
    }
}
