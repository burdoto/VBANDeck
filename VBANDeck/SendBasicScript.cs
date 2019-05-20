using System;
using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Vban;

#pragma warning disable 4014
namespace VBANDeck
{
    [PluginActionId("de.kaleidox.vbandeck.sendscript")]
    public class SendScript : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                var instance = new PluginSettings
                {
                    IpAddress = "127.0.0.1", Port = VBAN.DefaultPort, Script = ""
                };

                return instance;
            }

            [JsonProperty(PropertyName = "ip-address")]
            public string IpAddress { get; set; }

            [JsonProperty(PropertyName = "port")]
            public int Port { get; set; }

            [JsonProperty(PropertyName = "script")]
            public string Script { get; set; }
        }

        private readonly PluginSettings _settings;
        private VBANStream<string> _vbanStream;

        public SendScript(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                _settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(_settings));
            }
            else
            {
                _settings = payload.Settings.ToObject<PluginSettings>();
            }

            try
            {
                var ipAddress = IPAddress.Parse(_settings.IpAddress);
                _vbanStream.Close();
                _vbanStream = VBAN.OpenTextStream(ipAddress, _settings.Port);
            }
            catch (FormatException e)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, "IP-Address ["+_settings.IpAddress+"] is unparseable! ("+e.Message+")");
            }
        }

        public override void KeyPressed(KeyPayload payload)
        {
            if (string.IsNullOrEmpty(_settings.Script))
            {
                Connection.ShowAlert();
                Logger.Instance.LogMessage(TracingLevel.ERROR, "Cannot send empty string!");
            }
            else if (_vbanStream == null)
            {
                Connection.ShowAlert();
                Logger.Instance.LogMessage(TracingLevel.ERROR, "VBANStream is null! Is your IP-Address parseable? ["+_settings.IpAddress+"]");
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.DEBUG, "Sending simple script!");

                foreach (var line in _settings.Script.Split("\n"))
                {
                    _vbanStream.SendData(line);
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, "Sent script line: "+line);
                }

                Logger.Instance.LogMessage(TracingLevel.DEBUG, "Script sent!");
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(_settings, payload.Settings);

            try
            {
                var ipAddress = IPAddress.Parse(_settings.IpAddress);
                _vbanStream.Close();
                _vbanStream = VBAN.OpenTextStream(ipAddress, _settings.Port);
            }
            catch (FormatException e)
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, "IP-Address ["+_settings.IpAddress+"] is unparseable! ("+e.Message+")");
            }
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