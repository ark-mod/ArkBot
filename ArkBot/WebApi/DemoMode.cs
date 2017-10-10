using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi
{
    /// <summary>
    /// Generate random names for anonymous screenshots etc.
    /// </summary>
    public class DemoMode
    {
        private Dictionary<int, string> _playerNames;
        private Dictionary<int, string> _tribeNames;
        private Dictionary<Tuple<uint, uint>, string> _creatureNames;
        private Dictionary<string, string> _steamIds;

        private int _nextTribeNum;
        private int _nextPlayerNum;
        private Dictionary<string, int> _nextCreatureNum;

        private Random _rnd;

        public DemoMode()
        {
            _playerNames = new Dictionary<int, string>();
            _tribeNames = new Dictionary<int, string>();
            _creatureNames = new Dictionary<Tuple<uint, uint>, string>();
            _steamIds = new Dictionary<string, string>();
            _nextCreatureNum = new Dictionary<string, int>();
            _rnd = new Random();
        }

        public string GetTribeName(int id = -1)
        {
            if (id == -1) id = _rnd.Next();

            string name = null;
            if (_tribeNames.TryGetValue(id, out name)) return name;

            name = $"Tribe {++_nextTribeNum}";
            _tribeNames.Add(id, name);

            return name;
        }

        public string GetPlayerName(int id = -1)
        {
            if (id == -1) id = _rnd.Next();

            string name = null;
            if (_playerNames.TryGetValue(id, out name)) return name;

            name = $"Player {++_nextPlayerNum}";
            _playerNames.Add(id, name);

            return name;
        }

        public string GetCreatureName(uint id1, uint id2, string species)
        {
            var id = Tuple.Create(id1, id2);
            string name = null;
            if (_creatureNames.TryGetValue(id, out name)) return name;

            int currentNum = 0;
            if (!_nextCreatureNum.TryGetValue(species, out currentNum)) currentNum = 0;
            _nextCreatureNum[species] = ++currentNum;

            name = $"{species ?? "Creature"} {currentNum}";
            _creatureNames.Add(id, name);

            return name;
        }

        public string GetSteamId(string steamId)
        {
            string name = null;
            if (_steamIds.TryGetValue(steamId, out name)) return name;

            name = $"7656119";
            for (var i = 0; i < 10; i++) name += _rnd.Next(0, 10);
            _steamIds.Add(steamId, name);

            return name;
        }
    }
}
