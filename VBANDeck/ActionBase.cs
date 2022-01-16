using System.Collections.Generic;
using System.Net;
using BarRaider.SdTools;
using Vban;
using Vban.Model;

namespace VBANDeck
{
    public abstract class ActionBase : PluginBase
    {
        protected VBANOutputStream<string> _vbanStream;

        public ActionBase(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        protected abstract string _streamName { get; }
        protected abstract IPAddress _ipAddress { get; }
        protected abstract int _port { get; }

        protected void MakeVban()
        {
            _vbanStream?.Close();
            _vbanStream?.Dispose();
            _vbanStream = null;

            var headFactoryBuilder
                = VBANPacketHead<string>.Factory<string, IEnumerable<char>>.CreateBuilder(VBAN.Protocol<string>.Text);
            headFactoryBuilder.SampleRate = VBAN.BitsPerSecond.Bps256000;
            headFactoryBuilder.Channel = 0;
            headFactoryBuilder.Samples = 0;
            headFactoryBuilder.Format = VBAN.CommandFormat.Ascii;
            headFactoryBuilder.Codec = VBAN.Codec.PCM;
            headFactoryBuilder.StreamName = _streamName;
            var headFactory = headFactoryBuilder.Build();

            var bodyFactoryBuilder
                = VBANPacket<string>.Factory<string, IEnumerable<char>>.CreateBuilder(VBAN.Protocol<string>.Text);
            bodyFactoryBuilder.HeadFactory = headFactory;
            var bodyFactory = bodyFactoryBuilder.Build();

            _vbanStream = new VBANOutputStream<string>(bodyFactory, _ipAddress, _port);
        }
    }
}