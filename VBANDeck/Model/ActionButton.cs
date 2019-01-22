using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using streamdeck_client_csharp;

namespace VBANDeck.Model {
    public class ActionButtonManager {
        private readonly StreamDeckConnection _connection;
        private readonly Dictionary<string, ActionButton> _instances = new Dictionary<string, ActionButton>();

        protected internal ActionButtonManager(StreamDeckConnection connection) {
            _connection = connection;
        }

        public ActionButton GetOrCreate(string opaqueValue) {
            ActionButton actionButton;
            if (!_instances.ContainsKey(opaqueValue)) {
                actionButton = new ActionButton(_connection, opaqueValue);
                _instances.Add(opaqueValue, actionButton);
            }

            _instances.TryGetValue(opaqueValue, out actionButton);
            return actionButton;
        }

        public bool Destroy(string opaqueValue) {
            return _instances.Remove(opaqueValue);
        }
    }

    public class ActionButton : StreamDeckEntity {
        private const string Uninitialized = "ActionButton is not initialized!";

        private  ButtonModifier _modifier;
        protected internal int[]          Coord = new int[0];
        protected internal JObject        Sett  = null;
        protected internal uint           StateF;

        internal ActionButton(StreamDeckConnection conn, string context) : base(conn, context) {
            PartOfMultiAction = false;
        }

        #region Properties

        public uint State {
            get => StateF != -1 ? StateF : throw new NullReferenceException(Uninitialized);
            set => Conn.SetStateAsync(StateF = value, Context).Wait();
        }

        public bool PartOfMultiAction { get; internal set; }

        public int[] Coordinates => Coord.Length > 0 ? Coord : throw new NullReferenceException(Uninitialized);

        public JObject Settings => Sett ?? throw new NullReferenceException(Uninitialized);

        public ButtonModifier Modifier => _modifier ?? (_modifier = new ButtonModifier(this));

        #endregion
    }

    public class ButtonModifier {
        private static readonly JsonMergeSettings MergeSettings = new JsonMergeSettings {
                MergeArrayHandling     = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Ignore
        };

        private readonly ActionButton _actionButton;

        internal ButtonModifier(ActionButton actionButton) {
            _actionButton = actionButton;
        }

        public ButtonModifier DefineCoordinates(int x, int y) {
            _actionButton.Coord = new[] {x, y};
            return this;
        }

        public ButtonModifier DefineSettings(JObject newSettings) {
            _actionButton.Sett.Merge(newSettings, MergeSettings);
            return this;
        }

        public ButtonModifier DefineMultiAction(bool isMultiAction) {
            _actionButton.PartOfMultiAction = isMultiAction;
            return this;
        }

        public ButtonModifier DefineState(uint state) {
            _actionButton.StateF = state;
            return this;
        }
    }
}