using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Data;
using ArkBot.WebApi.Model;
using ArkSavegameToolkitNet.Domain;
using ArkSavegameToolkitNet.Domain.Internal;

namespace ArkBot.Ark
{
    public class ArkBotAnonymizeData : ArkAnonymizeData
    {
        private Dictionary<Tuple<string, int>, int> _tamed = new Dictionary<Tuple<string, int>, int>();
        private Dictionary<string, ExpandoObject> _server = new Dictionary<string, ExpandoObject>();
        private int _serverNext = 0;

        public dynamic GetServer(string serverKey)
        {
            var result = (dynamic)_server.GetOrCreate(serverKey, () => {
                dynamic o = new ExpandoObject();
                var n = ++_serverNext;
                o.Id = n;
                o.Key = $"server{n}";
                o.Address = $"server{n}.survivetheark.com:27015";
                o.Name = $"Server {n}";

                return (ExpandoObject)o;
            });

            return result;
        }

        public override string GetPlayerName(int id)
        {
            return $"Player {GetThreeLetterId(id)}";
        }

        public override string GetCharacterName(int id)
        {
            return $"Character {GetThreeLetterId(id)}";
        }

        public override string GetTribeName(int id)
        {
            return $"Tribe {GetThreeLetterId(id)}";
        }

        public override string GetDinoName(string className, int? teamId)
        {
            if (string.IsNullOrEmpty(className) || !teamId.HasValue) return base.GetDinoName(className, teamId);

            var speciesName = ArkSpeciesAliases.Instance.GetAliasesByClassName(className).FirstOrDefault();
            if (speciesName == null) return base.GetDinoName(className, teamId);

            var key = Tuple.Create(className, teamId.Value);
            _tamed.GetOrCreate(key, () => 0);

            return $"{speciesName} {++(_tamed[key])}";
        }

        const string _letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string GetThreeLetterId(int id)
        {
            var rnd = new Random(id);
            return $"{_letters[rnd.Next(_letters.Length)]}{_letters[rnd.Next(_letters.Length)]}{_letters[rnd.Next(_letters.Length)]}";
        }
    }
}
