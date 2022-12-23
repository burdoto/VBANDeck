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
    [PluginActionId("de.kaleidox.vbandeck.sendscript")]
    public class SendScript : ActionBase
    {
        private readonly PluginSettings _settings;

        public SendScript(SDConnection connection, InitialPayload payload) : base(connection,
            payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Initializing BasicScript action");

            try
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

                if (_settings == null)
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR,
                        "Settings for SendScript action could not be initialized");
                    throw new ApplicationException("Settings for SendScript action could not be initialized");
                }

                if (_vbanStream == null)
                    MakeVban();
            }
            catch (Exception e)
            {
                Program.LogException(e, "SendScript constructor");
            }
        }

        protected override string _streamName => _settings.StreamName;
        protected override IPAddress _ipAddress => _settings.IpAddress;
        protected override int _port => _settings.Port;

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
                Logger.Instance.LogMessage(TracingLevel.ERROR,
                    "VBANStream is null! Is your IP-Address parseable? [" + _settings.IpAddress +
                    "]");
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.DEBUG, "Sending simple script!");

                foreach (string line in _settings.Script.Split("\n"))
                {
                    _vbanStream.SendData(line);
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, "Sent script line: " + line);
                    Thread.Sleep(30);
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
            private string _streamName = "Command1";
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

            [JsonProperty(PropertyName = "script")]
            public string Script { get; set; }

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
                    IpAddressProperty = null, Port = 0, Script = "", StreamName = "Command1"
                };

                return instance;
            }
        }
    }
}