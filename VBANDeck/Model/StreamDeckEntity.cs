using streamdeck_client_csharp;

namespace VBANDeck.Model {
    public abstract class StreamDeckEntity {
        internal readonly StreamDeckConnection Conn;

        protected StreamDeckEntity(StreamDeckConnection conn, string context) {
            Conn = conn;
            Context = context;
        }

        public string Context { get; }
    }
}