using System;
using System.Collections.Generic;

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


    }
}
