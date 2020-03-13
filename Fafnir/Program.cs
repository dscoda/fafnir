using Fafnir.LogParsers;
using Fafnir.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fafnir
{
    class Program
    {
        public static MatchLog _matchLog = new MatchLog();

        static void Main(string[] args)
        {
            string line;

            System.IO.StreamReader file =
                new System.IO.StreamReader(@"..\..\..\..\log_exmples\L0311048.log");

            //todo: remove once refactor is done
            string dateTimeRegEx = @"\d{2}\/\d{2}\/\d{4} - \d{2}:\d{2}:\d{2})";

            var parsers = new IHLDSLogParser[]
            {
                new PlayerEnteredGame(_matchLog),
                new PlayerJoinedTeam(_matchLog),
                new PlayerDisconnected(_matchLog)
            };

            while ((line = file.ReadLine()) != null)
            {
                var match = (from p in parsers
                             where p.IsMatch(line)
                             select p).SingleOrDefault();

                if(match != null)
                {
                    match.Execute(line);
                }
                
                
                var playerChangedRolePattern = new Regex(@$"L (?<date>{dateTimeRegEx}: ""(?<player>.*?)"" changed role to ""(?<newrole>.*?)""");
                var playerKillOtherPlayerPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: ""(?<killer>.*?)"" killed ""(?<victim>.*?)"" with ""(?<weapon>.*?)""");
                var logFileClosedPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: Log file closed");
                var loadingMapPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: Loading map ""(?<map>.*?)""");

                //todo: implement this...
                //var playerInfectedAnotherPlayerPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: ""(?<player>.*?)"" triggered ""Medic_Infection"" against ""(?<victim>.*?)""");

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

            foreach (var player in _matchLog.players)
            {
                if (player.LeaveTime == null)
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

            foreach (var player in _matchLog.players)
            {
                Console.WriteLine("Player: {0}", player.Name);
                Console.WriteLine("     Time Played: {0} seconds", player.SecondsPlayed);
                Console.WriteLine("     SteamId: {0}", player.SteamId);
                Console.WriteLine("     StartTime: {0}", player.JoinTime);
                Console.WriteLine("     EndTime: {0}", player.LeaveTime);

                var kills = _matchLog.kills.Where(c => c.KillerName == player.Name && c.KillerId == player.SteamId);
                var deaths = _matchLog.kills.Where(c => c.VictimName == player.Name && c.VictimId == player.SteamId);

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

            Console.WriteLine("Map: {0}", _matchLog.map);
            Console.WriteLine("Start Time: {0}", _matchLog.matchStartTime);
            Console.WriteLine("End Time: {0}", _matchLog.matchEndTime);
            Console.WriteLine("Elapsed Time(min): {0}", (_matchLog.matchEndTime - _matchLog.matchStartTime).TotalSeconds);

            Console.WriteLine("Press Key To Exit...");
            Console.ReadKey();
        }

        private static void PlayerKilledPlayer(string dateTimeData, string killerData, string victimData, string weapon)
        {
            var killer = GetPlayer(killerData);
            var victim = GetPlayer(victimData);
            var time = GetEntryTime(dateTimeData);

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


        //todo: remove once refactor is done
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
            _matchLog.map = map;
            _matchLog.matchStartTime = GetEntryTime(dateTime);
        }

        private static void MatchEnded(string dateTime)
        {
            _matchLog.matchEndTime = GetEntryTime(dateTime);
        }


        //todo: remove once refactor is done
        private static Player GetPlayer(string playerData)
        {
            var playerDataPattern = new Regex(@"(?<name>.*?)<(?<localId>.*?)><(?<steamId>.*?)><(?<team>.*?)>");

            var matched = playerDataPattern.Match(playerData);

            var name = matched.Groups[1].Value;
            var localId = matched.Groups[2].Value;
            var steamId = matched.Groups[3].Value;
            var team = matched.Groups[4].Value;

            var player = (from p in _matchLog.players
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

                _matchLog.players.Add(newPlayer);

                player = (from p in _matchLog.players
                          where p.Name == name && p.SteamId == steamId
                          select p).SingleOrDefault();
            }

            return player;
        }
    }
}
