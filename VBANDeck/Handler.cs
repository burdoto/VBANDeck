using System;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;

namespace VBANDeck
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Handler
    {
        public void SendToPlugin(object sender, StreamDeckEventReceivedEventArgs<SendToPluginEvent> e)
        {
            throw new NotImplementedException();
        }

        public void KeyDown(object sender, StreamDeckEventReceivedEventArgs<KeyDownEvent> e)
        {
            throw new NotImplementedException();
        }

        public void KeyUp(object sender, StreamDeckEventReceivedEventArgs<KeyUpEvent> e)
        {
            throw new NotImplementedException();
        }
    }
}