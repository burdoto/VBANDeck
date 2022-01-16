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
            Logger.Instance.LogMessage(TracingLevel.DEBUG, "Creating new VBANOutputStream; old = " + _vbanStream);
            
            _vbanStream?.Close();
            _vbanStream?.Dispose();

            _vbanStream = new VBANOutputStream<string>(
                new VBANPacket<string>.Factory<string, IEnumerable<char>>.Builder<string, IEnumerable<char>>(VBAN.Protocol<string>.Text)
                {
                    HeadFactory = new VBANPacketHead<string>.Factory<string, IEnumerable<char>>.Builder<string, IEnumerable<char>>(VBAN.Protocol<string>.Text)
                    {
                        SampleRate = VBAN.BitsPerSecond.Bps256000,
                        Channel = 0,
                        Samples = 0,
                        Format = VBAN.CommandFormat.Ascii,
                        Codec = VBAN.Codec.PCM,
                        StreamName = _streamName
                    }.Build()
                }.Build(), _ipAddress, _port);
        }
    }
}