﻿
using System;
using System.Collections.Generic;
using System.Text;

namespace Fafnir.Models
{
    public class Player
    {
        public string Name;
        public string SteamId;
        public DateTime? JoinTime;
        public DateTime? LeaveTime;
        public double SecondsPlayed;
        public List<Team> Teams;
        public string currentTeam;
        public string currentRole;
        public List<Role> Roles;
        public int KillCount;
        public int DeathCount;

        public class Team
        {
            public string Name;
            public DateTime JoinTime;
            public DateTime? LeaveTime;
            public double SecondsPlayed;
        }
    }

    
}
