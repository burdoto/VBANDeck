using System;
using System.Collections.Generic;
using System.Threading;
using CommandLine;
using Newtonsoft.Json.Linq;
using streamdeck_client_csharp;
using VBANDeck.Model;

namespace VBANDeck {
    public class Plugin {
        private readonly ManualResetEvent _connectEvent    = new ManualResetEvent(false);
        private readonly ManualResetEvent _disconnectEvent = new ManualResetEvent(false);

        private readonly Handler       _handler;
        private readonly SemaphoreSlim _instancesLock = new SemaphoreSlim(1);
        public readonly  JObject       Info;

        public readonly int    Port;
        public readonly string RegisterEvent;
        public readonly string Uuid;

        public Plugin(StreamDeckOptions streamDeckOptions) {
            Port          = streamDeckOptions.Port;
            Uuid          = streamDeckOptions.Uuid;
            RegisterEvent = streamDeckOptions.RegisterEvent;
            Info          = streamDeckOptions.Info != null ? JObject.Parse(streamDeckOptions.Info) : null;

            StreamDeckConnection streamDeck = new StreamDeckConnection(Port, Uuid, RegisterEvent);
            _handler      = new Handler(this, streamDeck);
            DeviceManager = new DeviceManager(streamDeck);
            ButtonManager = new ActionButtonManager(streamDeck);
            VbanManager   = new VbanManager();

            streamDeck.OnConnected    += (sender, eventArgs) => _connectEvent.Set();
            streamDeck.OnDisconnected += (sender, eventArgs) => _disconnectEvent.Set();

            streamDeck.OnDeviceDidConnect    += _handler.DeviceConnect;
            streamDeck.OnDeviceDidDisconnect += _handler.DeviceDisconnect;
            streamDeck.OnWillAppear          += _handler.WillAppear;
            streamDeck.OnWillDisappear       += _handler.WillDisappear;

            streamDeck.OnKeyDown      += _handler.KeyDown;
            //streamDeck.OnKeyUp        += _handler.KeyUp;
            streamDeck.OnSendToPlugin += _handler.SendToPlugin;

            streamDeck.Run();

            // Wait for up to 10 seconds to connect
            if (_connectEvent.WaitOne(TimeSpan.FromSeconds(10)))
                while (!_disconnectEvent.WaitOne(TimeSpan.FromMilliseconds(1000)))
                    RunTick();
        }

        public DeviceManager       DeviceManager { get; }
        public ActionButtonManager ButtonManager { get; }
        public VbanManager         VbanManager   { get; }

        private async void RunTick() {
            await _instancesLock.WaitAsync();
            _instancesLock.Release();
        }

        #region Static Init

        public static readonly List<Plugin> Instances = new List<Plugin>();

        private static void Main(string[] args) {
            for (int count = 0; count < args.Length; count++)
                if (args[count].StartsWith("-") && !args[count].StartsWith("--"))
                    args[count] = $"-{args[count]}";

            Parser parser = new Parser(with => {
                with.EnableDashDash            = true;
                with.CaseInsensitiveEnumValues = true;
                with.CaseSensitive             = false;
                with.IgnoreUnknownArguments    = true;
                with.HelpWriter                = Console.Error;
            });

            ParserResult<StreamDeckOptions> options = parser.ParseArguments<StreamDeckOptions>(args);
            options.WithParsed(streamDeckOptions => Instances.Add(new Plugin(streamDeckOptions)));
        }

        #endregion
    }
}