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
    public class MacroButton : PluginBase
    {
        private readonly PluginSettings _settings;
        private bool _isOn;
        private VBANStream<string> _vbanStream;

        public MacroButton(SDConnection connection, InitialPayload payload) : base(connection,
            payload)
        {
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

                _vbanStream = VBAN.OpenTextStream(_settings.IpAddress, _settings.Port);
            }
            catch (Exception e)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL,
                    "Exception occurred in MacroButton constructor: " + e.Message);
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
                Logger.Instance.LogMessage(TracingLevel.ERROR,
                    "VBANStream is null! Is your IP-Address parseable? [" + _settings.IpAddress +
                    "]");
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.DEBUG, "Sending simple script!");

                foreach (var line in (_isOn ? _settings.OffScript : _settings.OnScript).Split("\n"))
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

            _vbanStream?.Close();
            _vbanStream = VBAN.OpenTextStream(_settings.IpAddress, _settings.Port);
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
            internal IPAddress IpAddress;
            internal int Port;

            [JsonProperty(PropertyName = "ip-address")]
            public string IpAddressProperty
            {
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        IpAddress = IPAddress.Loopback;
                        return;
                    }

                    try
                    {
                        IpAddress = IPAddress.Parse(value);
                    }
                    catch (FormatException)
                    {
                        Logger.Instance.LogMessage(TracingLevel.WARN,
                            "Unparseable IP address [" + value +
                            "] was entered! Falling back to IPAddress.Loopback");
                        IpAddress = IPAddress.Loopback;
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.LogMessage(TracingLevel.ERROR,
                            "Unexpected exception occurred in IpAddress setter: " + e.Message +
                            "\n Falling back to IPAddress.Loopback");
                        IpAddress = IPAddress.Loopback;
                    }
                }
            }

            [JsonProperty(PropertyName = "port")]
            public string PortProperty
            {
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        Port = VBAN.DefaultPort;
                        return;
                    }

                    if (Regex.IsMatch(value, "[0-9]{1,6}"))
                    {
                        Port = int.Parse(value);
                    }
                    else
                    {
                        Logger.Instance.LogMessage(TracingLevel.WARN,
                            "Invalid Port [" + value + "] was entered! Falling back to " +
                            VBAN.DefaultPort);
                        Port = VBAN.DefaultPort;
                    }
                }
            }

            [JsonProperty(PropertyName = "script-on")]
            public string OnScript { get; set; }

            [JsonProperty(PropertyName = "script-off")]
            public string OffScript { get; set; }

            public static PluginSettings CreateDefaultSettings()
            {
                var instance = new PluginSettings
                {
                    IpAddressProperty = null, Port = 0, OnScript = "",
                    OffScript = ""
                };

                return instance;
            }
        }
    }
}