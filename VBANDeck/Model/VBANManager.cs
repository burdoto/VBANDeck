using System.Collections.Generic;
using System.Net;
using Vban;

namespace VBANDeck.Model {
    public class VbanManager {
        private readonly Dictionary<IPEndPoint, VBANStream<string>> _instances = new Dictionary<IPEndPoint, VBANStream<string>>();

        protected internal VbanManager() { }

        public VBANStream<string> GetOrCreate(string adress, int port) {
            VBANStream<string> vban;
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(adress), port);
            if (!_instances.ContainsKey(ip)) {
                _instances.Add(ip, vban = VBAN.OpenTextStream(ip.Address, ip.Port));
            }

            _instances.TryGetValue(ip, out vban);
            return vban;
        }

        public bool Destroy(string adress, int port) {
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(adress), port);
            return _instances.Remove(ip);
        }
    }
}