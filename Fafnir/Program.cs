using Fafnir.LogParsers;
using Fafnir.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fafnir
{
    class Program
    {
        static void Main(string[] args)
        {
            var logFileDirectory = @"C:\tmp\tfc_logs\";
            var archiveLogFileDirectory = @"C:\tmp\tfc_logs\old\";

            var logFiles = Directory.GetFiles(logFileDirectory);

            Array.Sort(logFiles, (x, y) => String.CompareOrdinal(x, y));
            
            List<Match> validMatchLogs = new List<Match>();

            Match currentMatch = null;

            foreach (var logFile in logFiles.Take(logFiles.Length - 1))
            {
                string line;
                bool invalidMatch = false;

                
                if (currentMatch?.MatchEnded ?? true)
                {
                    currentMatch = new Match(logFile);
                }
                else
                {
                    currentMatch.LogFiles.Add(logFile);
                }

                using StreamReader file = new StreamReader(logFile);

                var parsers = new IHLDSLogParser[]
                {
                    new PlayerEnteredGame(currentMatch),
                    new PlayerJoinedTeam(currentMatch),
                    new PlayerDisconnected(currentMatch),
                    new PlayerChangedRole(currentMatch),
                    new PlayerKillOtherPlayer(currentMatch),
                    new LoadingMap(currentMatch),
                    new LogFileClosed(currentMatch),
                    new TeamEndingScore(currentMatch),
                    new PlayerKicked(currentMatch)
                };
                    
                while ((line = file.ReadLine()) != null && !invalidMatch)
                {
                    var match = (from p in parsers
                        where p.IsMatch(line)
                        select p).SingleOrDefault();

                    match?.Execute(line);
                }

                if (currentMatch.IsMatchFinished)
                {
                    validMatchLogs.Add(currentMatch);
                }
            }

            foreach (var validMatchLog in validMatchLogs)
            {
                foreach (var logFile in validMatchLog.LogFiles)
                {
                    var archiveLogFile = logFile.Replace(logFileDirectory, archiveLogFileDirectory);
                    File.Move(logFile,archiveLogFile);
                }
            }

            //console output for now...

            Console.WriteLine("Number of Valid Matches: {0}", validMatchLogs.Count);

            foreach (var validMatchLog in validMatchLogs)
            {
                foreach (var player in validMatchLog.Players)
                {
                    Console.WriteLine("Player: {0}", player.Name);
                    Console.WriteLine("     Time Played: {0} seconds", player.SecondsPlayed);
                    Console.WriteLine("     SteamId: {0}", player.SteamId);
                    Console.WriteLine("     StartTime: {0}", player.JoinTime);
                    Console.WriteLine("     EndTime: {0}", player.LeaveTime);

                    var kills = currentMatch.Kills.Where(c => c.KillerName == player.Name && c.KillerId == player.SteamId);
                    var deaths = currentMatch.Kills.Where(c => c.VictimName == player.Name && c.VictimId == player.SteamId);

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

                Console.WriteLine("Map: {0}", currentMatch.Map);
                Console.WriteLine("Start Time: {0}", currentMatch.MatchStartTime);
                Console.WriteLine("End Time: {0}", currentMatch.MatchEndTime);
                Console.WriteLine("Elapsed Time(min): {0}", (currentMatch.MatchEndTime - currentMatch.MatchStartTime).TotalSeconds);
            }
 
            Console.WriteLine("Press Key To Exit...");
            Console.ReadKey();
        }
    }
}
