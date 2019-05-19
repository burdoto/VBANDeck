using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Vban;

namespace VBANDeck
{
    [PluginActionId("de.kaleidox.vbandeck.sendscript-simple")]
    public class SendBasicScript : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();

                instance.IpAddress = "127.0.0.1";
                instance.Port = VBAN.DefaultPort;
                instance.Script = "";

                return instance;
            }

            [JsonProperty(PropertyName = "ip-address")]
            public string IpAddress { get; set; }

            [JsonProperty(PropertyName = "port")]
            public int Port { get; set; }

            [JsonProperty(PropertyName = "script")]
            public string Script { get; set; }
        }

        private readonly PluginSettings settings;
        private readonly VBANStream<string> vbanStream;

        public SendBasicScript(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(settings));
            }
            else
            {
                settings = payload.Settings.ToObject<PluginSettings>();
            }

            vbanStream = VBAN.OpenTextStream(IPAddress.Parse(settings.IpAddress), settings.Port);
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Sending simple script");

            vbanStream.SendData(settings.Script);
        }

        public override void KeyReleased(KeyPayload payload)
        {
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
        }

        public override void OnTick()
        {
        }

        public override void Dispose()
        {
        }
    }
}