using System;
using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Vban;

#pragma warning disable 4014
namespace VBANDeck
{
    [PluginActionId("de.kaleidox.vbandeck.macrobutton")]
    public class MacroButton : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                var instance = new PluginSettings
                {
                    IpAddress = "127.0.0.1", Port = VBAN.DefaultPort, InitialScript = "", OnScript = "", OffScript = ""
                };

                return instance;
            }

            [JsonProperty(PropertyName = "ip-address")]
            public string IpAddress { get; set; }

            [JsonProperty(PropertyName = "port")]
            public int Port { get; set; }

            [JsonProperty(PropertyName = "initial-script")]
            public string InitialScript { get; set; }

            [JsonProperty(PropertyName = "script-on")]
            public string OnScript { get; set; }

            [JsonProperty(PropertyName = "script-off")]
            public string OffScript { get; set; }
        }

        private readonly PluginSettings _settings;
        private VBANStream<string> _vbanStream;
        private bool _isOn;
        private bool _initiated;

        public MacroButton(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            if (string.IsNullOrEmpty(_settings.OnScript))
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

                foreach (var line in (_isOn ? _settings.OffScript : _settings.OnScript).Split("\n"))
                {
                    _vbanStream.SendData(line);
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, "Sent script line: "+line);
                    _isOn = !_isOn;
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
            if (!_initiated && !string.IsNullOrEmpty(_settings.InitialScript) && _vbanStream != null)
            {
                foreach (var line in _settings.InitialScript.Split("\n"))
                {
                    _vbanStream.SendData(line);
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, "Sent initial script line: "+line);
                }
                Logger.Instance.LogMessage(TracingLevel.INFO, "Sent initial script!");
                _initiated = true;
            }
        }

        public override void Dispose()
        {
        }
    }
}