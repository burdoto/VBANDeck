using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CommandLine;
using Newtonsoft.Json.Linq;
using streamdeck_client_csharp;
using Vban;
using VBANDeck.Model;
using static System.Diagnostics.Debug;

namespace VBANDeck
{
    // ReSharper disable once InconsistentNaming
    public class Plugin
    {
        private readonly ManualResetEvent _connectEvent    = new ManualResetEvent(false);
        private readonly ManualResetEvent _disconnectEvent = new ManualResetEvent(false);

        private readonly Handler       _handler       = new Handler();
        private readonly SemaphoreSlim _instancesLock = new SemaphoreSlim(1);
        public readonly  JObject       Info;

        public readonly int                Port;
        public readonly string             RegisterEvent;
        public readonly string             Uuid;
        public          VBANStream<string> Vban;

        public Plugin(StreamDeckOptions streamDeckOptions)
        {
            Port          = streamDeckOptions.Port;
            Uuid          = streamDeckOptions.Uuid;
            RegisterEvent = streamDeckOptions.RegisterEvent;
            Info          = JObject.Parse(streamDeckOptions.Info);

            WriteLine("Initializing Connection with Port:{0} Uuid:{1} RegisterEvent:{2}", Port, Uuid, RegisterEvent);

            StreamDeckConnection streamDeck = new StreamDeckConnection(Port, Uuid, RegisterEvent);

            streamDeck.OnConnected    += (sender, eventArgs) => _connectEvent.Set();
            streamDeck.OnDisconnected += (sender, eventArgs) => _disconnectEvent.Set();

            streamDeck.OnSendToPlugin += _handler.SendToPlugin;
            streamDeck.OnKeyDown      += _handler.KeyDown;
            streamDeck.OnKeyUp        += _handler.KeyUp;

            WriteLine("Running connection ...");
            streamDeck.Run();

            // Wait for up to 10 seconds to connect
            if (_connectEvent.WaitOne(TimeSpan.FromSeconds(10)))
                while (!_disconnectEvent.WaitOne(TimeSpan.FromMilliseconds(1000)))
                    RunTick();
        }

        // Function runs every second, used to update UI
        private async void RunTick()
        {
            await _instancesLock.WaitAsync();
            try
            {
                WriteLine("RunTick");
            }
            finally
            {
                _instancesLock.Release();
            }
        }

        #region Static Init

        public static readonly List<Plugin> instances = new List<Plugin>();

        private static void Main(string[] args)
        {
            WriteLine("Launching Plugin ...");

            for (int count = 0; count < args.Length; count++)
            {
                if (args[count].StartsWith("-") && !args[count].StartsWith("--"))
                {
                    args[count] = $"-{args[count]}";
                }
            }

            Parser parser = new Parser((with) =>
            {
                with.EnableDashDash            = true;
                with.CaseInsensitiveEnumValues = true;
                with.CaseSensitive             = false;
                with.IgnoreUnknownArguments    = true;
                with.HelpWriter                = Console.Error;
            });

            ParserResult<StreamDeckOptions> options = parser.ParseArguments<StreamDeckOptions>(args);
            options.WithParsed(streamDeckOptions => instances.Add(new Plugin(streamDeckOptions)));

            WriteLine("Plugin Launched!");
        }

        #endregion
    }
}