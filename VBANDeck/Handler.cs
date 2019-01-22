using System.IO;
using Newtonsoft.Json.Linq;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using Vban;
using VBANDeck.Model;

namespace VBANDeck {
    public class Handler {
        private readonly Plugin               _plugin;
        private readonly StreamDeckConnection _streamDeck;

        public Handler(Plugin plugin, StreamDeckConnection streamDeck) {
            _plugin     = plugin;
            _streamDeck = streamDeck;
        }

        public void DeviceConnect(object sender, StreamDeckEventReceivedEventArgs<DeviceDidConnectEvent> e) {
            DeviceDidConnectEvent ev       = e.Event;
            DeviceModifier        modifier = _plugin.DeviceManager.GetOrCreate(ev.Device).Modifier;

            modifier.DefineSize(ev.DeviceInfo.Size)
                    .DefineType(ev.DeviceInfo.Type);
        }

        public void DeviceDisconnect(object sender, StreamDeckEventReceivedEventArgs<DeviceDidDisconnectEvent> e) {
            DeviceDidDisconnectEvent ev = e.Event;
            _plugin.DeviceManager.Destroy(ev.Device);
        }

        public void WillAppear(object sender, StreamDeckEventReceivedEventArgs<WillAppearEvent> e) {
            WillAppearEvent   ev       = e.Event;
            AppearancePayload payload  = ev.Payload;
            ButtonModifier    modifier = _plugin.ButtonManager.GetOrCreate(ev.Context).Modifier;

            modifier.DefineState(payload.State)
                    .DefineSettings(payload.Settings)
                    .DefineCoordinates(payload.Coordinates.Rows, payload.Coordinates.Columns)
                    .DefineMultiAction(payload.IsInMultiAction);
        }

        public void WillDisappear(object sender, StreamDeckEventReceivedEventArgs<WillDisappearEvent> e) {
            WillDisappearEvent ev = e.Event;
            _plugin.ButtonManager.Destroy(ev.Context);
        }

        public void KeyDown(object sender, StreamDeckEventReceivedEventArgs<KeyDownEvent> e) {
            KeyDownEvent ev       = e.Event;
            ActionButton button   = _plugin.ButtonManager.GetOrCreate(ev.Context);
            JObject      settings = button.Settings;

            button.State = 1;

            VBANStream<string> vban = _plugin.VbanManager.GetOrCreate(settings.Value<string>("address"),
                    settings.Value<int>("port"));

            if (settings.Value<bool>("fromFile")) {
                StreamReader reader = new StreamReader(
                        new FileStream(settings.Value<string>("script"), FileMode.Open));
                vban.SendData(reader.ReadToEnd());
            }
            else {
                vban.SendData(settings.Value<string>("script"));
            }

            button.State = 0;
        }

        public void KeyUp(object sender, StreamDeckEventReceivedEventArgs<KeyUpEvent> e) {
            KeyUpEvent   ev     = e.Event;
            ActionButton button = _plugin.ButtonManager.GetOrCreate(ev.Context);

            button.State = 0;
        }

        public void SendToPlugin(object sender, StreamDeckEventReceivedEventArgs<SendToPluginEvent> e) {
            SendToPluginEvent ev     = e.Event;
            ActionButton      button = _plugin.ButtonManager.GetOrCreate(ev.Context);
            JObject           data   = ev.Payload;

            _streamDeck.SetSettingsAsync(data.Value<JObject>("data"), button.Context);
        }
    }
}