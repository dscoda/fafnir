using Fafnir.Models;

namespace Fafnir.LogParsers
{
    class PlayerEnteredGame : IHLDSLogParser
    {
        private Player[] _players;
        public PlayerEnteredGame(Player[] players)
        {
            _players = players;
        }

        public void Execute(string input)
        {
            throw new System.NotImplementedException();
        }

        public bool IsMatch(string input)
        {
            throw new System.NotImplementedException();
        }
    }
}
