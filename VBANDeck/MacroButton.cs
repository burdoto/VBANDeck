using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vban;

#pragma warning disable 4014
namespace VBANDeck
{
    [PluginActionId("de.kaleidox.vbandeck.macrobutton")]
    public class MacroButton : ActionBase
    {
        private readonly PluginSettings _settings;
        private bool _isOn;

        public MacroButton(SDConnection connection, InitialPayload payload) : base(connection,
            payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Initializing MacroButton action");

            try
            {
                _isOn = payload.State == 1;
                if (payload.Settings == null || payload.Settings.Count == 0)
                {
                    _settings = PluginSettings.CreateDefaultSettings();
                    connection.SetSettingsAsync(JObject.FromObject(_settings));
                }
                else
                {
                    _settings = payload.Settings.ToObject<PluginSettings>();
                }

                if (_settings == null)
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR,
                        "Settings for MacroButton action could not be initialized");
                    throw new ApplicationException("Settings for MacroButton action could not be initialized");
                }

                if (_vbanStream == null)
                    MakeVban();
            }
            catch (Exception e)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL,
                    "Exception occurred in MacroButton constructor: " + e.Message);
            }
        }


        protected override string _streamName => _settings.StreamName;
        protected override IPAddress _ipAddress => _settings.IpAddress;
        protected override int _port => _settings.Port;

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
                Logger.Instance.LogMessage(TracingLevel.ERROR,
                    "VBANStream is null! Is your IP-Address parseable? [" + _settings.IpAddress +
                    "]");
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.DEBUG, "Sending simple script!");

                foreach (string line in (_isOn ? _settings.OffScript : _settings.OnScript).Split("\n"))
                {
                    _vbanStream.SendData(line);
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, "Sent script line: " + line);
                    Thread.Sleep(30);
                }

                _isOn = !_isOn;

                Logger.Instance.LogMessage(TracingLevel.DEBUG, "Script sent!");
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(_settings, payload.Settings);

            MakeVban();
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

        private class PluginSettings
        {
            private string _streamName;
            internal IPAddress IpAddress;
            internal int Port;

            [JsonProperty(PropertyName = "ip-address")]
            public string IpAddressProperty
            {
                set
                {
                    if (Regex.IsMatch(value, "\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}"))
                        try
                        {
                            IpAddress = IPAddress.Parse(value);
                            return;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                    Logger.Instance.LogMessage(TracingLevel.ERROR,
                        $"Invalid IP Address [{value}] was entered! Falling back to [{IPAddress.Loopback}]");
                    IpAddress = IPAddress.Loopback;
                }
            }

            [JsonProperty(PropertyName = "port")]
            public string PortProperty
            {
                set
                {
                    int port;
                    if (Regex.IsMatch(value, "[0-9]{1,5}")
                        && (port = int.Parse(value)) <= 65535)
                    {
                        Port = port;
                        return;
                    }

                    Logger.Instance.LogMessage(TracingLevel.WARN,
                        $"Invalid Port [{value}] was entered! Falling back to [{VBAN.DefaultPort.ToString()}]");
                    Port = VBAN.DefaultPort;
                }
            }

            [JsonProperty(PropertyName = "script-on")]
            public string OnScript { get; set; }

            [JsonProperty(PropertyName = "script-off")]
            public string OffScript { get; set; }

            [JsonProperty(PropertyName = "streamName")]
            public string StreamName
            {
                set
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _streamName = value;
                        return;
                    }

                    Logger.Instance.LogMessage(TracingLevel.WARN,
                        $"Invalid Stream Name [{value}] was entered! Falling back to [Command1]");
                    _streamName = "Command1";
                }

                get => _streamName;
            }

            public static PluginSettings CreateDefaultSettings()
            {
                var instance = new PluginSettings
                {
                    IpAddressProperty = null,
                    Port = 0,
                    StreamName = "Command1",
                    OnScript = "",
                    OffScript = ""
                };

                return instance;
            }
        }
    }
}