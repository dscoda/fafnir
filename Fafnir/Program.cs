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
                new System.IO.StreamReader(@"..\..\..\..\log_exmples\L0415053.log");
            
            var parsers = new IHLDSLogParser[]
            {
                new PlayerEnteredGame(_matchLog),
                new PlayerJoinedTeam(_matchLog),
                new PlayerDisconnected(_matchLog),
                new PlayerChangedRole(_matchLog),
                new PlayerKillOtherPlayer(_matchLog),
                new LoadingMap(_matchLog),
                new LogFileClosed(_matchLog)
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

                //todo: implement this...
                //var playerInfectedAnotherPlayerPattern = new Regex(@$"L (?<date>{dateTimeRegEx}: ""(?<player>.*?)"" triggered ""Medic_Infection"" against ""(?<victim>.*?)""");
            }

            
            //console output for now...
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
                    Console.WriteLine("          Kills: {0}", kills.Count(c => c.killerTeam == team.Name));
                    Console.WriteLine("          Deaths: {0}", deaths.Count(c => c.VictimTeam == team.Name));
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
    }
}
